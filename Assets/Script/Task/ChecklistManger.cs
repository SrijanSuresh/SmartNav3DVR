using UnityEngine;
using UnityEngine.InputSystem;  // For controller & keyboard input

public class ChecklistManager : MonoBehaviour
{
    [Header("Checklist Settings")]
    public Key toggleKey = Key.X;  // Keyboard toggle key
    private bool showChecklist = false;

    // Example delivery goals
    private int redTotal = 2;
    private int blueTotal = 1;
    private int yellowTotal = 1;

    // Track how many have been delivered
    private int redDelivered = 0;
    private int blueDelivered = 0;
    private int yellowDelivered = 0;

    void Update()
    {
        // ✅ Toggle checklist with keyboard X key
        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            showChecklist = !showChecklist;
            Debug.Log("Checklist toggled (Keyboard): " + showChecklist);
        }

        // ✅ Toggle checklist with controller R2 (Right Trigger)
        if (Gamepad.current != null && Gamepad.current.rightTrigger.wasPressedThisFrame)
        {
            showChecklist = !showChecklist;
            Debug.Log("Checklist toggled (Controller R2): " + showChecklist);
        }
    }

    public void DeliverPackage(string color)
    {
        color = color.ToLower();
        if (color.Contains("red")) redDelivered = Mathf.Min(redDelivered + 1, redTotal);
        else if (color.Contains("blue")) blueDelivered = Mathf.Min(blueDelivered + 1, blueTotal);
        else if (color.Contains("yellow")) yellowDelivered = Mathf.Min(yellowDelivered + 1, yellowTotal);

        Debug.Log($"Delivered {color} package!");
    }

    void OnGUI()
    {
    if (!showChecklist) return;

    float boxWidth = 270;
    float boxHeight = 150;
    float padding = 10;
    float yPos = Screen.height - boxHeight - padding;

    // Create a GUI style
    GUIStyle style = new GUIStyle(GUI.skin.box);
    style.fontSize = 18;                        // Increase text size
    style.fontStyle = FontStyle.Bold;           // Make text bold
    style.normal.textColor = Color.black;       // Change text color
    style.alignment = TextAnchor.UpperLeft;     // Align text nicely
    style.wordWrap = true;                      // Wrap long lines

    string checklistText =
        "📦 DELIVERY CHECKLIST\n\n" +
        $"Red Packages:    {redDelivered}/{redTotal}\n" +
        $"Blue Packages:   {blueDelivered}/{blueTotal}\n" +
        $"Yellow Packages: {yellowDelivered}/{yellowTotal}\n\n" +
        "(Press X or R2 to toggle this checklist)";

    GUI.Box(new Rect(padding, yPos, boxWidth, boxHeight), checklistText, style);    }
}
