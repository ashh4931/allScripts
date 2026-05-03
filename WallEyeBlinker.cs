using UnityEngine;
using UnityEngine.Rendering.Universal; // Light2D (URP) için gerekli
using System.Collections;
using System.Collections.Generic;

public class WallEyeBlinker : MonoBehaviour
{
    [System.Serializable]
    public class WallEye
    {
        public SpriteRenderer eyeGraphic;
        public Light2D eyeLight;         

        // İçsel durum yönetimi
        [HideInInspector] public bool isOpen = false;           // Göz tamamen açık ve parlıyor mu?
        [HideInInspector] public bool isTransitioning = false;  // Şu an açılma/kapanma animasyonunda mı?
        [HideInInspector] public float timeOffset;       
        [HideInInspector] public float individualPulseSpeed; 
        [HideInInspector] public Vector3 originalScale; 
    }

    [Header("Gözler Listesi")]
    public List<WallEye> wallEyes;

    [Header("Sürü (Grup) Ayarları")]
    public int maxSimultaneousOpenEyes = 4; // Aynı anda açık kalabilecek MAKSİMUM göz sayısı
    public float checkInterval = 0.5f;      // Yeni bir göz açmak için kaç saniyede bir kontrol edilsin?
    private float nextCheckTime;

    [Header("Işık Ayarları (Açıkken Parlama)")]
    public float minPulseIntensity = 0.5f;
    public float maxPulseIntensity = 1.5f;
    public float basePulseSpeed = 2f;
    [Range(0f, 1f)] public float pulseSpeedVariation = 0.3f; 
    
    [Header("Animasyon Hızları")]
    public float openDuration = 0.5f;   // Gözün yavaşça açılma süresi
    public float closeDuration = 0.2f;  // Gözün kapanma süresi (eski squintDuration)

    [Header("Kapalı Hal Ayarları")]
    public float closedYScaleMultiplier = 0.05f; // Kapalıyken Y ekseninde ne kadar ince olsun? (0.05 = neredeyse düz çizgi)
    public float closedLightIntensity = 0f;      // Kapalıyken ışık şiddeti (0 = tamamen karanlık)

    [Header("Açık Kalma Süresi")]
    public float minOpenDuration = 3f; // Bir göz açılınca en az kaç saniye açık kalsın
    public float maxOpenDuration = 6f; // Bir göz açılınca en çok kaç saniye açık kalsın

    void Start()
    {
        InitializeEyes();
    }

    void InitializeEyes()
    {
        foreach (var eye in wallEyes)
        {
            if (eye.eyeGraphic == null || eye.eyeLight == null) continue;

            eye.originalScale = eye.eyeGraphic.transform.localScale;
            eye.timeOffset = Random.Range(0f, 100f);
            eye.individualPulseSpeed = basePulseSpeed * (1f + Random.Range(-pulseSpeedVariation, pulseSpeedVariation));

            // Başlangıçta TÜM GÖZLERİ KAPALI duruma getir
            Vector3 closedScale = new Vector3(eye.originalScale.x, eye.originalScale.y * closedYScaleMultiplier, eye.originalScale.z);
            eye.eyeGraphic.transform.localScale = closedScale;
            eye.eyeLight.intensity = closedLightIntensity;

            eye.isOpen = false;
            eye.isTransitioning = false;
        }
    }

    void Update()
    {
        // Sadece açık ve animasyonda (transition) olmayan gözlere Pulsing uygula
        foreach (var eye in wallEyes)
        {
            if (eye.isOpen && !eye.isTransitioning)
            {
                HandleOpenPulsing(eye);
            }
        }

        // Belirli aralıklarla yeni bir göz açmaya çalış
        if (Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + checkInterval;
            TryOpenRandomEye();
        }
    }

    // Açık göz sayısını kontrol eder ve gerekirse kapalı bir gözü uyandırır
    void TryOpenRandomEye()
    {
        int activeEyesCount = 0;
        List<WallEye> closedEyes = new List<WallEye>();

        // Hangi gözler aktif, hangileri tamamen kapalı sayalım
        foreach (var eye in wallEyes)
        {
            // Eğer açık veya şu an açılma/kapanma aşamasındaysa aktif sayıyoruz
            if (eye.isOpen || eye.isTransitioning) 
            {
                activeEyesCount++;
            }
            else
            {
                closedEyes.Add(eye);
            }
        }

        // Eğer limitten az göz açıksa ve kapalıda bekleyen göz varsa
        if (activeEyesCount < maxSimultaneousOpenEyes && closedEyes.Count > 0)
        {
            // Kapalı olanlardan tamamen rastgele birini seç
            WallEye chosenEye = closedEyes[Random.Range(0, closedEyes.Count)];
            
            // Seçilen göz için uyanma senaryosunu başlat
            StartCoroutine(EyeLifecycleSequence(chosenEye));
        }
    }

    void HandleOpenPulsing(WallEye eye)
    {
        float timeParam = (Time.time + eye.timeOffset) * eye.individualPulseSpeed;
        float lerpVal = (Mathf.Sin(timeParam) + 1f) / 2f;
        eye.eyeLight.intensity = Mathf.Lerp(minPulseIntensity, maxPulseIntensity, lerpVal);
    }

    // Bir gözün doğuşu, yaşamı ve batışı (Açıl -> Bekle -> Kapan)
    IEnumerator EyeLifecycleSequence(WallEye eye)
    {
        eye.isTransitioning = true; // Başka sistemler bu göze müdahale etmesin diye kilitliyoruz

        Vector3 closedScale = new Vector3(eye.originalScale.x, eye.originalScale.y * closedYScaleMultiplier, eye.originalScale.z);

        // --- 1. GÖZÜ AÇ (Yavaşça) ---
        yield return StartCoroutine(LerpEye(eye, closedScale, eye.originalScale, closedLightIntensity, maxPulseIntensity, openDuration));
        
        // Açılma tamamlandı, artık açık olarak işaretle
        eye.isOpen = true;
        eye.isTransitioning = false;

        // --- 2. AÇIK KALMA SÜRESİ ---
        // Göz bu sürede açık kalıp Pulsing fonksiyonu ile parlayacak
        float stayOpenDuration = Random.Range(minOpenDuration, maxOpenDuration);
        yield return new WaitForSeconds(stayOpenDuration);

        // --- 3. GÖZÜ KAPAT ---
        eye.isOpen = false;        // Artık pulsing uygulanmasın
        eye.isTransitioning = true; // Kapanma işlemi kilitlendi

        // Kapanırken ışık şiddeti o an pulse'tan dolayı neyse onu alıyoruz ki pürüzsüz kapansın
        float currentIntensity = eye.eyeLight.intensity;
        yield return StartCoroutine(LerpEye(eye, eye.originalScale, closedScale, currentIntensity, closedLightIntensity, closeDuration));

        // İşlem tamamen bitti, tekrar tamamen kapalılar listesine geri döndü
        eye.isTransitioning = false;
    }

    IEnumerator LerpEye(WallEye eye, Vector3 startScale, Vector3 targetScale, float startIntensity, float targetIntensity, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            eye.eyeGraphic.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            eye.eyeLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);

            yield return null;
        }

        eye.eyeGraphic.transform.localScale = targetScale;
        eye.eyeLight.intensity = targetIntensity;
    }
}