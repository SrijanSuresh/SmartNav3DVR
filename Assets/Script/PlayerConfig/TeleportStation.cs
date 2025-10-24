// TeleportStation.cs
using UnityEngine;

[DisallowMultipleComponent]
public class TeleportStation : MonoBehaviour
{
    [Tooltip("If set, teleport will place player's root at this exact transform position.")]
    public Transform teleportTarget;

    [Tooltip("Fallback offset used when no teleportTarget is assigned (useful to keep feet on ground).")]
    public Vector3 teleportOffset = new Vector3(0f, 0.9f, 0f);

    [Tooltip("If true, player will be rotated to match teleportTarget.rotation")]
    public bool snapRotation = true;

    // Optional: visual gizmo
    private void OnDrawGizmosSelected()
    {
        Vector3 pos = teleportTarget != null ? teleportTarget.position : transform.position + teleportOffset;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pos, 0.2f);
        Gizmos.DrawLine(transform.position, pos);
    }

    public Vector3 GetTargetPosition()
    {
        return teleportTarget != null ? teleportTarget.position : transform.position + teleportOffset;
    }

    public Quaternion GetTargetRotation()
    {
        return teleportTarget != null ? teleportTarget.rotation : transform.rotation;
    }
}
