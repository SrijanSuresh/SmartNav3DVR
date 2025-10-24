using UnityEngine;
using UnityEngine.InputSystem;

public class GazePickupDebug : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupRange = 5f;
    public Transform arms; // Parent object for holding the cube
    public LayerMask pickableMask;
    public Key pickupKey = Key.E;

    public float holdHeight = 0.5f;  // Y offset above arms
    public float dropForwardOffset = 0.5f; // forward offset when dropped

    private GameObject heldObject;
    private Rigidbody heldRigidbody;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current[pickupKey].wasPressedThisFrame)
        {
            Debug.Log("Pickup key pressed!");
            if (heldObject == null)
                TryPickUp();
            else
                Drop();
        }
    }

    void TryPickUp()
    {
        Camera cam = Camera.main;
        if (!cam)
        {
            Debug.LogWarning("No main camera found!");
            return;
        }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * pickupRange, Color.green, 2f);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickableMask))
        {
            Debug.Log("Raycast hit: " + hit.collider.name);

            heldObject = hit.collider.gameObject;
            heldRigidbody = heldObject.GetComponent<Rigidbody>();

            if (heldRigidbody != null)
            {
                heldRigidbody.useGravity = false;
                heldRigidbody.isKinematic = true;
            }

            // Parent to arms and set position above arms
            heldObject.transform.SetParent(arms);
            heldObject.transform.localPosition = new Vector3(0f, holdHeight, 0f);
            heldObject.transform.localRotation = Quaternion.identity;

            // Ensure scale stays (1,1,1)
            heldObject.transform.localScale = Vector3.one;

            Debug.Log("Picked up: " + heldObject.name);
        }
        else
        {
            Debug.Log("Raycast did not hit any pickable object!");
        }
    }

    void Drop()
    {
        if (heldObject == null) return;

        // Unparent
        heldObject.transform.SetParent(null);

        // Place slightly in front and above arms
        heldObject.transform.position = arms.position + arms.forward * dropForwardOffset + Vector3.up * holdHeight;

        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = false;
            heldRigidbody.useGravity = true;
        }

        // Reset scale to (1,1,1)
        heldObject.transform.localScale = Vector3.one;

        Debug.Log("Dropped: " + heldObject.name);

        heldObject = null;
        heldRigidbody = null;
    }
}
