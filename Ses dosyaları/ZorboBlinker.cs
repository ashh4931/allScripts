using UnityEngine;
using System.Collections;

public class ZorboBlinker : MonoBehaviour
{
    [Header("Göz Bebekleri (GameObjects)")]
    // Hiyerarşideki o küçük siyah yuvarlakları (Left_Pupil ve Right_Pupil) buraya sürükle.
    // RectTransform değil, doğrudan GameObject olarak sürükle!
    public GameObject leftPupilGO;
    public GameObject rightPupilGO;

    [Header("Göz Kırpma Ayarları")]
    public float blinkDuration = 0.1f; // Ne kadar süre kapalı kalsın? (0.1 idealdir)
    public float minBlinkInterval = 5f; // En az kaç saniyede bir kırpsın?
    public float maxBlinkInterval = 10f; // En çok kaç saniyede bir kırpsın?

    // Fare takip scriptine ulaşmamız gerekebilir (Opsiyonel)
    // private UIOverlayEyeFollow followScript; 

    void Start()
    {
        // followScript = GetComponent<UIOverlayEyeFollow>();

        // Oyuna başlar başlamaz göz kırpma Coroutine'ini başlat
        if (leftPupilGO != null && rightPupilGO != null)
        {
            StartCoroutine(BlinkingRoutine());
        }
        else
        {
            Debug.LogError("ZorboBlinker: Göz bebekleri atanmadı!");
        }
    }

    IEnumerator BlinkingRoutine()
    {
        // Oyun devam ettiği sürece döngü çalışır
        while (true)
        {
            // 1. Rastgele bir süre bekle (Rastgelelik canlılık katar)
            float randomWait = Random.Range(minBlinkInterval, maxBlinkInterval);
            yield return new WaitForSeconds(randomWait);

            // 2. Göz Kırpma Başlıyor (Kapat)
            Blink(true);

            // Gerekirse takip scriptini durdur (Kapatılınca hareket etmesinler)
            // if (followScript) followScript.enabled = false;

            // 3. 0.1 saniye bekle (Kapalı kalma süresi)
            yield return new WaitForSeconds(blinkDuration);

            // 4. Göz Kırpma Bitti (Aç)
            Blink(false);

            // Gerekirse takip scriptini tekrar aç
            // if (followScript) followScript.enabled = true;
        }
    }

    void Blink(bool close)
    {
        // Göz bebeklerini kapatıp açar
        leftPupilGO.SetActive(!close);
        rightPupilGO.SetActive(!close);
   
    }
}