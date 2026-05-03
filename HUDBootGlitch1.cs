using UnityEngine;
using UnityEngine.UI; // UI elemanları için gerekli
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class HUDBootGlitch : MonoBehaviour
{
    [Header("Boot Ayarları")]
    [Tooltip("UI'ın ekrana gelme süresi")]
    public float bootDuration = 2.0f; 
    
    [Tooltip("Arayüzün nefes alıp parlamasını ayarlayan grafik")]
    public AnimationCurve alphaCurve = new AnimationCurve(
        new Keyframe(0f, 0f),     // Başlangıç: Görünmez
        new Keyframe(0.2f, 0.4f), // Parladı
        new Keyframe(0.4f, 0.1f), // Söndü
        new Keyframe(0.7f, 0.8f), // Daha güçlü parladı
        new Keyframe(0.85f, 0.4f),// Tekrar hafif söndü
        new Keyframe(1f, 1f)      // Bitiş: Tamamen mat (1.0)
    );

    [Header("Glitch Renkleri")]
    public Color[] glitchColors = { Color.cyan, Color.magenta, Color.yellow, Color.red, Color.white };

    private CanvasGroup canvasGroup;
    private Graphic[] uiElements; // Tüm Text, Image, vs. elemanlarını tutar
    private Color[] originalColors;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f; // Başlangıçta kesinlikle görünmez olsun
        
        // Canvas altındaki tüm UI görsel ve yazılarını bul
        uiElements = GetComponentsInChildren<Graphic>();
        originalColors = new Color[uiElements.Length];
        
        // Orijinal renkleri hafızaya al ki sonra geri döndürebilelim
        for (int i = 0; i < uiElements.Length; i++)
        {
            originalColors[i] = uiElements[i].color;
        }
    }

    // GameIntroManager tarafından çağrılacak fonksiyon
    public void SystemOnline()
    {
        StartCoroutine(BootRoutine());
    }

    IEnumerator BootRoutine()
    {
        float elapsed = 0f;
        while (elapsed < bootDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / bootDuration; // 0'dan 1'e doğru ilerleyen zaman

            // 1. GÖRÜNÜRLÜK (ALPHA) DALGALANMASI
            // Eğrimizdeki o anki değeri alıp şeffaflığa uygula
            canvasGroup.alpha = alphaCurve.Evaluate(t);

            // 2. RENK ÇILDIRMASI (COLOR GLITCH)
            if (t < 0.85f) // Yüklemenin ilk %85'lik kısmında renkler karışık olsun
            {
                // Her karede değiştirmek göz yorar, o yüzden 4 karede bir titret
                if (Time.frameCount % 4 == 0) 
                {
                    for (int i = 0; i < uiElements.Length; i++)
                    {
                        uiElements[i].color = glitchColors[Random.Range(0, glitchColors.Length)];
                    }
                }
            }
            else 
            {
                // Yüklemenin son %15'lik kısmında yavaş yavaş orijinal renklere dön
                for (int i = 0; i < uiElements.Length; i++)
                {
                    uiElements[i].color = originalColors[i];
                }
            }

            yield return null; // Bir sonraki frame'e geç
        }

        // 3. TAMAMLANMA: Her şeyi kesin olarak normale ve %100 matlığa döndür
        canvasGroup.alpha = 1f;
        for (int i = 0; i < uiElements.Length; i++)
        {
            uiElements[i].color = originalColors[i];
        }
    }
}