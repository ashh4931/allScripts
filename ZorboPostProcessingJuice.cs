using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ZorboTotalScreenImpact : MonoBehaviour
{
    public Volume globalVolume;

    [Header("Ses Efekti")]
    public AudioClip clickSFX; // Çalmasını istediğiniz sesi buraya sürükleyin
    [Range(0f, 1f)] public float clickVolume = 0.8f;

    [Header("Tıklama Efekt Değerleri (Vuruş)")]
    public float clickGrainIntensity = 1f;
    public float clickChromaIntensity = 1f;
    [Range(0f, 1f)] public float clickDistortionIntensity = 0.5f; 
    public float recoverSpeed = 5f;

    [Header("Canlılık (Pulse) Ayarları")]
    public bool enablePulse = true;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.2f;

    private FilmGrain grain;
    private ChromaticAberration chroma;
    private Bloom bloom;
    private LensDistortion lensDistortion;
    
    private float startGrain;
    private float startChroma;
    private float startBloom;
    private float startDistortion;

    private float targetGrain;
    private float targetChroma;
    private float targetDistortion;

    void Awake()
    {
        if (globalVolume.profile.TryGet(out grain)) startGrain = grain.intensity.value;
        if (globalVolume.profile.TryGet(out chroma)) startChroma = chroma.intensity.value;
        if (globalVolume.profile.TryGet(out bloom)) startBloom = bloom.intensity.value;
        if (globalVolume.profile.TryGet(out lensDistortion)) startDistortion = lensDistortion.intensity.value;

        targetGrain = startGrain;
        targetChroma = startChroma;
        targetDistortion = startDistortion;
    }

    void Update()
    {
        // 1. TIKLAMA ETKİSİ
        if (Input.GetMouseButtonDown(0))
        {
            // Görsel Efektler
            targetGrain = clickGrainIntensity;
            targetChroma = clickChromaIntensity;
            targetDistortion = clickDistortionIntensity;

            // SES EFEKTİ (AudioManager ile uyumlu)
            if (AudioManager.instance != null && clickSFX != null)
            {
                // UI veya genel tıklama olduğu için kameranın pozisyonunda çalıyoruz
                AudioManager.instance.PlaySFXAtPosition(clickSFX, Camera.main.transform.position, clickVolume);
            }
        }

        // 2. RECOVERY (Eski haline dönme)
        targetGrain = Mathf.Lerp(targetGrain, startGrain, Time.deltaTime * recoverSpeed);
        targetChroma = Mathf.Lerp(targetChroma, startChroma, Time.deltaTime * recoverSpeed);
        targetDistortion = Mathf.Lerp(targetDistortion, startDistortion, Time.deltaTime * recoverSpeed);

        if (grain != null) grain.intensity.value = targetGrain;
        if (chroma != null) chroma.intensity.value = targetChroma;
        if (lensDistortion != null) lensDistortion.intensity.value = targetDistortion;

        // 3. PULSE (Sürekli Canlılık)
        if (enablePulse && bloom != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            bloom.intensity.value = startBloom + pulse;
        }
    }
}