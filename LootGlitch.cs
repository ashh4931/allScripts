using UnityEngine;
using System.Collections;

public class LootGlitch : MonoBehaviour
{
    [Header("Glitch Ayarları")]
    public float duration = 1.0f; // 1 saniye sürecek
    public Color[] glitchColors = { Color.cyan, Color.magenta, Color.yellow, Color.white };
    
    private SpriteRenderer[] renderers;
    private Color[] originalColors;

    void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].color;
            // Başlangıçta görünmez yapıyoruz
            Color c = originalColors[i];
            c.a = 0f;
            renderers[i].color = c;
        }
    }

    void Start()
    {
        StartCoroutine(GlitchRoutine());
    }

    IEnumerator GlitchRoutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // %80'ine kadar çılgınca renk değiştir ve titreş
            if (t < 0.8f)
            {
                if (Time.frameCount % 2 == 0) // Daha hızlı glitch (2 karede bir)
                {
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        Color randColor = glitchColors[Random.Range(0, glitchColors.Length)];
                        randColor.a = Random.Range(0.5f, 1f); // Rastgele şeffaflık
                        renderers[i].color = randColor;
                    }
                }
            }
            else
            {
                // Son %20'lik kısımda orijinal rengine yumuşakça dön
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].color = Color.Lerp(renderers[i].color, originalColors[i], (t - 0.8f) * 5f);
                }
            }

            yield return null;
        }

        // Garanti olsun diye en son her şeyi düzelt
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].color = originalColors[i];
        }
    }
}