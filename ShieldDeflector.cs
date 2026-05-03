using UnityEngine;

public class ShieldDeflector : MonoBehaviour
{
    [Header("Bağlantılar")]
    public PlayerShieldController shieldController; 
    [Header("Görsel Efektler")]
    public GameObject hitEffectPrefab; 
    [Header("Parry (Yansıtma) Ayarları")]
    public float parryWindow = 0.5f; 
    public AudioClip blockSound;
    public AudioClip parrySound;
    private AudioSource audioSource;

    [Header("Kırık Parça (Debris) Ayarları")]
    public GameObject[] debrisPrefabs;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            float timeSinceActivation = Time.time - shieldController.activationTime;

            if (timeSinceActivation <= parryWindow)
            {
                ParryBullet(other.gameObject);
            }
            else
            {
                BlockBullet(other.gameObject);
            }
        }
    }

    void ParryBullet(GameObject bullet)
    {
        if (audioSource != null && parrySound != null) audioSource.PlayOneShot(parrySound);

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = -bulletRb.linearVelocity * 1.5f;
            bullet.tag = "PlayerBullet"; 
            bullet.layer = LayerMask.NameToLayer("PlayerBullet"); 
        }
    }

void BlockBullet(GameObject bullet)
    {
        // Çarpan mermiden hasar değerini almayı deneyelim (Eğer mermide hasar kodu varsa)
        // Şimdilik varsayılan olarak controller'daki hitDrainAmount değerini kullanıyoruz.
        // İleride "Sniper Mermisi" yaparsan buraya o merminin hasarını yollayabilirsin!
        float incomingDamage = shieldController.hitDrainAmount; 
        
        // 🔴 YENİ: Kalkanın kırılıp kırılmadığını kontrol et
        bool isBroken = shieldController.TakeShieldDamage(incomingDamage);

        if (isBroken)
        {
            // KALKAN KIRILDI!
            // Çok daha şiddetli ve fazla sayıda parça dök (Örn: 8-12 parça)
            SpawnDebris(bullet.transform.position, 8, 12);
        }
        else
        {
            // NORMAL BLOKLAMA
            if (audioSource != null && blockSound != null) audioSource.PlayOneShot(blockSound);
            
            // Az sayıda parça dök (Örn: 1-3 parça)
            SpawnDebris(bullet.transform.position, 1, 3);
        }

        // Çarpma partikülünü yarat
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, bullet.transform.position, Quaternion.identity);
        }

        Destroy(bullet);
    }

    // 🔴 GÜNCELLENDİ: Artık kaç parça döküleceğini parametre olarak alıyor
    void SpawnDebris(Vector3 hitPoint, int minCount, int maxCount)
    {
        if (debrisPrefabs == null || debrisPrefabs.Length == 0) return;

        int debrisCount = Random.Range(minCount, maxCount + 1);

        for (int i = 0; i < debrisCount; i++)
        {
            GameObject randomDebris = debrisPrefabs[Random.Range(0, debrisPrefabs.Length)];
            GameObject spawnedDebris = Instantiate(randomDebris, hitPoint, Quaternion.identity);

            Rigidbody2D rb = spawnedDebris.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Rastgele saçılma
                Vector2 fakeGravityDir = new Vector2(Random.Range(-3f, 3f), -Random.Range(4f, 8f));
                rb.linearVelocity = fakeGravityDir;
                rb.angularVelocity = Random.Range(-400f, 400f);
            }

            Destroy(spawnedDebris, 1.5f);
        }
    }
}