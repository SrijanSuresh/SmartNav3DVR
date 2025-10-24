// MasterVRDemoIS.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class MasterVRDemoIS : MonoBehaviour
{
    [Header("Stereo Heads (parents you rotate)")]
    public Transform defaultHead;
    public Transform cartHead;

    [Header("Left/Right Cameras (assign child cams)")]
    public Camera defaultLeft;   // used for raycasts & audio
    public Camera defaultRight;
    public Camera cartLeft;
    public Camera cartRight;

    [Header("Cart")]
    public EllipseMover cartMover;
    public Transform cartRoot;   // Tag=Cart or assign explicitly

    [Header("Gaze")]
    public float gazeSeconds = 2f;
    public float exitPitchDeg = 45f;
    public LayerMask gazeMask = ~0;

    [Header("UI")]
    public bool showHelp = true;
    public bool showDebug = true;

    float gazeTimer;
    bool insideCart;

    void Start()
    {
        // Validate refs
        if (!defaultHead || !cartHead || !defaultLeft || !cartLeft)
        {
            Debug.LogError("Assign default/cart heads and at least the LEFT cameras.");
            enabled = false; return;
        }

        if (!cartRoot)
        {
            var go = GameObject.FindGameObjectWithTag("Cart");
            if (go) cartRoot = go.transform;
        }

        Time.timeScale = 1f;
        EnableDefaultHead();

        Debug.Log("Keys: 1=Outside, 2=Inside, T=Toggle, R=Reset, H=Help");
    }

    void Update()
    {
        // ----- Hotkeys via Input System -----
        var kb = Keyboard.current;
        if (kb != null)
        {
            // if (kb.digit1Key.wasPressedThisFrame) EnterDefault();
            // if (kb.digit2Key.wasPressedThisFrame) EnterCart();
            // if (kb.tKey.wasPressedThisFrame) ToggleCartMove();
            // if (kb.rKey.wasPressedThisFrame) ResetCart();
            // if (kb.hKey.wasPressedThisFrame) showHelp = !showHelp;
        }

        // ----- Gaze logic (casts from LEFT eye) -----
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

    // ---------- helpers ----------
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
        foreach (var c in head.GetComponentsInChildren<Camera>(true))
            c.enabled = on;
    }

    void EnsureOneAudioListener(Camera keepOn)
    {
        // Disable all existing AudioListeners
        foreach (var listener in Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
        {
            listener.enabled = false;
        }

        if (!keepOn) return;

        // Add or enable AudioListener on the camera we want to keep
        var keepListener = keepOn.GetComponent<AudioListener>();
        if (!keepListener) keepListener = keepOn.gameObject.AddComponent<AudioListener>();
        keepListener.enabled = true;
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
        if (cartMover) cartMover.moveEnabled = !cartMover.moveEnabled;
    }

    void ResetCart()
    {
        if (cartMover) cartMover.ResetCart();
        EnterDefault();
    }

    // void OnGUI()
    // {
    //     if (showDebug)
    //     {
    //         GUI.Label(new Rect(10,10,540,22),
    //             $"InsideCart: {insideCart}  Timer: {gazeTimer:F2}/{gazeSeconds:F2}  CartMoving: {(cartMover && cartMover.moveEnabled)}");
    //     }
    //     if (showHelp)
    //     {
    //         string help = "H toggle help\n1 Outside  2 Inside\nT Toggle cart move\nR Reset cart\nGaze: look at cart to enter; look down to exit";
    //         GUI.Box(new Rect(10, 34, 360, 90), help);
    //     }
    // }
}
