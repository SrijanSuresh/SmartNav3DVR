// VRPlayerController.cs  (replace your existing file with this)
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class VRPlayerController : MonoBehaviour
{
    [Header("Refs")]
    public Transform headTransform;        // center-eye (driven by GyroHeadIS)

    [Header("FP Eye Renderers")]
    public Renderer[] fpEyeRenderers;

    public CameraRigManagerIS cameraRig;
    public Animator animator;

    private CharacterController cc;

    [Header("Movement")]
    public float moveSpeed = 1.5f;      // slower for VR comfort
    public float gravity = -9.81f;
    public float jumpHeight = 1.2f;
    public float maxFallY = -10f;

    [Header("Free Gaze (legacy)")]
    public float lookSpeed = 60f;       // still used for mouse/gamepad input if present
    private float cameraYaw = 0f;
    private float cameraPitch = 0f;

    [Header("TP Settings")]
    public Vector3 tpOffsetFromFP = new Vector3(0f, 1.3f, -2.5f);
    public float tpPositionSmooth = 0.1f;
    public float tpRotationSmooth = 0.1f;
    private Vector3 tpVelocity;

    [Header("Body Rotation")]
    [Tooltip("How quickly the body rotates to face head yaw")]
    public float bodyRotateSpeed = 8f;

    [Header("Gaze Teleport (optional)")]
    public bool enableGazeTeleport = false;
    public LayerMask stationLayer = ~0;         // layer mask for stations (set to a stations layer for performance)
    public float gazeDuration = 1.2f;          // dwell time to trigger
    public float gazeMaxDistance = 40f;
    public UnityEngine.UI.Image gazeProgressUI; // optional radial UI
    public float fallbackTeleportYOffset = 0.9f; // used if station has no TeleportStation component
    [Header("Body Rotation Controls")]
    public float manualRotateSpeed = 120f; // degrees per second when using arrows / joystick
    public float rightStickDeadzone = 0.2f; // to ignore tiny stick noise

    // internal
    Vector3 velocity;
    float gazeTimer = 0f;
    Transform currentGazeTarget = null;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        if (cameraRig == null) Debug.LogWarning("CameraRigManagerIS not assigned.");

        if (headTransform != null)
        {
            cameraYaw = headTransform.eulerAngles.y;
            cameraPitch = headTransform.eulerAngles.x;
        }
    }

    void Update()
    {
        HandleGazeInput();      // keep existing mouse/gamepad yaw/pitch controls for desktop/dev
        HandleRotationInput();  // <- add this line
        HandleMovement();
        HandleAvatarRotation();  // <- changed: now rotates toward head yaw always
        HandleCameraPosition();
        HandleOutOfMap();
        UpdateEyeVisibility();

        if (enableGazeTeleport)
            HandleGazeTeleport(); // optional dwell-based teleport
    }

    // ---------- existing systems (unchanged logic overall) ----------
    void HandleGazeInput()
    {
        Vector2 lookInput = Vector2.zero;

        if (Mouse.current != null) lookInput = Mouse.current.delta.ReadValue();
        if (Gamepad.current != null) lookInput += Gamepad.current.rightStick.ReadValue() * 5f;

        cameraYaw += lookInput.x * lookSpeed * Time.deltaTime;
        cameraPitch -= lookInput.y * lookSpeed * Time.deltaTime;
        cameraPitch = Mathf.Clamp(cameraPitch, -89f, 89f);

        // Reset camera
        if ((Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.rightStickButton.wasPressedThisFrame))
        {
            if (headTransform != null)
            {
                cameraYaw = headTransform.eulerAngles.y;
                cameraPitch = headTransform.eulerAngles.x;
            }
        }
    }

    void HandleMovement()
    {
        Vector2 input = Vector2.zero;
        if (Keyboard.current != null)
        {
            input.x = (Keyboard.current.aKey.isPressed ? -1 : 0) + (Keyboard.current.dKey.isPressed ? 1 : 0);
            input.y = (Keyboard.current.wKey.isPressed ? 1 : 0) + (Keyboard.current.sKey.isPressed ? -1 : 0);
        }
        if (Gamepad.current != null) input += Gamepad.current.leftStick.ReadValue();

        Vector3 forward = headTransform.forward;
        forward.y = 0;
        forward.Normalize();
        Vector3 right = headTransform.right;
        right.y = 0;
        right.Normalize();

        Vector3 move = (forward * input.y + right * input.x);
        if (move.sqrMagnitude > 1f) move.Normalize();
        move *= moveSpeed;

        // Jump
        bool jumpPressed = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                           || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
        if (jumpPressed && cc.isGrounded) velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        if (cc.isGrounded && velocity.y < 0) velocity.y = -0.5f;
        velocity.y += gravity * Time.deltaTime;

        cc.Move((move + velocity) * Time.deltaTime);

        // Animator
        if (animator != null)
        {
            float speed = new Vector2(input.x, input.y).magnitude;
            animator.SetFloat("Speed", speed);
        }
    }

    // ---------- UPDATED: always rotate body toward head yaw (works FP & TP) ----------
    void HandleAvatarRotation()
    {
        if (headTransform == null) return;

    if (headTransform == null) return;

    // 1️⃣ Get head yaw (gaze/gyro)
    Vector3 lookDir = headTransform.forward;
    lookDir.y = 0f;
    if (lookDir.sqrMagnitude < 1e-6f) return;
    lookDir.Normalize();
    Quaternion gazeRot = Quaternion.LookRotation(lookDir);

    // 2️⃣ Combine with manual yaw already applied
    // Manual yaw is already applied via HandleRotationInput() on transform.rotation
    // So we just want to smoothly rotate toward gaze target
    Quaternion targetRot = Quaternion.Slerp(transform.rotation, gazeRot, Mathf.Clamp01(bodyRotateSpeed * Time.deltaTime));

    transform.rotation = targetRot;
    }

    void HandleCameraPosition()
    {
        if (cameraRig == null) return;

        if (cameraRig.isFirstPerson)
        {
            // FP: camera follows head position
            Vector3 targetPos = cameraRig.fpAnchor != null ? cameraRig.fpAnchor.position : headTransform.position;
            cameraRig.stereoRig.position = targetPos;

            // Free gaze applied
            cameraRig.stereoRig.rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
        }
        else
        {
            // TP: follow FP anchor smoothly
            Vector3 fpPos = cameraRig.fpAnchor.position;
            Vector3 desiredPos = fpPos + tpOffsetFromFP;

            Vector3 origin = playerRootPositionGuess(); // use player root position
            Vector3 dir = desiredPos - origin;
            float dist = dir.magnitude;
            dir.Normalize();
            if (Physics.SphereCast(origin, 0.25f, dir, out RaycastHit hit, dist, cameraRig.collideMask))
                desiredPos = hit.point - dir * 0.3f;

            cameraRig.stereoRig.position = Vector3.SmoothDamp(cameraRig.stereoRig.position, desiredPos, ref tpVelocity, tpPositionSmooth);

            // Look at FP
            Vector3 lookTarget = headTransform.position + Vector3.up * 0.1f;
            Quaternion targetRot = Quaternion.LookRotation(lookTarget - cameraRig.stereoRig.position);
            cameraRig.stereoRig.rotation = Quaternion.Slerp(cameraRig.stereoRig.rotation, targetRot, tpRotationSmooth);
        }
    }

    // helper to provide origin for spherecast (slightly above root)
    Vector3 playerRootPositionGuess()
    {
        return transform.position + Vector3.up * 1f;
    }

    void UpdateEyeVisibility()
    {
        if (fpEyeRenderers == null) return;

        bool showEyes = cameraRig != null && cameraRig.isFirstPerson;

        foreach (var r in fpEyeRenderers)
        {
            if (r != null) r.enabled = showEyes;
        }
    }

    void HandleOutOfMap()
    {
        if (transform.position.y < maxFallY && cameraRig != null && cameraRig.fpAnchor != null)
        {
            cc.enabled = false;
            transform.position = cameraRig.fpAnchor.position + Vector3.up * 0.1f;
            cc.enabled = true;
            velocity = Vector3.zero;
        }
    }

    // ---------- Gaze Teleport implementation (optional) ----------
    void HandleGazeTeleport()
    {
        if (headTransform == null || !enableGazeTeleport) return;

        Vector3 origin = headTransform.position;
        Vector3 dir = headTransform.forward;

        Debug.DrawRay(origin, dir * gazeMaxDistance, Color.green);

        if (Physics.Raycast(origin, dir, out RaycastHit hit, gazeMaxDistance, stationLayer))
        {
            Transform hitTf = hit.collider.transform;
            TeleportStation station = hit.collider.GetComponentInParent<TeleportStation>();
            Transform stationTf = station != null ? station.transform : hitTf;

            if (currentGazeTarget != stationTf)
            {
                currentGazeTarget = stationTf;
                gazeTimer = 0f;
            }

            gazeTimer += Time.deltaTime;
            UpdateGazeUI(gazeTimer / Mathf.Max(0.0001f, gazeDuration));

            if (gazeTimer >= gazeDuration)
            {
                // perform teleport
                if (station != null)
                {
                    TeleportToStation(station);
                }
                else
                {
                    Vector3 targetPos = hit.point + Vector3.up * fallbackTeleportYOffset;
                    SafeTeleportTo(targetPos, transform.rotation);
                }

                gazeTimer = 0f;
                currentGazeTarget = null;
                UpdateGazeUI(0f);
            }
            return;
        }

        // miss
        gazeTimer = 0f;
        currentGazeTarget = null;
        UpdateGazeUI(0f);
    }

    void UpdateGazeUI(float normalized)
    {
        if (gazeProgressUI != null)
            gazeProgressUI.fillAmount = Mathf.Clamp01(normalized);
    }
    // Rotate player root with keyboard arrows or right-stick X.
    void HandleRotationInput()
    {
        float inputX = 0f;

        // Keyboard arrows (left/right)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.isPressed) inputX -= 1f;
            if (Keyboard.current.rightArrowKey.isPressed) inputX += 1f;
        }

        // Gamepad right stick X
        if (Gamepad.current != null)
        {
            var rs = Gamepad.current.rightStick.ReadValue();
            if (Mathf.Abs(rs.x) > rightStickDeadzone) inputX += rs.x;
        }

        if (Mathf.Abs(inputX) < 1e-4f) return;

        // compute yaw delta in degrees for this frame
        float yawDelta = inputX * manualRotateSpeed * Time.deltaTime;

        // rotate player root (this transform) around Y
        transform.Rotate(0f, yawDelta, 0f, Space.World);

        // Keep cameraYaw in sync so free-look camera doesn't jump
        cameraYaw += yawDelta;
    }

    public void TeleportToStation(TeleportStation station)
    {
        if (station == null) return;
        Vector3 pos = station.GetTargetPosition();
        Quaternion rot = station.snapRotation ? station.GetTargetRotation() : transform.rotation;
        SafeTeleportTo(pos, rot);
    }

    void SafeTeleportTo(Vector3 pos, Quaternion rot)
    {
        if (cc != null) cc.enabled = false;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        transform.position = pos;
        transform.rotation = rot;

        if (rb != null) rb.isKinematic = false;
        if (cc != null) cc.enabled = true;
    }
}
