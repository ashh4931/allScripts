using UnityEngine;
using System.Collections;

public class SnakeSegment : MonoBehaviour, IDamageable
{
    private NewSnakeAI headAI;
    private float currentHealth;
    private bool isDead = false;

    [Header("Görsel Geri Bildirim")]
    private SpriteRenderer sr;
    private Color originalColor;

    private float damageTimer;
    private float damageInterval = 0.5f; 

    public void Setup(NewSnakeAI head, float health)
    {
        headAI = head;
        currentHealth = health;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
    }

    // --- OYUNCUYA HASAR VERME (Hata buradaydı) ---
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isDead || headAI == null) return;

        if (collision.CompareTag("Player"))
        {
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0)
            {
                StatController stats = collision.GetComponent<StatController>();
                if (stats != null)
                {
                    // 🔴 DİKKAT: baseData.baseDamage yerine headAI.GetDamageValue() kullanıyoruz!
                    stats.TakeDamage(headAI.GetDamageValue());
                    
                    // Oyuncu hasar alınca Hitstop tetiklensin
                    headAI.TriggerHitStop(0.07f);
                    
                    damageTimer = damageInterval;
                }
            }
        }
    }

    public void TakeDamage(float damage, float minDamage = 0f, float maxDamage = 0f)
    {
        if (isDead) return;
        currentHealth -= damage;

        if (gameObject.activeInHierarchy)
        {
            StopAllCoroutines();
            StartCoroutine(HitFeedbackRoutine());
        }

        if (currentHealth <= 0) SegmentDie();
    }

    private IEnumerator HitFeedbackRoutine()
    {
        if (sr == null) yield break;
        sr.color = Color.white;
        Vector3 posBeforeShake = transform.localPosition;
        float elapsed = 0f;
        while (elapsed < 0.1f)
        {
            transform.localPosition = posBeforeShake + (Vector3)Random.insideUnitCircle * 0.1f;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = posBeforeShake;
        sr.color = originalColor;
    }

    void SegmentDie()
    {
        if (isDead) return;
        isDead = true;

        if (headAI != null)
        {
            // Parça patlarken hafif Hitstop
            headAI.TriggerHitStop(0.05f);
            headAI.OnSegmentDestroyed(this.transform);

            var data = headAI.GetSnakeData();
            if (data.explosionEffect != null) Instantiate(data.explosionEffect, transform.position, Quaternion.identity);
            if (data.explosionSound != null) AudioManager.instance.PlaySFXAtPosition(data.explosionSound, transform.position, 0.7f);
        }

        Destroy(gameObject);
    }
}