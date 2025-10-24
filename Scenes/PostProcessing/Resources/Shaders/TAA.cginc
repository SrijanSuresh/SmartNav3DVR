using UnityEngine;

public class MasterVRDemo : MonoBehaviour
{
    [Header("Stereo Heads")]
    public Transform defaultHead;          // parent of outside L/R cams
    public Transform cartHead;             // parent of cart L/R cams

    [Header("Left/Right Cameras (assign child cams)")]
    public Camera defaultLeft;             // raycasts use LEFT eye
    public Camera defaultRight;
    public Camera cartLeft;
    public Camera cartRight;

    [Header("Cart")]
    public EllipseMover cartMover;
    public Transform cartRoot;

    [Header("Gaze")]
    public float gazeSeconds = 2f;
    public float exitPitchDeg = 45f;
    public LayerMask gazeMask = ~0;

    [Header("UI")]
    public bool showHelp = true;
    public bool showDebug = true;

    // state
    float gazeTimer;
    bool insideCart;

    // editor mouse-look (head rotation)
    float yaw, pitch;
    public float mouseSensitivity = 3f;
    public bool mouseLookInEditor = true;

    void Start()
    {
        // Validate
        if (!defaultHead || !cartHead || !defaultLeft || !cartLeft)
        {
            Debug.LogError("Assign default/cart heads and at least the LEFT cameras.");
            enabled = false; return;
        }

        // find cart by tag if not provided
        if (!cartRoot)
        {
            var go = GameObject.FindGameObjectWithTag("Cart");
            if (go) cartRoot = go.transform;
        }

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;

        EnableDefaultHead();

        Debug.Log("Controls: 1=Outside, 2=Inside, T=Toggle cart move, R=Reset, H=Help, Esc=Unlock mouse");
    }

    void Update()
    {
        // ----- editor mouse look: rotate the ACTIVE HEAD -----
#if UNITY_EDITOR
        if (mouseLookInEditor)
        {
            var head = insideCart ? cartHead : defaultHead;
            if (head)
            {
                yaw   += Input.GetAxis("Mouse X") * mouseSensitivity;
                pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
                pitch  = Mathf.Clamp(pitch, -80f, 80f);
                head.localRotation = Quaternion.Euler(pitch, yaw, 0f);
            }
        }
#endif

        // ----- hotkeys -----
        if (Input.GetKeyDown(KeyCode.Alpha1)) EnterDefault();      // outside
        if (Input.GetKeyDown(KeyCode.Alpha2)) EnterCart();         // inside
        if (Input.GetKeyDown(KeyCode.T)) ToggleCartMove();
        if (Input.GetKeyDown(KeyCode.R)) ResetCart();
        if (Input.GetKeyDown(KeyCode.H)) showHelp = !showHelp;
        if (Input.GetKeyDown(KeyCode.Escape)) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }

        // ----- gaze logic (casts from LEFT eye) -----
        var leftCam = insideCart ? cartLeft : defaultLeft;

        if (!insideCart)
        {
            if (RayHitsCart(leftCam))
            {
                gazeTimer += Time.deltaTime;
                if (gazeTimer >= gazeSeconds) EnterCart();
            }
            else gazeTimer = 0f;
        }
        else
        {
            if (IsLookingDown(leftCam))
            {
                gazeTimer += Time.deltaTime;
                if (gazeTimer >= gazeSeconds) EnterDefault();
            }
            else gazeTimer = 0f;
        }
    }

    // -------- helpers ----------
    void EnableDefaultHead()
    {
        SetHeadActive(defaultHead, true);
        SetHeadActive(cartHead, false);
        EnsureOneAudioListener(defaultLeft);
        insideCart = false;
    }

    void EnableCartHead()
    {
        SetHeadActive(defaultHead, false);
        SetHeadActive(cartHead, true);
        EnsureOneAudioListener(cartLeft);
        insideCart = true;
    }

    void SetHeadActive(Transform head, bool on)
    {
        if (!head) return;
        head.gameObject.SetActive(on);
        var cams = head.GetComponentsInChildren<Camera>(true);
        foreach (var c in cams) c.enabled = on;
    }

    void EnsureOneAudioListener(Camera keepOn)
    {
        foreach (var al in FindObjectsOfType<AudioListener>(true)) al.enabled = false;
        var alKeep = keepOn ? keepOn.GetComponent<AudioListener>() : null;
        if (!alKeep && keepOn) alKeep = keepOn.gameObject.AddComponent<AudioListener>();
        if (alKeep) alKeep.enabled = true;
    }

    bool RayHitsCart(Camera cam)
    {
        if (!cam) return false;
        Vector3 o = cam.transform.position;
        Vector3 d = cam.transform.forward;

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
        if (!cam) return false;
        Vector3 f = cam.transform.forward;
        float pitchDeg = Vector3.Angle(new Vector3(f.x, 0f, f.z).normalized, f); // 0..90
        return (f.y < 0f) && (pitchDeg >= exitPitchDeg);
    }

    void EnterCart()
    {
        gazeTimer = 0f;
        EnableCartHead();
        if (cartMover) cartMover.moveEnabled = true;
    }

    void EnterDefault()
    {
        gazeTimer = 0f;
        EnableDefaultHead();
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

    void OnGUI()
    {
        if (showDebug)
        {
            GUI.Label(new Rect(10,10,520,22),
                $"InsideCart: {insideCart}  Timer: {gazeTimer:F2}/{gazeSeconds:F2}  CartMoving: {(cartMover && cartMover.moveEnabled)}  TimeScale: {Time.timeScale:F1}");
        }
        if (showHelp)
        {
            string help =
                "H toggles this help\n" +
                "Mouse (Editor) = look\n" +
                "1 = Outside   2 = Cart\n" +
                "T = Toggle cart movement\n" +
                "R = Reset cart & go outside\n" +
                "Gaze: Look at cart to enter; look down to exit\n";
            GUI.Box(new Rect(10, 34, 400, 110), help);
        }
    }
}
