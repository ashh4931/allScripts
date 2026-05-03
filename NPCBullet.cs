using UnityEngine;

public class NPCBullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;
    public float maxDamage = 20f;
    public float minDamage = 5f; 
    public float damageDropOffIntensity = 0.5f; 

    public GameObject hitEffectPrefab;
    public AudioClip hitmark;

    [Header("Penetration")]
    public int penetration = 1;

    private Vector2 startPosition; 
    
    // --- 🔴 ÖMÜR (LIFETIME) KONTROL DEĞİŞKENLERİ ---
    private float currentAge = 0f;
    private float agingSpeed = 1f; // 1 normal hızda yaşlanır, düştükçe ömrü uzar

    void Start()
    {
        startPosition = transform.position; 
        // 🔴 ESKİ KODDAKİ "Destroy(gameObject, lifeTime);" SATIRINI SİLDİK!
    }

    void Update()
    {
        // 1. Hareket
        transform.Translate(Vector2.right * speed * Time.deltaTime);

        // 2. 🔴 YENİ: Manuel Yaşlanma Mantığı
        currentAge += Time.deltaTime * agingSpeed;
        if (currentAge >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    [Range(0f, 1f)] public float hitmarkVolume = 0.5f; 

    private void playsoundeffect()
    {
        if (hitmark != null && AudioManager.instance != null)
        {
           AudioManager.instance.PlaySFXAtPosition(hitmark, Camera.main.transform.position, hitmarkVolume);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return; 
        }
        IDamageable dmg = other.GetComponent<IDamageable>();

        if (dmg != null)
        {
            float distanceTraveled = Vector2.Distance(startPosition, transform.position);
            float calculatedDamage = maxDamage - (distanceTraveled * damageDropOffIntensity);
            calculatedDamage = Mathf.Max(calculatedDamage, minDamage);

            dmg.TakeDamage(calculatedDamage, minDamage, maxDamage);

            playsoundeffect();
            penetration--;

            if (other.CompareTag("DestructibleObject"))
            {
                penetration -= 3;
            }

            if (hitEffectPrefab != null) Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

            if (penetration <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    // --- 🔴 ZAMAN YIRTIĞI DESTEĞİ ---
    private float originalSpeed;
    private bool isTimeSlowed = false;

    public void ApplyTimeSlow(float slowPercentage)
    {
        if (isTimeSlowed) return;
        isTimeSlowed = true;
        originalSpeed = speed;

        float timeMultiplier = (1f - slowPercentage); // Örn: %70 yavaşlarsa çarpan 0.3 olur
        speed *= timeMultiplier;
        
        // 🔴 YENİ: Merminin yaşlanma hızını da yavaşlatıyoruz!
        // Böylece mermi %70 yavaşlarsa, %70 daha geç yok olacak ve aynı mesafeye ulaşabilecek.
        agingSpeed = timeMultiplier; 
    }

    public void RemoveTimeSlow()
    {
        if (!isTimeSlowed) return;
        isTimeSlowed = false;
        
        speed = originalSpeed;
        agingSpeed = 1f; // Çemberden çıkınca normal hızda yaşlanmaya devam et
    }
}