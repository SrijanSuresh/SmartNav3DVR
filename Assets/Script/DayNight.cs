using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Material daySkybox;
    public Material nightSkybox;
    public float switchInterval = 10f; // seconds

    private float timer = 0f;
    private bool isDay = true;

    void Start()
    {
        if (daySkybox != null)
            RenderSettings.skybox = daySkybox;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= switchInterval)
        {
            timer = 0f;
            isDay = !isDay;
            RenderSettings.skybox = isDay ? daySkybox : nightSkybox;
        }
    }
}
