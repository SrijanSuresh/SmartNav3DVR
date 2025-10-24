using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRigManagerIS : MonoBehaviour
{
    [Header("Main refs")]
    public Transform stereoRig;
    public Transform playerRoot;
    public Transform headTransform;

    [Header("FP Anchor")]
    public Transform fpAnchor;

    [Header("TP Settings")]
    public Vector3 tpOffsetFromFP = new Vector3(0f, 1.3f, -2.5f);
    public float tpPositionSmooth = 8f;
    public float tpRotationSmooth = 8f;
    public float tpMinDistance = 0.6f;
    public float tpMaxDistance = 4.0f;

    [Header("Collision Avoidance (TP)")]
    public float spherecastRadius = 0.25f;
    public float spherecastClearance = 0.3f;
    public LayerMask collideMask = ~0;

    [Header("FP Options")]
    public GameObject fpEyeHUD;
    public Renderer[] hideRenderersInFP;

    [Header("Look Settings")]
    public float lookSensitivity = 1.5f;
    public float tpPitchClampMin = -45f;
    public float tpPitchClampMax = 75f;

    [HideInInspector] public bool isFirstPerson = true;

    // Internal
    Vector3 tpVelocity;
    float tpYaw;
    float tpPitch;
    Quaternion fpInitialRotation;

    void Start()
    {
        if (stereoRig == null || playerRoot == null)
        {
            Debug.LogError("[CameraRigManagerIS] Assign stereoRig and playerRoot.");
            enabled = false;
            return;
        }

        // store initial FP rotation for R reset
        fpInitialRotation = fpAnchor != null ? fpAnchor.rotation : stereoRig.rotation;

        if (fpAnchor != null) SetFirstPerson();
        else SetThirdPerson();
    }

    void Update()
    {
        HandleInput();

        if (isFirstPerson)
            UpdateFirstPerson();
        else
            UpdateThirdPerson();
    }

    void HandleInput()
    {
        var kb = Keyboard.current;
        var gp = Gamepad.current;

        if (kb != null)
        {
            if (kb.fKey.wasPressedThisFrame) SetFirstPerson();
            if (kb.tKey.wasPressedThisFrame) SetThirdPerson();
            if (kb.vKey.wasPressedThisFrame) ToggleView();
            if (kb.rKey.wasPressedThisFrame) ResetFPYaw();
        }

        if (gp != null)
        {
            // 'Y' button (usually buttonNorth)
            if (gp.buttonNorth.wasPressedThisFrame) ToggleView();

            // D-Pad Up/Down
            if (gp.dpad.up.wasPressedThisFrame) SetThirdPerson();
            if (gp.dpad.down.wasPressedThisFrame) SetFirstPerson();
        }
    }

    void ToggleView()
    {
        if (isFirstPerson) SetThirdPerson();
        else SetFirstPerson();
    }

    void ResetFPYaw()
    {
        if (isFirstPerson)
        {
            stereoRig.rotation = fpInitialRotation;
        }
    }

    public void SetFirstPerson()
    {
        isFirstPerson = true;

        Vector3 pos = fpAnchor != null ? fpAnchor.position : (headTransform != null ? headTransform.position : playerRoot.position);
        stereoRig.position = pos;

        // full FP rotation = head rotation
        if (headTransform != null)
            stereoRig.rotation = headTransform.rotation;

        if (fpEyeHUD != null) fpEyeHUD.SetActive(true);
        SetBodyRenderers(false);
    }

    public void SetThirdPerson()
    {
        isFirstPerson = false;
        if (fpEyeHUD != null) fpEyeHUD.SetActive(false);
        SetBodyRenderers(true);

        // initialize TP yaw/pitch based on current rotation
        Vector3 euler = stereoRig.rotation.eulerAngles;
        tpYaw = euler.y;
        tpPitch = euler.x;
    }

    void UpdateFirstPerson()
    {
        // move with player
        Vector3 pos = fpAnchor != null ? fpAnchor.position : playerRoot.position;
        stereoRig.position = pos;

        // full head rotation (gaze)
        if (headTransform != null)
            stereoRig.rotation = headTransform.rotation;
    }

    void UpdateThirdPerson()
    {
        if (fpAnchor == null) return;

        // TP camera target
        Vector3 fpPos = fpAnchor.position;
        Vector3 desired = fpPos + tpOffsetFromFP;

        Vector3 origin = playerRoot.position + Vector3.up * 1f;
        Vector3 dir = desired - origin;
        float dist = dir.magnitude;
        dir.Normalize();

        if (Physics.SphereCast(origin, spherecastRadius, dir, out RaycastHit hit, dist, collideMask))
            desired = hit.point - dir * spherecastClearance;

        float d = Vector3.Distance(desired, fpPos);
        if (d < tpMinDistance) desired = fpPos + (desired - fpPos).normalized * tpMinDistance;
        if (d > tpMaxDistance) desired = fpPos + (desired - fpPos).normalized * tpMaxDistance;

        stereoRig.position = Vector3.SmoothDamp(stereoRig.position, desired, ref tpVelocity, Mathf.Max(0.0001f, 1f / tpPositionSmooth));

        // free look (mouse/gamepad)
        Vector2 lookInput = Vector2.zero;
        if (Mouse.current != null) lookInput += Mouse.current.delta.ReadValue();
        if (Gamepad.current != null) lookInput += Gamepad.current.rightStick.ReadValue() * 10f;

        tpYaw += lookInput.x * lookSensitivity * Time.deltaTime;
        tpPitch -= lookInput.y * lookSensitivity * Time.deltaTime;
        tpPitch = Mathf.Clamp(tpPitch, tpPitchClampMin, tpPitchClampMax);

        Quaternion lookRot = Quaternion.Euler(tpPitch, tpYaw, 0f);
        stereoRig.rotation = lookRot;
    }

    void SetBodyRenderers(bool visible)
    {
        if (hideRenderersInFP == null) return;
        foreach (var r in hideRenderersInFP)
        {
            if (r != null) r.enabled = visible;
        }
    }
}
