using UnityEngine;

public class spell : MonoBehaviour
{
    public enum ElementType { None, Fire, Ice, Electric, Poison }
    [Header("Hareket ve Yaşam Süresi")]
    public float speed = 10f;
    public float lifeTime = 2f;
    [Header("Element Ayarları")]
    public ElementType spellElement;
    public float effectDuration = 3f;  // Efekt ne kadar sürecek?
    [Range(0, 1)] public float slowAmount = 0.5f; // Buz için yavaşlatma oranı
    public float dotDamage = 5f;       // Ateş/Zehir için saniyelik hasar
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
    // 1. Oyuncuya çarpınca patlamasın (kendi mermimizden hasar almayalım)
    if (other.CompareTag("Player"))
        return;

    // 2. Eğer zaten patlamışsa (aynı karede birden fazla yere çarpabilir) durdur
    if (hasExploded) return;

    // 3. Düşman mı yoksa engel mi kontrolü
    bool hitEnemy = other.GetComponent<IDamageable>() != null;
    
    // Duvar/Engel kontrolü: "Default" layerı genelde duvarlardır, 
    // veya özel bir "Environment" layer'ın varsa onu da kontrol edebilirsin.
    bool hitWall = other.gameObject.layer == LayerMask.NameToLayer("Default") || 
                   other.gameObject.layer == LayerMask.NameToLayer("Environment");

    if (hitEnemy || hitWall)
    {
        // Invoke ile planlanmış patlamayı iptal et (zamanı dolmadan çarptığı için)
        CancelInvoke("PlayExplosion"); 
        
        // Patlama rutinini başlat
        PlayExplosion();
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
        // Patlama alanındaki tüm objeleri bul
        Collider2D[] collidersInRadius = Physics2D.OverlapCircleAll(transform.position, explosionRadius, hitLayer);

        int damageableCount = 0;
        foreach (Collider2D hit in collidersInRadius)
        {
            if (hit.GetComponent<IDamageable>() != null)
            {
                damageableCount++;
            }
        }

        if (damageableCount == 0) return;

        float splitDamage = totalDamage / damageableCount;
        splitDamage = Mathf.Max(splitDamage, minDamage);

        // Şimdi hasarı ve itme gücünü uygula
        foreach (Collider2D hit in collidersInRadius)
        {
            IDamageable dmg = hit.GetComponent<IDamageable>();

            // --- HATA BURADAYDI, BU SATIRI EKLEDİK ---
            NPCStatusManager status = hit.GetComponent<NPCStatusManager>();
            // ----------------------------------------

            if (dmg != null)
            {
                dmg.TakeDamage(splitDamage, minDamage, totalDamage);

                // status değişkeni artık tanımlı olduğu için hata vermeyecek
                if (status != null)
                {
                    ApplyElementEffect(status);
                }

                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 knockbackDirection = (hit.transform.position - transform.position).normalized;
                    rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
                }
            }
        }
    }

    private void ApplyElementEffect(NPCStatusManager status)
    {
        switch (spellElement)
        {
            case ElementType.Ice:
                status.ApplyIceEffect(effectDuration, slowAmount);
                break;
            case ElementType.Fire:
                status.ApplyBurnEffect(effectDuration, dotDamage);
                break;
            case ElementType.Electric:
                status.ApplyStunEffect(effectDuration);
                break;
            case ElementType.Poison:
                status.ApplyPoisonEffect(effectDuration, dotDamage);
                break;
        }
    }
    // Editörde patlama yarıçapını görebilmen için çizim aracı
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}