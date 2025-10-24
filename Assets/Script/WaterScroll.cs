// WaterScroll.cs
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WaterScroll : MonoBehaviour
{
    public Vector2 normalScroll = new Vector2(0.02f, 0.01f);
    private Material mat;
    private Vector2 offset;

    void Awake() => mat = GetComponent<Renderer>().material;

    void Update()
    {
        offset += normalScroll * Time.deltaTime;
        // "_BumpMap" is the normal map slot on Standard/URP Lit
        mat.SetTextureOffset("_BumpMap", offset);
    }
}
