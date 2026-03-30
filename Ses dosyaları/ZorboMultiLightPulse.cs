using UnityEngine;
using UnityEngine.Rendering.Universal; // Light2D için gerekli

public class ZorboMultiLightPulse : MonoBehaviour
{
    [Header("Işık Listesi")]
    public Light2D[] targetLights;

    [Header("Genel Ayarlar")]
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;
    public float basePulseSpeed = 2f;

    [Header("Rastgelelik (Variations)")]
    [Range(0f, 1f)] public float speedVariation = 0.3f; // Bazı ışıklar biraz daha hızlı/yavaş olsun

    // Her ışık için özel faz farkını ve hız çarpanını saklayacağız
    private float[] offsets;
    private float[] individualSpeeds;

    void Start()
    {
        if (targetLights == null || targetLights.Length == 0) return;

        offsets = new float[targetLights.Length];
        individualSpeeds = new float[targetLights.Length];

        for (int i = 0; i < targetLights.Length; i++)
        {
            // Her ışığa 0 ile 100 arasında rastgele bir başlangıç noktası veriyoruz
            offsets[i] = Random.Range(0f, 100f);
            
            // Hızları da biraz farklılaştırıyoruz ki zamanla birbirlerinden kopsunlar
            float speedMod = Random.Range(-speedVariation, speedVariation);
            individualSpeeds[i] = basePulseSpeed + speedMod;
        }
    }

    void Update()
    {
        if (targetLights == null) return;

        for (int i = 0; i < targetLights.Length; i++)
        {
            if (targetLights[i] == null) continue;

            // Formül: (Time.time + her ışığın kendi offseti) * kendi hızı
            float timeParam = (Time.time + offsets[i]) * individualSpeeds[i];
            
            // Sinüs dalgasını 0 ile 1 arasına çekiyoruz
            float lerpVal = (Mathf.Sin(timeParam) + 1f) / 2f;

            // Işığın şiddetini uygula
            targetLights[i].intensity = Mathf.Lerp(minIntensity, maxIntensity, lerpVal);
        }
    }
}