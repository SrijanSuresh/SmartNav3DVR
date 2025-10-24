using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // ✅ New Input System

public class GazePickupInventory : MonoBehaviour
{
    [Header("References")]
    public Transform holdPoint; // where boxes appear when held
    public float pickupRange = 3f;
    public LayerMask pickableLayer;
    public DeliveryHub deliveryHub;

    [Header("Inventory Settings")]
    public int maxCapacity = 5;
    public List<GameObject> inventory = new List<GameObject>();

    [Header("UI")]
    public string helpText =
        "E / Gamepad X = Pick up box\n" +
        "R / Gamepad B = Drop selected box\n" +
        "← / → to change selected item\n" +
        "Deliver near a hub of matching color.";

    int selectedIndex = 0;

    void Update()
    {
        HandlePickup();
        HandleDrop();
        HandleSelect();
    }

    void HandlePickup()
    {
        // ✅ Keyboard / Gamepad input (E key or Gamepad X button)
        bool pickupPressed =
            (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame);

        if (pickupPressed && inventory.Count < maxCapacity)
        {
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward,
                out RaycastHit hit, pickupRange, pickableLayer))
            {
                GameObject obj = hit.collider.gameObject;
                if (obj.CompareTag("Package"))
                {
                    inventory.Add(obj);
                    obj.SetActive(false);
                    Debug.Log($"Picked up {obj.name} ({inventory.Count}/{maxCapacity})");
                }
            }
        }
    }

    void HandleDrop()
    {
        // ✅ Keyboard / Gamepad input (R key or Gamepad B button)
        bool dropPressed =
            (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame);

        if (dropPressed && inventory.Count > 0)
        {
            GameObject obj = inventory[selectedIndex];
            inventory.RemoveAt(selectedIndex);

            obj.transform.position = holdPoint.position + holdPoint.forward * 1f;
            obj.SetActive(true);
            Debug.Log($"Dropped {obj.name}");

            if (deliveryHub != null)
                deliveryHub.TryDeliver(obj);

            if (selectedIndex >= inventory.Count)
                selectedIndex = Mathf.Max(0, inventory.Count - 1);
        }
    }

    void HandleSelect()
    {
        // ✅ Arrow keys or Gamepad D-Pad / Right stick
        bool leftPressed =
            (Keyboard.current != null && Keyboard.current.leftArrowKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.dpad.left.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.rightStick.left.wasPressedThisFrame);

        bool rightPressed =
            (Keyboard.current != null && Keyboard.current.rightArrowKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.dpad.right.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.rightStick.right.wasPressedThisFrame);

        if (inventory.Count == 0) return;

        if (leftPressed) selectedIndex = (selectedIndex - 1 + inventory.Count) % inventory.Count;
        if (rightPressed) selectedIndex = (selectedIndex + 1) % inventory.Count;
    }

void OnGUI()
{
    // 🎨 Create custom styles for boxes
    GUIStyle invStyle = new GUIStyle(GUI.skin.box);
    invStyle.fontSize = 18;                          // Larger font
    invStyle.fontStyle = FontStyle.Bold;             // Bold text
    invStyle.normal.textColor = Color.white;         // White text
    invStyle.alignment = TextAnchor.UpperLeft;       // Left-aligned text
    invStyle.wordWrap = true;                        // Wrap long lines

    GUIStyle helpStyle = new GUIStyle(GUI.skin.box);
    helpStyle.fontSize = 16;
    helpStyle.normal.textColor = Color.yellow;       // Help text in yellow
    helpStyle.wordWrap = true;

    // Optional: darker transparent background for readability
    Texture2D darkBg = new Texture2D(1, 1);
    darkBg.SetPixel(0, 0, new Color(0, 0, 0, 0.7f)); // semi-transparent black
    darkBg.Apply();

    invStyle.normal.background = darkBg;
    helpStyle.normal.background = darkBg;

    // 🧰 Build inventory string
    string inv = "🎒 Inventory:\n\n";
    for (int i = 0; i < maxCapacity; i++)
    {
        if (i < inventory.Count)
        {
            string itemName = inventory[i].name;
            if (i == selectedIndex)
                inv += $"> {itemName}\n";  // Highlight selected
            else
                inv += $"  {itemName}\n";
        }
        else
            inv += "-\n";
    }

    // 🖼 Draw UI boxes
    GUI.Box(new Rect(10, 10, 220, 200), inv, invStyle);
    GUI.Box(new Rect(240, 10, 320, 120), helpText, helpStyle);
}

}
