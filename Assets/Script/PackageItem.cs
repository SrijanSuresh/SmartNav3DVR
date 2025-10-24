using UnityEngine;

public class PackageItem : MonoBehaviour
{
    public ColorType color;
    [Tooltip("Optional: used to show highlight when selected later")]
    public Renderer rend;

    void Reset()
    {
        if (!rend) rend = GetComponentInChildren<Renderer>();
    }

    // Helper to get color name as string
    public string GetColorName()
    {
        return color.ToString();
    }
}
