using UnityEngine;

public class AmbienceController : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource weatherAmbience;

    /* =======================
       PUBLIC API
       ======================= */

    public void StartRain()
    {
        if (weatherAmbience == null) return;

        if (!weatherAmbience.isPlaying)
            weatherAmbience.Play();
    }

    public void StopRain()
    {
        if (weatherAmbience == null) return;

        if (weatherAmbience.isPlaying)
            weatherAmbience.Stop();
    }
}

