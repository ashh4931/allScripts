using UnityEngine;
using UnityEngine.Rendering.Universal; // Light2D için gerekli

public class LightPulse : MonoBehaviour
{
    private Light2D eyeLight;

    [Header("Işık Ayarları")]
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;
    public float pulseSpeed = 2f;

    void Start()
    {
        eyeLight = GetComponent<Light2D>();
    }

    void Update()
    {
        if (eyeLight == null) return;

        // Sinüs dalgasını 0 ile 1 arasına normalize ediyoruz
        float lerpVal = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        
        // Belirlediğin min ve max değerler arasında yumuşak geçiş
        eyeLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, lerpVal);
    }
}