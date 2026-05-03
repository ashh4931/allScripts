using UnityEngine;
using System.Collections;

public class SpriteBootGlitch : MonoBehaviour
{
    [Header("Boot Ayarları")]
    [Tooltip("Karakterin ekrana gelme süresi (HUD ile aynı yap)")]
    public float bootDuration = 2.0f; 
    
    [Tooltip("HUD scriptindeki aynı eğriyi kullanıyoruz")]
    public AnimationCurve alphaCurve = new AnimationCurve(
        new Keyframe(0f, 0f), 
        new Keyframe(0.2f, 0.4f), 
        new Keyframe(0.4f, 0.1f), 
        new Keyframe(0.7f, 0.8f), 
        new Keyframe(0.85f, 0.4f),
        new Keyframe(1f, 1f)
    );

    [Header("Glitch Renkleri")]
    public Color[] glitchColors = { Color.cyan, Color.magenta, Color.yellow, Color.red, Color.white };

    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;

    void Awake()
    {
        // Oyuncunun altındaki tüm SpriteRenderer'ları (Visual dâhil) otomatik bulur
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[spriteRenderers.Length];
        
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            // Orijinal renkleri hafızaya al
            originalColors[i] = spriteRenderers[i].color;
            
            // Başlangıçta görünmez yap (Alpha = 0)
            Color c = originalColors[i];
            c.a = 0f;
            spriteRenderers[i].color = c;
        }
    }

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
            float t = elapsed / bootDuration;
            
            // Eğriden o anki Alpha (şeffaflık) değerini al
            float currentAlpha = alphaCurve.Evaluate(t);

            if (t < 0.85f)
            {
                // 4 karede bir renk değiştir (HUD ile aynı mantık)
                if (Time.frameCount % 4 == 0) 
                {
                    for (int i = 0; i < spriteRenderers.Length; i++)
                    {
                        Color randColor = glitchColors[Random.Range(0, glitchColors.Length)];
                        randColor.a = currentAlpha; // Alphayı uygula
                        spriteRenderers[i].color = randColor;
                    }
                }
                else
                {
                    // Renk değişmese bile Alpha'yı sürekli güncelle
                    for (int i = 0; i < spriteRenderers.Length; i++)
                    {
                        Color c = spriteRenderers[i].color;
                        c.a = currentAlpha;
                        spriteRenderers[i].color = c;
                    }
                }
            }
            else 
            {
                // Son kısımda yavaşça orijinal renklere dön
                for (int i = 0; i < spriteRenderers.Length; i++)
                {
                    Color c = originalColors[i];
                    c.a = currentAlpha;
                    spriteRenderers[i].color = c;
                }
            }

            yield return null;
        }

        // Bitişte her şeyi tamamen orijinal (mat) haline döndür
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].color = originalColors[i];
        }
    } 
}