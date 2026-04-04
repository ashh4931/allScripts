using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // URP Paketinin yüklü olması şart

public class ZorboTotalScreenImpact : MonoBehaviour
{
    public Volume globalVolume;
    
    [Header("Tıklama Efekt Değerleri (Vuruş)")]
    public float clickGrainIntensity = 1f;
    public float clickChromaIntensity = 1f;
    // 0 ile 1 arası. Pozitif değerler dışa doğru bükülme (Barrel) yapar.
    [Range(0f, 1f)] public float clickDistortionIntensity = 0.5f; 
    public float recoverSpeed = 5f; // Eski haline dönme hızı (Hızlı olması iyi)

    [Header("Canlılık (Pulse) Ayarları")]
    public bool enablePulse = true;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.2f;

    // Başlangıç Değerleri (Awake'te dolacak)
    private FilmGrain grain;
    private ChromaticAberration chroma;
    private Bloom bloom;
    private LensDistortion lensDistortion; // YENİ EKLENDİ
    
    private float startGrain;
    private float startChroma;
    private float startBloom;
    private float startDistortion; // YENİ EKLENDİ

    // Hedef Değerler
    private float targetGrain;
    private float targetChroma;
    private float targetDistortion; // YENİ EKLENDİ

    void Awake()
    {
        // Volume Profile üzerinden efektlere ulaşıyoruz ve başlangıç değerlerini kaydediyoruz
        if (globalVolume.profile.TryGet(out grain)) startGrain = grain.intensity.value;
        if (globalVolume.profile.TryGet(out chroma)) startChroma = chroma.intensity.value;
        if (globalVolume.profile.TryGet(out bloom)) startBloom = bloom.intensity.value;
        if (globalVolume.profile.TryGet(out lensDistortion)) startDistortion = lensDistortion.intensity.value; // YENİ EKLENDİ

        // Hedefleri başlangıç değerine eşitle
        targetGrain = startGrain;
        targetChroma = startChroma;
        targetDistortion = startDistortion; // YENİ EKLENDİ
    }

    void Update()
    {
        // 1. TIKLAMA ETKİSİ (Darbe Algılama)
        if (Input.GetMouseButtonDown(0))
        {
            // IMPACT! Değerleri anlık olarak zirveye çekiyoruz
            targetGrain = clickGrainIntensity;
            targetChroma = clickChromaIntensity;
            targetDistortion = clickDistortionIntensity; // YENİ EKLENDİ
        }

        // 2. YAVAŞÇA ESKİ HALİNE DÖNME (Recovery - Lerp)
        targetGrain = Mathf.Lerp(targetGrain, startGrain, Time.deltaTime * recoverSpeed);
        targetChroma = Mathf.Lerp(targetChroma, startChroma, Time.deltaTime * recoverSpeed);
        targetDistortion = Mathf.Lerp(targetDistortion, startDistortion, Time.deltaTime * recoverSpeed); // YENİ EKLENDİ

        // Güncel değerleri Profile'a yazıyoruz
        if (grain != null) grain.intensity.value = targetGrain;
        if (chroma != null) chroma.intensity.value = targetChroma;
        if (lensDistortion != null) lensDistortion.intensity.value = targetDistortion; // YENİ EKLENDİ

        // 3. CANLILIK (Sürekli Pulse Efekti)
        if (enablePulse && bloom != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            bloom.intensity.value = startBloom + pulse;
        }
    }
}