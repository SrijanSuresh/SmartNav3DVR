using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[ExecuteAlways]
public class MinimapController : MonoBehaviour
{
    [Header("References")]
    public Camera minimapCamera;      // top-down orthographic camera (renders to RenderTexture)
    public Transform player;         // player root or head (follow X/Z)
    public RawImage minimapRawImage; // your UI RawImage (set texture = minimapCamera.targetTexture)
    public RectTransform playerIcon; // UI icon centered on RawImage that rotates to show heading

    [Header("Camera follow")]
    public float height = 50f;       // camera Y above player
    public float followSmooth = 10f; // smoothing for camera position
    public float orthoSize = 20f;    // camera.orthographicSize

    [Header("Behaviour")]
    public bool rotateWithPlayer = false; // if true, map rotates so forward is up
    public Key toggleKey = Key.M;    // keyboard toggle (new Input System)
    public bool visibleAtStart = true;

    // internal
    bool visible;

    void Start()
    {
        visible = visibleAtStart;
        if (minimapRawImage != null) minimapRawImage.gameObject.SetActive(visible);
        if (minimapCamera != null)
        {
            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = orthoSize;
            // ensure camera points straight down
            minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }

    void Update()
    {
        if (player == null || minimapCamera == null) return;

        HandleToggleInput();
        UpdateCameraPositionAndRotation();
        UpdatePlayerIconRotation();
    }

    void HandleToggleInput()
    {
        // keyboard
        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            visible = !visible;
            if (minimapRawImage != null) minimapRawImage.gameObject.SetActive(visible);
        }

        // controller (X button / buttonWest)
        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
        {
            visible = !visible;
            if (minimapRawImage != null) minimapRawImage.gameObject.SetActive(visible);
        }
    }

    void UpdateCameraPositionAndRotation()
    {
        // follow player X,Z
        Vector3 targetPos = new Vector3(player.position.x, height, player.position.z);
        minimapCamera.transform.position = Vector3.Lerp(minimapCamera.transform.position, targetPos, Time.deltaTime * followSmooth);

        // rotation handling:
        if (rotateWithPlayer)
        {
            // rotate camera around Y so player forward is up in map
            float yaw = player.eulerAngles.y;
            // camera should remain looking straight down; rotate around world up
            minimapCamera.transform.rotation = Quaternion.Euler(90f, yaw, 0f);
        }
        else
        {
            // north-up
            minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }

        // orthographic size live update (optional)
        if (minimapCamera.orthographic) minimapCamera.orthographicSize = orthoSize;
    }

    void UpdatePlayerIconRotation()
    {
        if (playerIcon == null) return;

        if (rotateWithPlayer)
        {
            // if map rotates with player, keep icon upright (no rotation)
            playerIcon.localRotation = Quaternion.identity;
        }
        else
        {
            // rotate icon to show player's heading relative to north-up map
            float yaw = player.eulerAngles.y;
            playerIcon.localRotation = Quaternion.Euler(0f, 0f, -yaw);
        }
    }

    // Editor helper: ensure RawImage texture matches camera's RT (execute in edit mode too)
    void OnValidate()
    {
        if (minimapRawImage != null && minimapCamera != null && minimapCamera.targetTexture != null)
            minimapRawImage.texture = minimapCamera.targetTexture;
    }
}
