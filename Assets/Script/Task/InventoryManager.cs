using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple singleton inventory that stores up to maxCount Package entries.
/// Each entry keeps the original scene GameObject reference (deactivated on pickup)
/// and a small visual clone parented to `visualParent` (usually arms).
/// Provides simple OnGUI inventory display and selection/drop API.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory")]
    public int maxCount = 5;

    [Header("Visuals")]
    public Transform visualParent;        // parent for small stacked visuals (assign to Arms or PackageHolder)
    public Vector3 visualOffset = new Vector3(0f, 0.25f, 0f); // offset between visuals
    public Vector3 visualScale = Vector3.one * 0.45f;        // scale for visuals

    [Header("UI")]
    public Key toggleKey = Key.X;
    public bool showUIAtStart = true;

    // internal
    bool visible;
    int selectedIndex = 0;

    public class Item
    {
        public string name;        // color/name
        public GameObject original; // original scene object (deactivated when picked)
        public GameObject visual;   // small visual instantiated under visualParent
    }

    List<Item> items = new List<Item>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        visible = showUIAtStart;
    }

    void Update()
    {
        // toggle UI
        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            visible = !visible;
        }
        if (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame)
        {
            visible = !visible;
        }

        // navigate selection when inventory visible
        if (visible && items.Count > 0)
        {
            bool left = (Keyboard.current != null && Keyboard.current.leftArrowKey.wasPressedThisFrame)
                        || (Gamepad.current != null && Gamepad.current.dpad.left.wasPressedThisFrame);
            bool right = (Keyboard.current != null && Keyboard.current.rightArrowKey.wasPressedThisFrame)
                         || (Gamepad.current != null && Gamepad.current.dpad.right.wasPressedThisFrame);

            if (left) { selectedIndex--; if (selectedIndex < 0) selectedIndex = items.Count - 1; }
            if (right) { selectedIndex++; if (selectedIndex >= items.Count) selectedIndex = 0; }
        }
    }

    // Add a picked object to inventory. originalGO is the scene object (we will deactivate it).
    // Returns true if added, false if full.
    public bool Add(GameObject originalGO)
    {
        if (items.Count >= maxCount) return false;
        if (originalGO == null) return false;

        var it = new Item();
        it.original = originalGO;
        it.name = originalGO.name;

        // Create visual clone (small primitive or clone mesh) under visualParent
        if (visualParent != null)
        {
            // instantiate a simple cube (or clone original's mesh). Here we clone the original but strip components.
            GameObject vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vis.name = "InvVis_" + it.name;
            vis.transform.SetParent(visualParent, false);

            // position stacked
            vis.transform.localPosition = Vector3.up * (items.Count * visualOffset.y) + visualOffset;
            vis.transform.localRotation = Quaternion.identity;
            vis.transform.localScale = visualScale;

            // color material based on name (basic)
            var mr = vis.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.material = new Material(Shader.Find("Standard"));
                if (it.name.ToLower().Contains("red")) mr.material.color = Color.red;
                else if (it.name.ToLower().Contains("blue")) mr.material.color = Color.blue;
                else if (it.name.ToLower().Contains("yellow")) mr.material.color = Color.yellow;
                else mr.material.color = Color.gray;
            }

            it.visual = vis;
        }

        // deactivate the original in the world so it disappears
        originalGO.SetActive(false);

        items.Add(it);
        if (items.Count == 1) selectedIndex = 0; // ensure index valid

        return true;
    }

    // Drop the currently selected inventory item into the world in front of `dropOrigin`.
    // If dropToHub==true, caller should handle delivering (we just remove and destroy original).
    public bool DropSelected(Vector3 dropPosition, Quaternion dropRotation, bool dropToHub = false)
    {
        if (items.Count == 0) return false;
        selectedIndex = Mathf.Clamp(selectedIndex, 0, items.Count - 1);

        var it = items[selectedIndex];

        // if we stored an original reference, either destroy it (delivered) or re-enable and place it
        if (it.original != null)
        {
            if (dropToHub)
            {
                // delivered -> destroy original and visual
                Destroy(it.original);
            }
            else
            {
                it.original.SetActive(true);
                it.original.transform.position = dropPosition;
                it.original.transform.rotation = dropRotation;
            }
        }

        // destroy the visual if exists
        if (it.visual != null) Destroy(it.visual);

        items.RemoveAt(selectedIndex);
        if (items.Count == 0) selectedIndex = 0;
        else if (selectedIndex >= items.Count) selectedIndex = items.Count - 1;

        // re-stack visuals (if any)
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].visual != null && visualParent != null)
                items[i].visual.transform.localPosition = Vector3.up * (i * visualOffset.y) + visualOffset;
        }

        return true;
    }

    // DeliverSelected: convenience to drop to hub and notify checklist if provided
    public bool DeliverSelected(DeliveryHub hub)
    {
        if (hub == null) return false;
        if (items.Count == 0) return false;
        selectedIndex = Mathf.Clamp(selectedIndex, 0, items.Count - 1);
        var it = items[selectedIndex];

        // check color match using hub's hubColor
        if (!it.name.ToLower().Contains(hub.hubColor.ToLower()))
        {
            Debug.Log("Inventory: selected item color does not match hub.");
            return false;
        }

        // notify checklist
        if (hub.checklist != null) hub.checklist.DeliverPackage(hub.hubColor);

        // destroy original and visual
if (it.original != null)
{
    hub.PlaceDelivered(it.original); // this will SetActive(true) and parent it correctly
    // NOTE: InventoryManager should not destroy the original now
}        if (it.visual != null) Destroy(it.visual);

        items.RemoveAt(selectedIndex);
        if (items.Count == 0) selectedIndex = 0;
        else if (selectedIndex >= items.Count) selectedIndex = items.Count - 1;

        // re-stack visuals
        for (int i = 0; i < items.Count; i++)
            if (items[i].visual != null) items[i].visual.transform.localPosition = Vector3.up * (i * visualOffset.y) + visualOffset;

        return true;
    }

    // expose some helpers
    public int Count => items.Count;
    public int MaxCount => maxCount;
    public int SelectedIndex => selectedIndex;
    public string GetItemName(int index)
    {
        if (index < 0 || index >= items.Count) return "";
        return items[index].name;
    }

// void OnGUI()
// {
//     // if (!visible) return;

//     // // --- INVENTORY BOX (bottom-left) ---
//     // float w = 240;
//     // float h = 30 + (maxCount * 22);

//     // // Place it at bottom-left of the screen
//     // Rect r = new Rect(10, Screen.height - h - 10, w, h);
//     // GUI.Box(r, "Inventory (carry up to " + maxCount + ")");

//     // // list items
//     // for (int i = 0; i < items.Count; i++)
//     // {
//     //     string label = $"{i + 1}. {items[i].name}";
//     //     Rect ir = new Rect(r.x + 2, r.y + 20 + i * 20, w - 20, 20);

//     //     if (i == selectedIndex)
//     //         GUI.Box(ir, label);
//     //     else
//     //         GUI.Label(ir, label);
//     // }

//     // if (items.Count == 0)
//     // {
//     //     GUI.Label(new Rect(r.x + 2, r.y + 20, w - 20, 20), "<empty>");
//     // }

//     // --- HELP TEXT (top-left, unchanged) ---
//     // GUI.Label(new Rect(12, 10, 400, 16), "Left/Right to select. Enter/A to drop. X to toggle.");
// }

}
