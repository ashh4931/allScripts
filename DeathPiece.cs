using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class DeathPiece : MonoBehaviour
{
    public float lifeTime = 2f;      // Yerde ne kadar kalacak?
    public float fadeDuration = 1f;  // Kaç saniyede solup yok olacak?

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        
        // Fırlatıldıktan kısa bir süre sonra yavaşlayıp durması için sürtünme ekliyoruz
        rb.linearDamping = 3f; 

        StartCoroutine(FadeAndDestroy());
    }

    private IEnumerator FadeAndDestroy()
    {
        // Belirlenen süre kadar bekle
        yield return new WaitForSeconds(lifeTime);

        // Yavaşça saydamlaş (Alpha değerini 1'den 0'a düşür)
        Color startColor = spriteRenderer.color;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        // Tamamen görünmez olunca objeyi sahneden sil
        Destroy(gameObject);
    }
}