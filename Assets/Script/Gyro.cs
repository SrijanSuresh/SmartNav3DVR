// GyroHeadIS.cs (Input System only)
using UnityEngine;
using UnityEngine.InputSystem;

public class GyroHeadIS : MonoBehaviour
{
    [Tooltip("Right-mouse look in Editor (Input System Mouse).")]
    public bool mouseLookInEditor = true;

    [Range(0f, 1f)] public float smooth = 0.15f;

    Quaternion baseRot, lastRot;

    void OnEnable()
    {
        baseRot = transform.rotation;
        lastRot = baseRot;

        // Safely enable sensors if present (some platforms ignore this)
        if (AttitudeSensor.current != null) InputSystem.EnableDevice(AttitudeSensor.current);
        if (UnityEngine.InputSystem.Gyroscope.current != null) InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);
        if (Accelerometer.current != null) InputSystem.EnableDevice(Accelerometer.current);
        if (GravitySensor.current != null) InputSystem.EnableDevice(GravitySensor.current);
    }

    void Update()
    {
#if UNITY_EDITOR
        if (mouseLookInEditor && Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            Vector2 d = Mouse.current.delta.ReadValue();
            float yaw = d.x * 0.15f;    // tune sensitivity to taste
            float pitch = -d.y * 0.15f;

            transform.rotation = Quaternion.Euler(0, yaw, 0) * transform.rotation;
            transform.rotation = Quaternion.Euler(pitch, 0, 0) * transform.rotation;
            return;
        }
#endif
        // Read attitude via Input System
        if (AttitudeSensor.current == null) return;

        var a = AttitudeSensor.current.attitude.ReadValue();   // right-handed
        var q = new Quaternion(a.x, a.y, -a.z, -a.w);          // RH -> Unity LH
        var adjust = Quaternion.Euler(90, 0, 0);               // screen -> world

        var target = baseRot * (adjust * q);

        // Optional: reduce roll for comfort in Cardboard
        var e = target.eulerAngles;
        e.z = 0f;
        target = Quaternion.Euler(e);

        transform.rotation = Quaternion.Slerp(lastRot, target, 1f - smooth);
        lastRot = transform.rotation;

        // Optional recenter (works in Editor/with keyboard connected)
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
            baseRot = Quaternion.Inverse(adjust * q) * transform.rotation;
    }
}
