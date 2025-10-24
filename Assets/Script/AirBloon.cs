using UnityEngine;

public class BalloonLoopLerp : MonoBehaviour
{
    [Header("Path (world-space waypoints; NOT children of BalloonRoot)")]
    [SerializeField] Transform[] waypoints;

    [Header("Motion")]
    [SerializeField, Min(0.1f)] float secondsPerSegment = 4f;
    [SerializeField] bool faceDirection = true;
    [SerializeField] float bobHeight = 0.5f;
    [SerializeField] float bobSpeed = 0.5f;

    int segCount;
    float t;
    Vector3 lastPos;

    void Awake()
    {
        segCount = (waypoints == null) ? 0 : waypoints.Length;
        if (segCount < 2)
        {
            Debug.LogError("[BalloonLoopLerp] Need at least 2 waypoints.");
            enabled = false;
            return;
        }

        // Snap to first WP on start
        transform.position = waypoints[0].position;
        lastPos = transform.position;
    }

    void Update()
    {
        if (segCount < 2) return;

        // progress along loop in segment space
        t += Time.deltaTime / Mathf.Max(0.1f, secondsPerSegment);
        float loop = t % segCount;
        int i = Mathf.FloorToInt(loop);
        int j = (i + 1) % segCount;
        float u = loop - i; // 0..1 within this segment

        // world-space positions (waypoints must not be children of this transform)
        Vector3 a = waypoints[i].position;
        Vector3 b = waypoints[j].position;

        Vector3 p = Vector3.Lerp(a, b, u);

        // vertical bob
        if (bobHeight > 0f)
            p.y += Mathf.Sin(Time.time * bobSpeed * Mathf.PI * 2f) * bobHeight;

        if (faceDirection)
        {
            Vector3 dir = (p - lastPos);
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir, Vector3.up), 0.2f);
        }

        transform.position = p;
        lastPos = p;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            var a = waypoints[i].position;
            var b = waypoints[(i + 1) % waypoints.Length].position;
            Gizmos.DrawSphere(a, 0.2f);
            Gizmos.DrawLine(a, b);
        }
    }
#endif
}
