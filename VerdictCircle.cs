using UnityEngine;

public class VerdictCircle : MonoBehaviour
{
    [Header("Referanslar")]
    public GameObject lightningPrefab;
    public GameObject strikeParticlePrefab; // 🔴 YENİ: Yerdeki patlama efekti
    public AudioClip spawnSound;
    public AudioClip lightningSound;

    private float damage;
    private float radius;
    private float lightningHeight;

    public void Setup(float dmg, float rad, float height)
    {
        damage = dmg;
        radius = rad;
        lightningHeight = height;

        if (spawnSound != null && AudioManager.instance != null)
        {
            // Çemberin doğuş sesi[cite: 6, 11]
            AudioManager.instance.PlaySFXAtPosition(spawnSound, transform.position);
        }
    }

    public void Strike()
    {
        // 1. Yıldırım Sesini Çal[cite: 5, 11]
        if (lightningSound != null && AudioManager.instance != null)
        {
            float randomPitch = Random.Range(0.85f, 1.15f);
            AudioManager.instance.PlaySFXAtPosition(lightningSound, transform.position, 1f, randomPitch);
        }

        // 2. Yıldırım Görselini Yarat
        if (lightningPrefab != null)
        {
            Vector3 lightningPos = transform.position + Vector3.up * lightningHeight;
            Instantiate(lightningPrefab, lightningPos, Quaternion.identity);
        }

        // 3. 🔴 YENİ: Partikül Sistemini Yarat
        if (strikeParticlePrefab != null)
        {
            // Tam çemberin olduğu noktada patlama efekti çıkar
            Instantiate(strikeParticlePrefab, transform.position, Quaternion.identity);
        }

        // 4. Hasar Uygula[cite: 16]
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable dmgInterface = enemy.GetComponent<IDamageable>();
            if (dmgInterface != null)
            {
                // Düşman sistemine hasarı uygula[cite: 11, 16]
                dmgInterface.TakeDamage(damage);
            }
        }

        // Her şey bittikten sonra çemberi yok et
        Destroy(gameObject, 0.5f);
    }
}