// EllipseMover.cs
// Attach to Cart (root). Assign center = PathCenter (empty at pond center).
using UnityEngine;

public class EllipseMover : MonoBehaviour
{
    [Header("Ellipse path")]
    public Transform center;           // Put an Empty (PathCenter) at the pond center and assign it here
    public float radiusX = 12f;        // Half of pond/path X scale
    public float radiusZ = 8.5f;       // Half of pond/path Z scale
    [Tooltip("Revolutions per second (0.1 = one lap in ~10s)")]
    public float speedRevsPerSec = 0.08f;

    [Header("Orientation")]
    [Tooltip("Use this if the cart looks like sliding sideways. Try 90, -90, or 180.")]
    public float headingOffsetDeg = 90f;

    [Header("Headlights (optional)")]
    public Light headlightL;
    public Light headlightR;

    [Header("State")]
    public bool moveEnabled = false;   // Turn true after teleport-in (or for testing)
    [Range(0f, 1f)] public float startT = 0f;  // Where on the loop to start (0..1)
    private float t;
    private float fixedY;              // Keep constant height

    void Start()
    {
        fixedY = transform.position.y; // remember current Y
        t = startT;
        SnapToEllipse(t);
    }

    void Update()
    {
        if (!center) return;

        if (moveEnabled)
        {
            t += speedRevsPerSec * Time.deltaTime;
            if (t > 1f) t -= 1f;
        }

        float ang = t * Mathf.PI * 2f;

        // Position on ellipse
        Vector3 pos = new Vector3(
            center.position.x + radiusX * Mathf.Cos(ang),
            fixedY,
            center.position.z + radiusZ * Mathf.Sin(ang)
        );
        transform.position = pos;

        // Face tangent direction
        Vector3 tangent = new Vector3(
            -radiusX * Mathf.Sin(ang),
             0f,
             radiusZ * Mathf.Cos(ang)
        );
        if (tangent.sqrMagnitude > 1e-6f)
        {
            var look = Quaternion.LookRotation(tangent.normalized, Vector3.up)
                     * Quaternion.Euler(0f, headingOffsetDeg, 0f);
            transform.rotation = look;
        }
    }

    public void SetHeadlights(bool on)
    {
        if (headlightL) headlightL.enabled = on;
        if (headlightR) headlightR.enabled = on;
    }

    public void ResetCart()
    {
        moveEnabled = false;
        t = startT;
        SnapToEllipse(t);
    }

    private void SnapToEllipse(float tt)
    {
        if (!center) return;
        float ang = tt * Mathf.PI * 2f;
        transform.position = new Vector3(
            center.position.x + radiusX * Mathf.Cos(ang),
            fixedY,
            center.position.z + radiusZ * Mathf.Sin(ang)
        );
        // initial facing = forward + heading offset
        transform.rotation = Quaternion.Euler(0f, headingOffsetDeg, 0f);
    }

    // Draw the path in Scene view when Cart is selected
    void OnDrawGizmosSelected()
    {
        if (!center) return;
        Gizmos.color = Color.cyan;
        const int seg = 128;
        Vector3 prev = Vector3.zero;
        for (int i = 0; i <= seg; i++)
        {
            float tt = (float)i / seg;
            float ang = tt * Mathf.PI * 2f;
            Vector3 p = new Vector3(
                center.position.x + radiusX * Mathf.Cos(ang),
                Application.isPlaying ? fixedY : transform.position.y,
                center.position.z + radiusZ * Mathf.Sin(ang)
            );
            if (i > 0) Gizmos.DrawLine(prev, p);
            prev = p;
        }
    }
}
