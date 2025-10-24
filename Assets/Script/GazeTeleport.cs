using UnityEngine;

public class MasterVRDemo : MonoBehaviour
{
    [Header("Cameras")]
    public Camera defaultCam;   // outside view (enabled at start)
    public Camera cartCam;      // inside cart (disabled at start)

    [Header("Cart")]
    public EllipseMover cartMover; // your mover script on Cart
    public Transform cartRoot;     // Cart root (has BoxCollider, Tag=Cart)

    [Header("Gaze")]
    public float gazeSeconds = 2f;      // set to 5s later
    public float exitPitchDeg = 45f;    // look down this much to exit
    public LayerMask gazeMask = ~0;     // Everything for now

    [Header("UI")]
    public bool showHelp = true;        // toggle with H
    public bool showDebug = true;       // shows timer/state

    float gazeTimer;
    bool insideCart;

    // --- simple mouse look (works in Editor for both cams) ---
    float yaw, pitch;
    public float mouseSensitivity = 3f;

    void Start()
    {
        // Safety: cameras
        if (!defaultCam || !cartCam) { Debug.LogError("Assign both cameras to MasterVRDemo."); enabled = false; return; }
        EnableDefaultCam();

        // Force TimeScale to 1 in case you accidentally paused
        Time.timeScale = 1f;

        // Try find cart by tag if not assigned
        if (!cartRoot)
        {
            var go = GameObject.FindGameObjectWithTag("Cart");
            if (go) cartRoot = go.transform;
        }

        Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
        Debug.Log("Controls: 1 = Outside, 2 = Inside (force).  T = Toggle cart move.  R = Reset cart.  H = Help.  Esc = unlock mouse.");
    }

    void Update()
    {
        // --- MOUSE LOOK on the active cam ---
        var cam = insideCart ? cartCam : defaultCam;
        if (cam)
        {
            yaw   += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch  = Mathf.Clamp(pitch, -80f, 80f);
            cam.transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        // --- HOTKEYS (so you can test even if gaze not working yet) ---
        // if (Input.GetKeyDown(KeyCode.Alpha1)) EnterDefault();      // force outside
        // if (Input.GetKeyDown(KeyCode.Alpha2)) EnterCart();         // force inside
        // if (Input.GetKeyDown(KeyCode.T)) ToggleCartMove();         // start/stop mover
        // if (Input.GetKeyDown(KeyCode.R)) ResetCart();              // reset mover + go outside
        // if (Input.GetKeyDown(KeyCode.H)) showHelp = !showHelp;
        // if (Input.GetKeyDown(KeyCode.Escape)) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }

        // --- GAZE LOGIC (works once you can aim with mouse) ---
        if (!insideCart)
        {
            if (RayHitsCart(defaultCam))
            {
                gazeTimer += Time.deltaTime;
                if (gazeTimer >= gazeSeconds) EnterCart();
            }
            else gazeTimer = 0f;
        }
        else
        {
            if (IsLookingDown(cartCam))
            {
                gazeTimer += Time.deltaTime;
                if (gazeTimer >= gazeSeconds) EnterDefault();
            }
            else gazeTimer = 0f;
        }
    }

    // ---------- helpers ----------
    void EnableDefaultCam()
    {
        defaultCam.enabled = true;
        cartCam.enabled = false;
        EnsureOneAudioListener(defaultCam);
        insideCart = false;
    }

    void EnableCartCam()
    {
        defaultCam.enabled = false;
        cartCam.enabled = true;
        EnsureOneAudioListener(cartCam); // optional; we keep audio on default usually
        insideCart = true;
    }

    void EnsureOneAudioListener(Camera keepOn)
    {
        foreach (var al in FindObjectsOfType<AudioListener>(true)) al.enabled = false;
        var alKeep = keepOn.GetComponent<AudioListener>();
        if (!alKeep) alKeep = keepOn.gameObject.AddComponent<AudioListener>();
        alKeep.enabled = true;
    }

    bool RayHitsCart(Camera cam)
    {
        Vector3 o = cam.transform.position;
        Vector3 d = cam.transform.forward;

        // RaycastAll so ground/buildings don't block the cart collider
        var hits = Physics.RaycastAll(o, d, 200f, gazeMask);
        foreach (var h in hits)
        {
            if (cartRoot && (h.collider.transform == cartRoot || h.collider.transform.IsChildOf(cartRoot)))
                return true;
            if (!cartRoot && h.collider.CompareTag("Cart"))
                return true;
        }
        return false;
    }

    bool IsLookingDown(Camera cam)
    {
        Vector3 f = cam.transform.forward;
        float pitchDeg = Vector3.Angle(new Vector3(f.x, 0f, f.z).normalized, f); // 0..90
        return (f.y < 0f) && (pitchDeg >= exitPitchDeg);
    }

    void EnterCart()
    {
        gazeTimer = 0f;
        EnableCartCam();
        if (cartMover) cartMover.moveEnabled = true;
    }

    void EnterDefault()
    {
        gazeTimer = 0f;
        EnableDefaultCam();
        if (cartMover) cartMover.ResetCart();
    }

    void ToggleCartMove()
    {
        if (!cartMover) return;
        cartMover.moveEnabled = !cartMover.moveEnabled;
    }

    void ResetCart()
    {
        if (cartMover) cartMover.ResetCart();
        EnterDefault();
    }

}
