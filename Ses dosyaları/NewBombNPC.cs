using UnityEngine;
using System.Collections;

public class NewBombNPC : NewBaseEnemyAI
{
    private NewBombEnemyData bombData;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    // Patlama durumu kontrolleri
    private bool isHeatingUp = false;
    private bool hasExploded = false;
    private float heatTimer = 0f;

    protected override void Start()
    {
        base.Start(); // Base sınıftaki (rb, anim, player) atamalarını yap.

        // Datayı kendi tipimize çeviriyoruz (Cast işlemi)
        bombData = baseData as NewBombEnemyData;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>(); // Sesi çalmak için

        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    protected override void Update()
    {
        // Eğer patladıysa veya öldüyse hiçbir şey yapma
        if (hasExploded || isDead) return;

        base.Update(); // Base sınıftaki mesafe kontrolünü (CheckPlayerDistance) çalıştırır.

        // Eğer saldırı menzilinden çıktıysa ve henüz tam patlamadıysa soğuma yapabiliriz
        if (!isAttacking && isHeatingUp)
        {
            CoolDown();
        }
    }

    // 🔴 HAREKET: Base sınıf bunu biz kovalama modundayken otomatik çağıracak
    protected override void HandleMovement()
    {
        // Eğer ısınıyorsa (patlamak üzereyse) hareket etmesin
       

        // Dümdüz oyuncuya doğru git
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * bombData.moveSpeed;

        UpdateAnimation(direction);
    }

    // 🔴 SALDIRI: Base sınıf biz attackRange (saldırı menzili) içine girince bunu otomatik çağıracak
    protected override void HandleAttack()
    {
        if (hasExploded) return;

        isHeatingUp = true;
        heatTimer += Time.deltaTime;

        // Renk değiştirme (Isınma efekti)
        float t = Mathf.Clamp01(heatTimer / bombData.heatUpTime);
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(Color.white, bombData.warningColor, t);
        }

        // Isınma süresi dolduysa patlama rutinini başlat
        if (t >= 1f && !hasExploded)
        {
            hasExploded = true;
            StartCoroutine(ExplodeRoutine());
        }
    }

    private void CoolDown()
    {
        // Oyuncu menzilden çıkarsa bombanın ısısı düşsün
        heatTimer -= Time.deltaTime;
        heatTimer = Mathf.Clamp(heatTimer, 0f, bombData.heatUpTime);

        float t = heatTimer / bombData.heatUpTime;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(Color.white, bombData.warningColor, t);
        }

        if (heatTimer <= 0f)
        {
            isHeatingUp = false;
        }
    }

    private IEnumerator ExplodeRoutine()
    {
        rb.linearVelocity = Vector2.zero; // Kesinlikle dur

        // Patlamadan hemen önceki küçük gecikme
        yield return new WaitForSeconds(bombData.detonationDelay);

        // --- PATLAMA HASAR VE İTME MANTIĞI ---
        Vector2 explosionCenter = transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(explosionCenter, bombData.explosionRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // 1. HASAR HESABI
                float distance = Vector2.Distance(explosionCenter, hit.transform.position);
                float normalized = Mathf.Clamp01(distance / bombData.explosionRadius);
                float damageFactor = 1f - (normalized * normalized);
                float finalDamage = bombData.minDamage + (bombData.baseDamage - bombData.minDamage) * damageFactor;

                // Hasar verme (StatController scriptin varsa)
                StatController stat = hit.GetComponent<StatController>();
                if (stat != null)
                {
                    stat.TakeDamage(Mathf.RoundToInt(finalDamage));
                }

                // 2. KNOCKBACK (İTME) MANTIĞI - Yorum satırlarını kaldırdık!
                PlayerController playerController = hit.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    Vector2 knockbackDirection = (Vector2)hit.transform.position - explosionCenter;
                    Vector2 force = knockbackDirection.normalized * bombData.knockbackForce;

                    playerController.ApplyKnockback(force);
                    Debug.Log("Oyuncuya knockback uygulandı: " + force);
                }

                if (bombData.knockbackSound != null) audioSource.PlayOneShot(bombData.knockbackSound);
            }
        }

        // --- GÖRSEL VE SES ---
        if (bombData.explosionEffect != null) Instantiate(bombData.explosionEffect, transform.position, Quaternion.identity);
        if (bombData.explosionSound != null) audioSource.PlayOneShot(bombData.explosionSound);

        // --- YOK OLMA ---
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;

        Destroy(gameObject, 1f); // Sesi çalması için 1 saniye sonra tamamen sil
    }

    // Editörde patlama alanını görmek için (Sadece Geliştirici Ekranında Çizilir)
    private void OnDrawGizmosSelected()
    {
        if (bombData != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, bombData.explosionRadius);
        }
    }
}