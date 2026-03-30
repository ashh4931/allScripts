using UnityEngine;

public class spell : MonoBehaviour
{
    [Header("Hareket ve Yaşam Süresi")]
    public float speed = 10f;
    public float lifeTime = 2f;

    [Header("Salınım Ayarları")]
    public float amplitude = 0.5f; // Yukarı-aşağı salınım miktarı
    public float frequency = 5f;   // Salınım hızı

    [Header("Patlama ve Hasar (AoE)")]
    public float totalDamage = 50f;     // Alandaki hedeflere BÖLÜNECEK toplam hasar
    public float minDamage = 5f;        // IDamageable arayüzünün gereksinimi için
    public float explosionRadius = 3f;  // Patlama etki alanı
    public float knockbackForce = 10f;  // İtme gücü şiddeti
    public LayerMask hitLayer;          // Hangi katmanların hasar alacağını seçmek için (örn: Enemies)

    [Header("Görsel ve İşitsel Efektler")]
    public GameObject[] explosionPrefabs;
    public AudioClip[] explosionSounds;

    private float spawnTime;
    private bool hasExploded = false; // Çift patlamayı önlemek için

    void Start()
    {
        spawnTime = Time.time;
        // lifeTime süresi dolunca patla
        Invoke("PlayExplosion", lifeTime);
    }

    void Update()
    {
        // İleri hareket
        Vector3 movement = Vector3.right * speed * Time.deltaTime;

        float elapsed = Time.time - spawnTime;

        // Salınım ekle
        Vector3 sideStep = transform.up * Mathf.Sin(elapsed * frequency) * amplitude;
        transform.Translate(movement + sideStep * Time.deltaTime);
    }

    // İsteğe bağlı: Bir düşmana veya duvara çarparsa süreyi beklemeden patlaması için
    void OnTriggerEnter2D(Collider2D other)
    {
        // Kendi karakterimize çarpıp hemen patlamasını engellemek için tag veya layer kontrolü yapabilirsin
        if (other.CompareTag("Player"))
            return;
        if (!hasExploded && other.GetComponent<IDamageable>() != null)
        {
            CancelInvoke("PlayExplosion"); // Süreli patlamayı iptal et
            PlayExplosion();               // Hemen patla
        }
    }

    void PlayExplosion()
    {
        if (hasExploded) return;
        hasExploded = true;

        // 1️⃣ Görsel Efekt
        if (explosionPrefabs != null && explosionPrefabs.Length > 0)
        {
            int index = Random.Range(0, explosionPrefabs.Length);
            Instantiate(explosionPrefabs[index], transform.position, Quaternion.identity);
        }

        // 2️⃣ Ses Efekti
        if (explosionSounds != null && explosionSounds.Length > 0)
        {
            int soundIndex = Random.Range(0, explosionSounds.Length);
            AudioClip clip = explosionSounds[soundIndex];

            GameObject soundObj = new GameObject("ExplosionSound");
            soundObj.transform.position = transform.position;

            AudioSource s = soundObj.AddComponent<AudioSource>();
            s.clip = clip;
            s.pitch = Random.Range(0.7f, 1.4f);
            s.volume = Random.Range(0.5f, 0.8f);
            s.spatialBlend = 0f;
            s.Play();

            Destroy(soundObj, clip.length / s.pitch);
        }

        // 3️⃣ ALAN HASARI VE İTME (AoE & Knockback)
        ApplyExplosionDamageAndKnockback();

        // Spell objesini yok et
        Destroy(gameObject);
    }

    private void ApplyExplosionDamageAndKnockback()
    {
        // Patlama alanındaki tüm objeleri bul (hitLayer ile sadece belirli katmanları filtreleyebilirsin)
        Collider2D[] collidersInRadius = Physics2D.OverlapCircleAll(transform.position, explosionRadius, hitLayer);

        // Önce kaç tane "Hasar Alabilir" (IDamageable) obje olduğunu sayalım
        int damageableCount = 0;
        foreach (Collider2D hit in collidersInRadius)
        {
            if (hit.GetComponent<IDamageable>() != null)
            {
                damageableCount++;
            }
        }

        // Eğer alanda kimse yoksa işlemi bitir
        if (damageableCount == 0) return;

        // Hasarı böl
        float splitDamage = totalDamage / damageableCount;
        // Hasar minimum değerin altına düşmesin diye kontrol ekliyoruz
        splitDamage = Mathf.Max(splitDamage, minDamage);

        // Şimdi hasarı ve itme gücünü uygula
        foreach (Collider2D hit in collidersInRadius)
        {
            IDamageable dmg = hit.GetComponent<IDamageable>();

            if (dmg != null)
            {
                // Hasar ver (senin bullet.cs mantığına uygun imza ile)
                dmg.TakeDamage(splitDamage, minDamage, totalDamage);

                // İtme Gücü (Knockback) uygula
                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // Patlama merkezinden hedefe doğru olan yönü bul
                    Vector2 knockbackDirection = (hit.transform.position - transform.position).normalized;

                    // Hedefe ani bir güç (Impulse) uygula
                    rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
                }
            }
        }
    }

    // Editörde patlama yarıçapını görebilmen için çizim aracı
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}