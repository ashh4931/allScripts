using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HealthVisualEffects : MonoBehaviour
{
    [Header("Referanslar")]
    public PlayerStats playerStats; 
    public Volume volume;
    public AudioSource heartBeatAudio; 
    public Camera mainCamera; // Ana Kameranı buraya sürükle

    [Header("Görsel Ayarlar")]
    [Range(0, 1)] public float effectThreshold = 0.35f; 
    public float pulseSpeedBase = 5f; 
    public float maxVignetteIntensity = 0.55f; 

    [Header("Ses Ayarları")]
    public float minPitch = 1.0f; 
    public float maxPitch = 2.5f; 

    [Header("Kamera Zoom Ayarları")]
    [Range(0, 1)] public float maxZoomPercent = 0.2f; // Can %0 iken kamera ne kadar yakınlaşsın? (0.2 = %20)
    public float zoomSpeed = 5f; 

    private Vignette _vignette;
    private float _initialCameraSize; // 2D için
    private float _initialCameraFOV;  // 3D için

    void Start()
    {
        // 1. Post-Processing Ayarı
        if (volume.profile.TryGet(out _vignette))
        {
            _vignette.color.value = Color.red;
        }
        
        // 2. Ses Ayarı
        if (heartBeatAudio != null)
        {
            heartBeatAudio.loop = true;
            heartBeatAudio.Stop();
        }

        // 3. Kamera Başlangıç Değerlerini Kaydet
        if (mainCamera != null)
        {
            _initialCameraSize = mainCamera.orthographicSize;
            _initialCameraFOV = mainCamera.fieldOfView;
        }
        else
        {
            // Eğer atanmadıysa ana kamerayı otomatik bulmayı dene
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                _initialCameraSize = mainCamera.orthographicSize;
                _initialCameraFOV = mainCamera.fieldOfView;
            }
        }
    }

    void Update()
    {
        if (playerStats == null || _vignette == null || mainCamera == null) return;

        // CAN HESABI: currentHealth / maxHealth
        float healthRatio = playerStats.currentHealth / playerStats.maxHealth;

        if (healthRatio <= effectThreshold)
        {
            // --- Tehlike Faktörü (0-1 arası) ---
            float dangerFactor = 1f - (healthRatio / effectThreshold);

            // --- 1. Ses Hızlandırma ---
            if (heartBeatAudio != null)
            {
                if (!heartBeatAudio.isPlaying) heartBeatAudio.Play();
                heartBeatAudio.pitch = Mathf.Lerp(minPitch, maxPitch, dangerFactor);
            }

            // --- 2. Görsel Nabız (Animasyon) ---
            float currentPulseSpeed = pulseSpeedBase + (dangerFactor * 10f);
            float pulse = Mathf.Sin(Time.time * currentPulseSpeed) * 0.1f;
            _vignette.intensity.value = Mathf.Lerp(_vignette.intensity.value, maxVignetteIntensity + pulse, Time.deltaTime * 5f);

            // --- 3. Kamera Zoom (Dinamik) ---
            if (mainCamera.orthographic) // 2D Oyunsa
            {
                // Can azaldıkça orthographicSize küçülür (yakınlaşır)
                float targetSize = _initialCameraSize * (1f - (dangerFactor * maxZoomPercent));
                mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
            }
            else // 3D Oyunsa
            {
                // Can azaldıkça Field of View küçülür (yakınlaşır)
                float targetFOV = _initialCameraFOV * (1f - (dangerFactor * maxZoomPercent));
                mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
            }
        }
        else
        {
            // --- Her Şeyi Normal Hale Getir ---
            _vignette.intensity.value = Mathf.Lerp(_vignette.intensity.value, 0, Time.deltaTime * 2f);
            
            if (heartBeatAudio != null && heartBeatAudio.isPlaying)
            {
                heartBeatAudio.Stop();
            }

            // Kamerayı yavaşça eski haline döndür
            if (mainCamera.orthographic)
            {
                mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, _initialCameraSize, Time.deltaTime * (zoomSpeed / 2f));
            }
            else
            {
                mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, _initialCameraFOV, Time.deltaTime * (zoomSpeed / 2f));
            }
        }
    }
}