using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class AdvancedGlitchAnim : MonoBehaviour
{
    [Header("Görsel Katmanlar (Sprite'lar)")]
    public SpriteRenderer[] glitchLayers;

    [Header("Post Processing Ayarları")]
    public Volume globalVolume;
    
    private Vector3[] originalScales;
    private Color[] originalColors;
    private Vector3[] originalPositions;

    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;
    private float originalCAIntensity = 0f;
    private float originalLDIntensity = 0f;

    private Coroutine chaosCoroutine; // Çalışan glitch döngüsünü tutmak için

    void Awake()
    {
        originalScales = new Vector3[glitchLayers.Length];
        originalColors = new Color[glitchLayers.Length];
        originalPositions = new Vector3[glitchLayers.Length];

        for (int i = 0; i < glitchLayers.Length; i++)
        {
            originalScales[i] = glitchLayers[i].transform.localScale;
            originalColors[i] = glitchLayers[i].color;
            originalPositions[i] = glitchLayers[i].transform.localPosition;
        }

        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out chromaticAberration);
            globalVolume.profile.TryGet(out lensDistortion);
        }
    }

    void OnEnable()
    {
        if (chromaticAberration != null) originalCAIntensity = chromaticAberration.intensity.value;
        if (lensDistortion != null) originalLDIntensity = lensDistortion.intensity.value;

        // Glitch döngüsünü başlat ve değişkene ata (daha sonra durdurabilmek için)
        chaosCoroutine = StartCoroutine(ChaosGlitchRoutine());
    }

    IEnumerator ChaosGlitchRoutine()
    {
        while (true)
        {
            for (int i = 0; i < glitchLayers.Length; i++)
            {
                float randomXScale = originalScales[i].x * Random.Range(0.1f, 4.5f);
                float randomYScale = originalScales[i].y * Random.Range(0.8f, 1.2f);
                glitchLayers[i].transform.localScale = new Vector3(randomXScale, randomYScale, originalScales[i].z);

                float randomXOffset = Random.Range(-0.2f, 0.2f);
                glitchLayers[i].transform.localPosition = new Vector3(originalPositions[i].x + randomXOffset, originalPositions[i].y, originalPositions[i].z);

                Color randColor = originalColors[i];
                randColor.a = Random.Range(0.2f, 1f);
                glitchLayers[i].color = randColor;
            }

            if (chromaticAberration != null) 
                chromaticAberration.intensity.value = Random.Range(0.5f, 1f);
            
            if (lensDistortion != null) 
                lensDistortion.intensity.value = Random.Range(-0.4f, 0.4f);

            yield return new WaitForSeconds(Random.Range(0.01f, 0.06f));
        }
    }

    // YENİ EKLENEN KISIM: YUMUŞAK KAPANMA FONKSİYONU
    public void StopGlitchSmoothly(float fadeDuration)
    {
        if (chaosCoroutine != null) StopCoroutine(chaosCoroutine); // Kaotik titremeyi durdur
        StartCoroutine(FadeOutRoutine(fadeDuration)); // Yumuşak kapanmayı başlat
    }

    IEnumerator FadeOutRoutine(float duration)
    {
        float elapsedTime = 0f;

        // O anki PP değerlerini al
        float startCA = chromaticAberration != null ? chromaticAberration.intensity.value : originalCAIntensity;
        float startLD = lensDistortion != null ? lensDistortion.intensity.value : originalLDIntensity;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration; // 0'dan 1'e doğru artan zaman oranı

            // 1. Post Process değerlerini yavaşça orijinaline döndür (Lerp)
            if (chromaticAberration != null)
                chromaticAberration.intensity.value = Mathf.Lerp(startCA, originalCAIntensity, t);
            
            if (lensDistortion != null)
                lensDistortion.intensity.value = Mathf.Lerp(startLD, originalLDIntensity, t);

            // 2. Çizgileri yavaşça şeffaflaştırarak yok et
            for (int i = 0; i < glitchLayers.Length; i++)
            {
                Color c = glitchLayers[i].color;
                c.a = Mathf.Lerp(c.a, 0f, t); // Alpha'yı (saydamlığı) yavaşça 0 yap
                glitchLayers[i].color = c;
            }

            yield return null; // Her frame (kare) bekle
        }

        // Kapanma bittiğinde objeyi tamamen kapat
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        // Obje kapanınca her ihtimale karşı değerleri temizle
        for (int i = 0; i < glitchLayers.Length; i++)
        {
            glitchLayers[i].transform.localScale = originalScales[i];
            glitchLayers[i].color = originalColors[i];
            glitchLayers[i].transform.localPosition = originalPositions[i];
        }

        if (chromaticAberration != null) chromaticAberration.intensity.value = originalCAIntensity;
        if (lensDistortion != null) lensDistortion.intensity.value = originalLDIntensity;
        
    }


}