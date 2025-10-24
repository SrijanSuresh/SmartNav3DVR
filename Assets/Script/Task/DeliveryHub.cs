using UnityEngine;

/// <summary>
/// Delivery hub: accepts packages of a given color, notifies the checklist,
/// and places a visual of the delivered package at the hub (keeps visible).
/// Supports two modes:
///  - moveOriginal = true : the original GameObject is re-parented to the hub and made static
///  - moveOriginal = false: a copy of the package is instantiated at the hub and the original is destroyed
/// </summary>
public class DeliveryHub : MonoBehaviour
{
    [Header("Hub Settings")]
    [Tooltip("Color name this hub accepts (compare with package name)")]
    public string hubColor = "Red";

    [Tooltip("Reference to the checklist manager to update deliveries")]
    public ChecklistManager checklist;

    [Header("Placement Settings")]
    [Tooltip("Optional transform under which placed packages will be parented. If null, hub transform is used.")]
    public Transform placeParent;
    [Tooltip("Local offset from placeParent where first placed package will sit")]
    public Vector3 placeOffset = Vector3.zero;
    [Tooltip("Vertical spacing between stacked placed packages")]
    public float placeSpacing = 0.25f;
    [Tooltip("If true, the original package GameObject will be moved into the hub and made static. If false, a visual clone will be instantiated.")]
    public bool moveOriginal = true;
    [Tooltip("If instantiating a copy, optionally assign a prefab to use for the visual (keeps original format). If null, the original GameObject is cloned.")]
    public GameObject deliveredVisualPrefab;

    int _placedCount = 0;

    // Called when any collider enters the hub trigger. If it's a package, attempt delivery.
    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        // react to common tags for packages (support both "Package" and "Pickable")
        if (other.CompareTag("Package"))
        {
            TryDeliver(other.gameObject);
        }
    }

    /// <summary>
    /// Attempt to deliver a package GameObject to this hub. This will check color match,
    /// update checklist (if assigned), and call PlaceDelivered to show the package on the hub.
    /// </summary>
    public void TryDeliver(GameObject package)
    {
        if (package == null) return;

        string packageName = package.name.ToLower();
        string hubColorLower = hubColor.ToLower();

        Debug.Log($"DeliveryHub: Attempting to deliver '{package.name}' to hub '{hubColor}'");

        // color match
        if (!packageName.Contains(hubColorLower))
        {
            Debug.Log($"DeliveryHub: Wrong color. '{package.name}' does not match hub '{hubColor}'.");
            return;
        }

        // update checklist
        if (checklist != null)
        {
            checklist.DeliverPackage(hubColor);
        }
        else
        {
            Debug.LogWarning("DeliveryHub: ChecklistManager not assigned on hub inspector.");
        }

        // place the visual - either move original or instantiate a copy
        PlaceDelivered(package);
    }

    /// <summary>
    /// Place the delivered package visually at the hub.
    /// If moveOriginal==true, the original GameObject will be parented to the hub,
    /// physics/colliders disabled and it will be kept visible.
    /// If moveOriginal==false, a clone (or deliveredVisualPrefab) will be instantiated and the original destroyed.
    /// </summary>
    public void PlaceDelivered(GameObject package)
    {
        if (package == null) return;

        Transform parent = placeParent != null ? placeParent : this.transform;
        Vector3 localPos = placeOffset + Vector3.up * (_placedCount * placeSpacing);

        if (moveOriginal)
        {
            // Make original visible (in case it was deactivated) and safe for hub placement
            package.SetActive(true);

            // disable physics and colliders so it sits nicely
            var rb = package.GetComponent<Rigidbody>();
            if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }

            foreach (var col in package.GetComponentsInChildren<Collider>())
                col.enabled = false;

            // parent and position relative to parent (no world position stay)
            package.transform.SetParent(parent, worldPositionStays: false);
            package.transform.localPosition = localPos;
            package.transform.localRotation = Quaternion.identity;
            package.transform.localScale = Vector3.one * 0.5f; // scale down to fit hub

            Debug.Log($"DeliveryHub: Placed original package '{package.name}' at hub '{hubColor}'.");
        }
        else
        {
            // Instantiate visual copy
            GameObject visual = null;

            if (deliveredVisualPrefab != null)
            {
                visual = Instantiate(deliveredVisualPrefab, parent);
            }
            else
            {
                // clone the original (simple) — instantiate and remove unnecessary components
                visual = Instantiate(package, parent);
            }

            // ensure visible and remove physics/colliders on the clone
            if (visual != null)
            {
                visual.SetActive(true);

                var rb2 = visual.GetComponent<Rigidbody>();
                if (rb2 != null) { Destroy(rb2); }

                foreach (var col in visual.GetComponentsInChildren<Collider>())
                    Destroy(col);

                // reset local transform
                visual.transform.localPosition = localPos;
                visual.transform.localRotation = Quaternion.identity;
                visual.transform.localScale = Vector3.one;

                Debug.Log($"DeliveryHub: Instantiated visual for '{package.name}' at hub '{hubColor}'.");
            }

            // destroy or deactivate the original (we choose destroy to free memory)
            // If you want to keep original to re-use, call SetActive(false) instead.
            Destroy(package);
        }

        _placedCount++;
    }

    /// <summary>
    /// Optional: reset placed visuals (editor / debugging helper).
    /// </summary>
    public void ClearPlacedVisuals()
    {
        _placedCount = 0;
        if (placeParent == null) return;

        // destroy all children of placeParent
        for (int i = placeParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(placeParent.GetChild(i).gameObject);
        }
    }
}
