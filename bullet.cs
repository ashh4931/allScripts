using UnityEngine;

public class bullet : MonoBehaviour
{
    // 1️⃣ Mermi Efektleri İçin Açılır Liste (Enum)
    public enum BulletEffectType { None, Lifesteal, Poison, Slow, Fire, Electric }

    [Header("Temel Ayarlar")]
    public float speed = 10f;
    public float lifeTime = 2f;
    public float maxDamage = 20f;
    public float minDamage = 5f;
    public float damageDropOffIntensity = 0.5f;

    [Header("Görsel/İşitsel")]
    public GameObject hitEffectPrefab;
    public AudioClip hitmark;
    [Range(0f, 1f)] public float hitmarkVolume = 0.5f;

    [Header("Penetration (Delip Geçme)")]
    public int penetration = 1;

    [Header("Mermi Efekt Ayarları")]
    public BulletEffectType effectType = BulletEffectType.None; // Inspector'dan seçilecek özellik
    public float effectDuration = 3f;  // Zehir/Buz/Ateş kaç saniye sürecek?
    public float effectDamage = 5f;    // Zehir/Ateş için saniyelik hasar
    [Range(0f, 1f)] public float slowAmount = 0.5f; // Buz için yavaşlatma oranı
    [Range(0f, 1f)] public float lifestealPercentage = 0.2f; // Can çalma oranı (örn: %20 için 0.2)

    private Vector2 startPosition;
    private bool hasPlayedSound = false;

    void Start()
    {
        startPosition = transform.position;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void playsoundeffect()
    {
        if (hasPlayedSound || hitmark == null) return;
        AudioManager.instance.PlaySFXAtPosition(hitmark, Camera.main.transform.position, hitmarkVolume);
        hasPlayedSound = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable dmg = other.GetComponent<IDamageable>();
        NPCStatusManager status = other.GetComponent<NPCStatusManager>(); // Düşmanın efekt yöneticisi

        if (dmg != null)
        {
            float distanceTraveled = Vector2.Distance(startPosition, transform.position);
            float calculatedDamage = maxDamage - (distanceTraveled * damageDropOffIntensity);
            calculatedDamage = Mathf.Max(calculatedDamage, minDamage);

            // Düşmana hasar ver
            dmg.TakeDamage(calculatedDamage, minDamage, maxDamage);

            // 2️⃣ Mermi özelliklerini uygula
            ApplyBulletEffect(status, calculatedDamage);

            playsoundeffect();

            penetration--;

            if (other.CompareTag("DestructibleObject") || other.CompareTag("Shield"))
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

    // --- ÖZELLİK UYGULAMA FONKSİYONU ---
    private void ApplyBulletEffect(NPCStatusManager status, float dealtDamage)
    {
        // A. CAN ÇALMA (Düşmanın status scriptine ihtiyaç duymaz, oyuncuyu iyileştirir)
        if (effectType == BulletEffectType.Lifesteal)
        {
            StatController playerStats = FindObjectOfType<StatController>();
            if (playerStats != null)
            {
                
                float healAmount = dealtDamage * lifestealPercentage;
                // NOT: PlayerStats scriptinde bir Heal(float miktar) fonksiyonu olmalı!
                playerStats.Heal(healAmount); 
                Debug.Log($"Can Çalma Başarılı! {healAmount} kadar can kazanıldı.");
            }
        }

        // B. DÜŞMANA VERİLEN EFEKTLER (Düşmanda NPCStatusManager olmak zorundadır)
        if (status != null)
        {
            switch (effectType)
            {
                case BulletEffectType.Poison:
                    status.ApplyPoisonEffect(effectDuration, effectDamage);
                    break;
                case BulletEffectType.Slow:
                    status.ApplyIceEffect(effectDuration, slowAmount);
                    break;
                case BulletEffectType.Fire:
                    status.ApplyBurnEffect(effectDuration, effectDamage);
                    break;
                case BulletEffectType.Electric:
                    status.ApplyStunEffect(effectDuration);
                    break;
            }
        }
    }
}