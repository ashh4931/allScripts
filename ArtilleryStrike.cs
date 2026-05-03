using UnityEngine;
using System.Collections;

public class ArtilleryStrike : MonoBehaviour
{
    [Header("Görsel Referanslar")]
    public SpriteRenderer indicatorSprite; // Yerdeki kırmızı alan (Çember veya Çizgi)
    public Transform fallingRocketVisual;  // Gökten inen roket görseli (Başlangıçta yukarıda olmalı)

    private float indicatorDelay;
    private float fallDelay;
    private float radius;
    private float baseDamage;
    private float minDamage;
    private float knockbackForce;
    private GameObject explosionVFX;
    private AudioClip indicatorSound;
    private AudioClip explosionSound;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        if (indicatorSprite != null) indicatorSprite.enabled = false;
        if (fallingRocketVisual != null) fallingRocketVisual.gameObject.SetActive(false);
    }

    // AI roket ateşleyince bu fonksiyonu çağırıp tüm ayarları yollayacak
    public void SetupStrike(NewRocketEnemyData data, bool isLinePattern, Vector2 lineDirection)
    {
        indicatorDelay = data.indicatorDelay;
        fallDelay = data.fallDelay;
        radius = data.explosionRadius;
        baseDamage = data.baseDamage;
        minDamage = data.minDamage;
        knockbackForce = data.knockbackForce;
        explosionVFX = data.explosionVFX;
        indicatorSound = data.indicatorSound;
        explosionSound = data.explosionSound;

        // Eğer çizgi şeklindeyse, kırmızı hedef sprite'ını o yöne çevir (Eğer elips/çizgi şeklinde bir sprite kullanıyorsan)
        if (isLinePattern && indicatorSprite != null)
        {
            float angle = Mathf.Atan2(lineDirection.y, lineDirection.x) * Mathf.Rad2Deg;
            indicatorSprite.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Çember boyutunu patlama alanına eşitle (Görseli tam oturtmak için)
        if (indicatorSprite != null)
        {
            indicatorSprite.transform.localScale = new Vector3(radius * 2, radius * 2, 1f);
        }

        StartCoroutine(StrikeRoutine());
    }

    private IEnumerator StrikeRoutine()
    {
        // 1. Bekleme Süresi (Roket havada süzülüyor hissiyatı)
        yield return new WaitForSeconds(indicatorDelay);

        // 2. Kırmızı Alanı Aç ve Uyarı Sesi Çal
        if (indicatorSprite != null) indicatorSprite.enabled = true;
        if (indicatorSound != null) audioSource.PlayOneShot(indicatorSound);

        // 3. Roketin Düşüşü
        if (fallingRocketVisual != null)
        {
            fallingRocketVisual.gameObject.SetActive(true);
            Vector3 startPos = fallingRocketVisual.localPosition + new Vector3(0, 15f, 0); // 15 birim yukarıdan başla
            Vector3 targetPos = Vector3.zero; // Ana objenin merkezi
            float fallTimer = 0f;

            while (fallTimer < fallDelay)
            {
                fallTimer += Time.deltaTime;
                float t = fallTimer / fallDelay;
                // Aşağı doğru hızlanarak inmesi için t * t (Quadratic) kullanıyoruz
                fallingRocketVisual.localPosition = Vector3.Lerp(startPos, targetPos, t * t);
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(fallDelay); // Görsel yoksa sadece bekle
        }

        // 4. PATLAMA!
        Explode();
    }

    private void Explode()
    {
        // BombNPC'deki hasar mantığının aynısı
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                float normalized = Mathf.Clamp01(distance / radius);
                float damageFactor = 1f - (normalized * normalized);
                float finalDamage = minDamage + (baseDamage - minDamage) * damageFactor;

                StatController stat = hit.GetComponent<StatController>();
                if (stat != null) stat.TakeDamage(Mathf.RoundToInt(finalDamage));

                PlayerController playerController = hit.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    Vector2 knockbackDirection = (Vector2)hit.transform.position - (Vector2)transform.position;
                    playerController.ApplyKnockback(knockbackDirection.normalized * knockbackForce);
                }
            }
        }

        // Görsel ve Sesleri Oynat
        if (explosionVFX != null) Instantiate(explosionVFX, transform.position, Quaternion.identity);
        if (explosionSound != null) AudioManager.instance.PlaySFXAtPosition(explosionSound, transform.position, 1f);

        // Temizlik (Sesin yarım kesilmemesi için sprite'ları kapatıp 1 sn sonra yok et)
        if (indicatorSprite != null) indicatorSprite.enabled = false;
        if (fallingRocketVisual != null) fallingRocketVisual.gameObject.SetActive(false);
        Destroy(gameObject, 1.5f);
    }
}