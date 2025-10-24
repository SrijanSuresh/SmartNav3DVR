using UnityEngine;

public class ReticleDot : MonoBehaviour
{
    [Header("Cameras (assign LEFT eyes is fine)")]
    public Camera defaultCam;   // outside (active when cartCam is disabled)
    public Camera cartCam;      // inside cart (active when enabled)

    [Header("Cart Target")]
    public Transform cartRoot;        // the Cart root (must have a collider)
    public LayerMask gazeMask = ~0;   // raycast mask

    [Header("Gaze Timing")]
    public float gazeSeconds = 2f;    // time to “confirm” stare
    public float exitPitchDeg = 45f;  // look-down threshold (inside cart)

    [Header("Reticle (2D GUI)")]
    public int size = 10;             // dot size in pixels
    public Color idleColor = Color.white;
    public Color hitColor  = Color.green;
    [Range(0f,1f)] public float alpha = 0.9f;

    [Header("Loader (3D in world)")]
    public GameObject loaderPrefab;   // simple prefab (cube, ring, icon)
    public float loaderDistance = 1.2f;
    public Vector3 loaderScaleIdle = new Vector3(0.05f, 0.05f, 0.05f);
    public Vector3 loaderScaleFull = new Vector3(0.12f, 0.12f, 0.12f);
    public Color loaderIdleColor = new Color(1,1,1,0.35f);
    public Color loaderFullColor = new Color(0.2f,1f,0.2f,1f);

    // state
    bool _isTargeting;   // true when staring at cart (outside) or looking down (inside)
    float _timer;        // counts up to gazeSeconds
    GameObject _loaderInstance;
    Renderer _loaderRend;

    // ---------- Unity ----------
    void Awake()
    {
        if (loaderPrefab)
        {
            _loaderInstance = Instantiate(loaderPrefab);
            _loaderInstance.SetActive(false);
            _loaderRend = _loaderInstance.GetComponentInChildren<Renderer>();
        }
    }

    void Update()
    {
        var cam = (cartCam && cartCam.enabled) ? cartCam : defaultCam;
        if (!cam) { HideLoader(); return; }

        // Determine whether we’re “loading”
        if (cartCam && cartCam.enabled)
        {
            // inside cart: stare DOWN to exit
            _isTargeting = IsLookingDown(cam);
        }
        else
        {
            // outside: stare at the cart (any collider under cartRoot)
            _isTargeting = RayHitsCart(cam);
        }

        if (_isTargeting)
        {
            _timer += Time.deltaTime;
            ShowAndUpdateLoader(cam, Mathf.Clamp01(_timer / Mathf.Max(0.0001f, gazeSeconds)));
        }
        else
        {
            _timer = 0f;
            HideLoader();
        }
    }

    void OnGUI()
    {
        // style
        var style = new GUIStyle(GUI.skin.label) {
            alignment = TextAnchor.MiddleCenter,
            fontSize  = Mathf.Max(8, size)
        };
        var c = _isTargeting ? hitColor : idleColor;
        c.a = alpha;
        style.normal.textColor = c;

        float halfW = Screen.width * 0.5f;
        float cxL   = Screen.width * 0.25f;  // center of left eye
        float cxR   = Screen.width * 0.75f;  // center of right eye
        float cy    = Screen.height * 0.5f;
        float w     = size * 2f, h = size * 2f;

        // draw two dots (one per eye)
        GUI.Label(new Rect(cxL - size, cy - size, w, h), "●", style);
        GUI.Label(new Rect(cxR - size, cy - size, w, h), "●", style);

        // OPTIONAL: if you keep a single loader in world space, you do not need two.
        // Both eyes will see it correctly due to stereo rendering.
    }

    // ---------- helpers ----------
    bool RayHitsCart(Camera cam)
    {
        if (!cartRoot) return false;

        Vector3 o = cam.transform.position;
        Vector3 d = cam.transform.forward;

        var hits = Physics.RaycastAll(o, d, 200f, gazeMask);
        foreach (var h in hits)
        {
            if (!h.collider) continue;
            var t = h.collider.transform;
            if (t == cartRoot || t.IsChildOf(cartRoot))
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

    void ShowAndUpdateLoader(Camera cam, float t) // t = 0..1
    {
        if (!_loaderInstance) return;

        // position loader in front of camera
        _loaderInstance.SetActive(true);
        _loaderInstance.transform.position = cam.transform.position + cam.transform.forward * loaderDistance;
        _loaderInstance.transform.LookAt(cam.transform);
        _loaderInstance.transform.Rotate(0f, 180f, 0f, Space.Self); // face camera

        // scale blends from idle to full
        _loaderInstance.transform.localScale = Vector3.Lerp(loaderScaleIdle, loaderScaleFull, t);

        // color blend
        if (_loaderRend && _loaderRend.material)
        {
            var col = Color.Lerp(loaderIdleColor, loaderFullColor, t);
            // ensure material supports color (Standard/URP Lit: use _BaseColor)
            if (_loaderRend.material.HasProperty("_BaseColor"))
                _loaderRend.material.SetColor("_BaseColor", col);
            else if (_loaderRend.material.HasProperty("_Color"))
                _loaderRend.material.color = col;
        }
    }

    void HideLoader()
    {
        if (_loaderInstance) _loaderInstance.SetActive(false);
    }
}
