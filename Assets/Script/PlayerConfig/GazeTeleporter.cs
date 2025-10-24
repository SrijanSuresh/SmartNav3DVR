// SimpleGazeTeleporter.cs
// Attach to the player's HMD / camera object (center-eye).
using UnityEngine;

public class SimpleGazeTeleporter : MonoBehaviour
{
    [Header("Gaze")]
    public float gazeDuration = 1.5f;
    public float maxDistance = 50f;
    public LayerMask stationLayer = ~0;   // set to TeleportStation layer
    public string stationTag = "TeleportStation"; // or leave blank to use component search

    [Header("Player")]
    public VRPlayerController playerController; // assign the root controller here

    [Header("UI")]
    public UnityEngine.UI.Image gazeProgressImage; // optional radial fill image

    float timer = 0f;
    TeleportStation lastStation = null;

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.green);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, stationLayer))
        {
            TeleportStation station = hit.collider.GetComponentInParent<TeleportStation>();
            if (station == null && !string.IsNullOrEmpty(stationTag))
            {
                // try tag-based fallback
                if (hit.collider.CompareTag(stationTag))
                    station = hit.collider.GetComponentInParent<TeleportStation>();
            }

            if (station != null)
            {
                if (lastStation != station)
                {
                    lastStation = station;
                    timer = 0f;
                }

                timer += Time.deltaTime;
                UpdateProgress(timer / gazeDuration);

                if (timer >= gazeDuration)
                {
                    // Teleport via player controller
                    if (playerController != null)
                    {
                        playerController.TeleportToStation(station);
                    }
                    timer = 0f;
                    lastStation = null;
                    UpdateProgress(0f);
                }
                return;
            }
        }

        // miss or not a station
        timer = 0f;
        lastStation = null;
        UpdateProgress(0f);
    }

    void UpdateProgress(float normalized)
    {
        if (gazeProgressImage != null)
            gazeProgressImage.fillAmount = Mathf.Clamp01(normalized);
    }
}
