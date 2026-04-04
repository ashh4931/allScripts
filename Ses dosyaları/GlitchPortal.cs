using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class AdvancedGlitchAnim : MonoBehaviour
{
    [Header("Görsel Katmanlar")]
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

    // Glitch şiddetini kontrol eden çarpan (1 = Tam şiddet, 0 = Durmuş)
    private float glitchIntensityMultiplier = 1f;
    private bool isFadingOut = false;

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
        glitchIntensityMultiplier = 1f;
        isFadingOut = false;
        
        if (chromaticAberration != null) originalCAIntensity = chromaticAberration.intensity.value;
        if (lensDistortion != null) originalLDIntensity = lensDistortion.intensity.value;

        StartCoroutine(ChaosGlitchRoutine());
    }

    IEnumerator ChaosGlitchRoutine()
    {
        // isFadingOut olsa bile şiddet 0 olana kadar devam etsin
        while (glitchIntensityMultiplier > 0.01f)
        {
            for (int i = 0; i < glitchLayers.Length; i++)
            {
                // Rastgeleliği şiddet çarpanıyla çarpıyoruz
                float randomXScale = originalScales[i].x * Random.Range(1f, 1f + (3.5f * glitchIntensityMultiplier));
                float randomYScale = originalScales[i].y * Random.Range(1f - (0.2f * glitchIntensityMultiplier), 1f + (0.2f * glitchIntensityMultiplier));
                glitchLayers[i].transform.localScale = new Vector3(randomXScale, randomYScale, originalScales[i].z);

                float randomXOffset = Random.Range(-0.2f, 0.2f) * glitchIntensityMultiplier;
                glitchLayers[i].transform.localPosition = new Vector3(originalPositions[i].x + randomXOffset, originalPositions[i].y, originalPositions[i].z);

                Color randColor = originalColors[i];
                // Hem rastgelelik hem de genel şiddet alfayı etkilesin
                randColor.a = Random.Range(0.2f, 1f) * (isFadingOut ? glitchIntensityMultiplier : 1f);
                glitchLayers[i].color = randColor;
            }

            if (chromaticAberration != null) 
                chromaticAberration.intensity.value = Mathf.Lerp(originalCAIntensity, Random.Range(0.5f, 1f), glitchIntensityMultiplier);
            
            if (lensDistortion != null) 
                lensDistortion.intensity.value = Mathf.Lerp(originalLDIntensity, Random.Range(-0.4f, 0.4f), glitchIntensityMultiplier);

            yield return new WaitForSeconds(Random.Range(0.01f, 0.06f));
        }
    }

    public void StopGlitchSmoothly(float fadeDuration)
    {
        if (!isFadingOut)
        {
            isFadingOut = true;
            StartCoroutine(FadeOutRoutine(fadeDuration));
        }
    }

    IEnumerator FadeOutRoutine(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            // Şiddeti 1'den 0'a düşür
            glitchIntensityMultiplier = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            yield return null; 
        }

        glitchIntensityMultiplier = 0f;
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        // Değerleri sıfırla
        ResetToOriginals();
    }

    private void ResetToOriginals()
    {
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