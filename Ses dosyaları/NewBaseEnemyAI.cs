using UnityEngine;
using System.Collections;

// Artık NavMeshAgent değil, Rigidbody2D zorunlu kılıyoruz.
[RequireComponent(typeof(Rigidbody2D))]
public abstract class NewBaseEnemyAI : MonoBehaviour, IDamageable
{
    [Header("Ölüm Efektleri (Parçalanma)")]
    public GameObject[] deathPieces; // Inspector'dan eklenecek parçalar havuzu
    public int minPiecesToDrop = 1;  // Minimum kaç parça düşsün?
    public int maxPiecesToDrop = 3;  // Maksimum kaç parça düşsün?
    public float scatterForce = 5f;  // Parçaların etrafa saçılma hızı


    [Header("Görsel Geri Bildirim (Hit Feedback)")]
    public Transform visualBody;
    public SpriteRenderer spriteRenderer;

    protected Color originalColor = Color.white;
    protected Vector3 originalLocalPos = Vector3.zero;
    protected Coroutine hitRoutine;

    [Header("Görsel Efektler (Partiküller)")]
    [SerializeField] protected GameObject damageEffectPrefab; // Hasar alınca 1 kez çıkacak efekt (Kan, kıvılcım vb.)
    [SerializeField] protected GameObject lowHealthEffectPrefab; // Can azalınca sürekli çıkacak efekt (Duman, ateş vb.)
    [SerializeField][Range(0f, 1f)] protected float lowHealthThreshold = 0.3f; // % kaç canın altına düşünce efekt başlasın? (0.3 = %30)

    protected GameObject activeLowHealthEffect; // Yaratılan duman efektini hafızada tutmak için
    [Header("Temel Veri")]
    // Data kısmını alt sınıfların erişebilmesi için protected yapıyoruz.
    [SerializeField] protected NewBaseEnemyData baseData;

    protected Transform player;
    protected Rigidbody2D rb;
    protected Animator anim;

    protected float currentHealth;
    protected AudioSource audioSource;
    // Temel Durumlar
    protected bool isDead = false;
    protected bool isFrozen = false;
    protected bool isChasing = false;
    protected bool isAttacking = false;
    [Header("UI Ayarları")]
    [SerializeField] protected GameObject damagePopupPrefab;
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // Ses kaynağını bul veya yoksa otomatik ekle
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (baseData != null)
        {
            currentHealth = baseData.maxHealth;
            if (baseData.startFrozen) isFrozen = true;
        }
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (visualBody == null && spriteRenderer != null) visualBody = spriteRenderer.transform;

        if (visualBody != null) originalLocalPos = visualBody.localPosition;
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }
    protected virtual void Update()
    {
        if (isDead || isFrozen || player == null) return;

        CheckPlayerDistance();

        // 🔴 DEĞİŞİKLİK: isAttacking kontrolünü kaldırdık! 
        // Artık kovalıyorsa her türlü HandleMovement çağrılacak. Durup durmamaya alt sınıf karar verecek.
        if (isChasing && baseData.canMove)
        {
            HandleMovement();
        }
    }
    protected virtual void CheckPlayerDistance()
    {
        if (baseData == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= baseData.detectionRadius)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
            rb.linearVelocity = Vector2.zero;
            UpdateAnimation(Vector2.zero);
        }

        if (isChasing && distance <= baseData.attackRange)
        {
            isAttacking = true;
            // 🔴 DEĞİŞİKLİK: Buradaki rb.velocity = Vector2.zero; ve UpdateAnimation kısımlarını sildik!
            HandleAttack();
        }
        else
        {
            isAttacking = false;
        }
    }

    // 🔴 EN ÖNEMLİ KISIM: Alt sınıflar bu fonksiyonu ezip kendi hareket tarzını yazacak!
    protected virtual void HandleMovement()
    {
        // Eğer oyuncu yoksa veya ölmüşse hata vermemesi için küçük bir güvenlik önlemi
        if (player == null) return;

        // Oyuncunun pozisyonuna doğru yönü hesapla
        Vector2 direction = (player.position - transform.position).normalized;

        // Rigidbody'nin hızını bu yöne doğru ayarla
        rb.linearVelocity = direction * baseData.moveSpeed;

        // Animasyonu güncelle
        UpdateAnimation(direction);
    }

    // 🔴 Alt sınıflar kendi saldırı mantığını buraya yazacak.
    protected virtual void HandleAttack()
    {
        // Temel sınıfta içi boş kalıyor.
    }

    // Ekstra Radar Collider yerine matematiksel mesafe ölçümü yapıyoruz (Performans dostu)

    protected virtual void UpdateAnimation(Vector2 direction)
    {
        if (anim == null) return;
        anim.SetFloat("moveX", direction.x);
        anim.SetFloat("moveY", direction.y);
    }


    public virtual void TakeDamage(float damage, float minDamage = 0f, float maxDamage = 0f)
    {
        if (isDead) return;
        currentHealth -= damage;

        // --- SES ÇALMA KISMI (Mevcut kodun) ---
        if (baseData.hurtSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.75f, 1.25f);
            audioSource.PlayOneShot(baseData.hurtSound, 0.8f);
        }

        // --- HASAR YAZISI ÇIKARMA KISMI (Mevcut kodun) ---
        if (damagePopupPrefab != null)
        {
            Vector3 popupPosition = transform.position + (Vector3.up * 1f);
            GameObject popupObj = Instantiate(damagePopupPrefab, popupPosition, Quaternion.identity);
            DamagePopup popupScript = popupObj.GetComponent<DamagePopup>();
            if (popupScript != null) popupScript.Setup(Mathf.RoundToInt(damage), minDamage, maxDamage);
        }

        // 🔴 YENİ: ANLIK HASAR EFEKTİ (Örn: Kan sıçraması)
        if (damageEffectPrefab != null)
        {
            // Efekti düşmanın pozisyonunda yaratıyoruz
            Instantiate(damageEffectPrefab, transform.position, Quaternion.identity);
        }

        // 🔴 YENİ: DÜŞÜK CAN EFEKTİ KONTROLÜ
        CheckLowHealthEffect();
        if (!isDead && visualBody != null && spriteRenderer != null)
        {
            if (hitRoutine != null) StopCoroutine(hitRoutine);
            hitRoutine = StartCoroutine(HitFeedbackRoutine());
        }
        if (currentHealth <= 0) Die();
    }
    // 🔴 YENİ: Düşmanların dışarıdan can almasını sağlayan fonksiyon
    public virtual void Heal(float amount)
    {
        // Ölü düşmanları diriltmiyoruz :)
        if (isDead) return;

        // Canı artır
        currentHealth += amount;

        // Maksimum can sınırını geçmesini engelle
        if (currentHealth > baseData.maxHealth)
        {
            currentHealth = baseData.maxHealth;
        }

        // 🔴 Heal Popup'ını çıkar
        // Not: Eğer pop-up prefabını base scriptinde 'damagePopupPrefab' gibi bir değişkenle tutuyorsan, adını ona göre ayarla.
        if (damagePopupPrefab != null)
        {
            // Popup'ı yarat
            GameObject popup = Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);

            // DamagePopup scriptine ulaş ve SetupHeal fonksiyonunu çalıştır
            DamagePopup popupScript = popup.GetComponent<DamagePopup>();
            if (popupScript != null)
            {
                popupScript.SetupHeal(Mathf.RoundToInt(amount));
            }
        }
    }
    protected virtual IEnumerator HitFeedbackRoutine()
    {
        // 1. Anında beyazlaş
        spriteRenderer.color = Color.white;

        // 2. Çok hızlı titreme döngüsü
        float elapsed = 0f;
        float duration = 0.15f;
        float shakeIntensity = 0.2f;

        while (elapsed < duration)
        {
            visualBody.localPosition = originalLocalPos + (Vector3)Random.insideUnitCircle * shakeIntensity;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3. Süre bitince her şeyi eski, normal haline döndür
        visualBody.localPosition = originalLocalPos;
        spriteRenderer.color = originalColor;
    }
    protected virtual void CheckLowHealthEffect()
    {
        if (lowHealthEffectPrefab == null || baseData == null) return;

        // Düşmanın can yüzdesini hesapla (Örn: 30 / 100 = 0.3)
        float healthPercentage = currentHealth / baseData.maxHealth;

        // Eğer can eşiğin altındaysa VE efekt henüz yaratılmamışsa
        if (healthPercentage <= lowHealthThreshold && activeLowHealthEffect == null)
        {
            // Efekti yarat ve düşmanın 'child'ı (çocuğu) yap ki düşman hareket ettikçe efekt de onunla sürüklensin
            activeLowHealthEffect = Instantiate(lowHealthEffectPrefab, transform.position, Quaternion.identity, transform);

            // İstersen efektin pozisyonunu düşmanın tam merkezine veya kafasına göre hafif yukarı kaydırabilirsin:
            activeLowHealthEffect.transform.localPosition = new Vector3(0, 0.5f, 0);
        }
    }
    public virtual void Die()
    {
        if (isDead) return; // İki kere ölmeyi engelle
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;

        // 🔴 YENİ: Düşük can efekti varsa, onu düşmandan ayır ve yavaşça yok et
        if (activeLowHealthEffect != null)
        {
            activeLowHealthEffect.transform.SetParent(null); // Düşmandan ayır (Düşman silinince silinmesin)
            ParticleSystem ps = activeLowHealthEffect.GetComponent<ParticleSystem>();
            if (ps != null) ps.Stop(); // Partikül üretimini durdur (mevcut dumanlar yavaşça dağılır)
            Destroy(activeLowHealthEffect, 2f); // 2 saniye sonra kalıntıları tamamen sil
        }

        // Ses ve Ganimet kısımları (Mevcut kodların)
        if (baseData.deathSound != null)
        {
            AudioManager.instance.PlaySFXAtPosition(baseData.deathSound, Camera.main.transform.position, 1f);
        }

        if (player != null)
        {
            StatController playerStats = player.GetComponent<StatController>();
            if (playerStats != null)
            {
                playerStats.AddExperience(baseData.expValue);
            }
        }
        DropLoot();
        ScatterDeathPieces();
        Destroy(gameObject, 0.1f);
    }

    // 🔴 GÜNCELLENEN: Loot Düşürme Fonksiyonu (Patlama Efektli)
    protected void DropLoot()
    {
        // Güvenlik kontrolü: Loot tablosu boşsa işlem yapma
        if (baseData == null || baseData.lootTable == null || baseData.lootTable.Length == 0) return;

        foreach (EnemyLoot loot in baseData.lootTable)
        {
            float roll = Random.Range(0f, 100f);

            // Şans tuttuysa ve prefab atanmışsa
            if (roll <= loot.dropChance && loot.itemPrefab != null)
            {
                // 1. Eşyayı yarat
                Vector3 dropPos = transform.position + (Vector3)Random.insideUnitCircle * 0.2f;
                GameObject droppedItem = Instantiate(loot.itemPrefab, dropPos, Quaternion.identity);

                // 2. Fiziksel Saçılma (Fırlama Efekti)
                Rigidbody2D rbItem = droppedItem.GetComponent<Rigidbody2D>();
                if (rbItem != null)
                {
                    Vector2 randomDirection = Random.insideUnitCircle.normalized;
                    float burstSpeed = Random.Range(6f, 10f); // Fırlama hızı
                    rbItem.linearVelocity = randomDirection * burstSpeed;

                   
                }

                // 3. Glitch Efektini Başlat (Eğer prefabda script takılıysa otomatik çalışır)
                // Eğer scripti otomatik başlatmak istemiyorsan buraya: 
                // droppedItem.GetComponent<LootGlitch>()?.StartGlitch(); gibi bir satır eklenebilir.
            }
        }
    }
    protected void ScatterDeathPieces()
    {
        // Eğer liste boşsa hiç yorulma, geri dön
        if (deathPieces == null || deathPieces.Length == 0) return;

        // 1 ile 3 arasında (max değere 1 ekliyoruz çünkü Random.Range int'lerde son sayıyı dahil etmez) rastgele bir miktar belirle
        int dropCount = Random.Range(minPiecesToDrop, maxPiecesToDrop + 1);

        for (int i = 0; i < dropCount; i++)
        {
            // Listeden tamamen RASTGELE bir parça seç
            GameObject randomPiecePrefab = deathPieces[Random.Range(0, deathPieces.Length)];

            if (randomPiecePrefab != null)
            {
                // Seçilen parçayı yarat
                GameObject spawnedPiece = Instantiate(randomPiecePrefab, transform.position, Quaternion.identity);

                // Parçada Rigidbody2D varsa, onu rastgele bir yöne fırlat ve döndür
                Rigidbody2D rb = spawnedPiece.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 randomDir = Random.insideUnitCircle.normalized; // Rastgele bir yön (360 derece)
                    rb.linearVelocity = randomDir * scatterForce;
                    rb.angularVelocity = Random.Range(-300f, 300f); // Havada kendi etrafında fırıl fırıl dönsün
                }
            }
        }
    }
}