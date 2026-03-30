using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SpeedEffectManager : MonoBehaviour
{
    [Header("Referanslar")]
    public PlayerController player; 
    public Volume globalVolume;

    [Header("Efekt Ayarları")]
    [Tooltip("Maksimum hızda ulaşılacak bükülme miktarı (Örn: -0.4)")]
    public float maxDistortion = -0.3f; 
    
    [Tooltip("Efektin hıza ne kadar çabuk tepki vereceği")]
    public float lerpSpeed = 5f;

    [Header("Hız Eşikleri")]
    [Tooltip("Bu hızın altındayken efekt HİÇ çalışmaz (Örn: 10)")]
    public float minSpeedThreshold = 10f;
    
    [Tooltip("Efektin maksimum bükülmeye ulaşacağı hız (Örn: 20)")]
    public float maxSpeedThreshold = 20f;

    private LensDistortion _lensDistortion;
    private Rigidbody2D _rb;

    void Start()
    {
        // Rigidbody referansını al (Hızı ölçmek için)
        _rb = player.GetComponent<Rigidbody2D>();

        // Volume içinden Lens Distortion bileşenini bul
        if (globalVolume.profile.TryGet(out _lensDistortion))
        {
            _lensDistortion.intensity.overrideState = true;
        }
        else
        {
            Debug.LogError("Global Volume içinde Lens Distortion bulunamadı!");
        }
    }

    void Update()
    {
        if (_lensDistortion == null || _rb == null) return;

        // Mevcut hızı al
        float currentSpeed = _rb.linearVelocity.magnitude;
        
        // 1. ADIM: Hızı verdiğimiz iki eşik değere göre oranla
        // Eğer currentSpeed <= 10 ise, oran = 0 olur.
        // Eğer currentSpeed >= 20 ise, oran = 1 olur.
        // Eğer currentSpeed 15 ise, oran = 0.5 olur (tam ortası).
        float speedRatio = Mathf.InverseLerp(minSpeedThreshold, maxSpeedThreshold, currentSpeed);

        // 2. ADIM: Hedef bükülme miktarını bu orana göre hesapla
        float targetIntensity = speedRatio * maxDistortion;

        // 3. ADIM: Yumuşak geçişle (Lerp) uygula
        _lensDistortion.intensity.value = Mathf.Lerp(
            _lensDistortion.intensity.value, 
            targetIntensity, 
            Time.deltaTime * lerpSpeed
        );
    }
}