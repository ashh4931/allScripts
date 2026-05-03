using UnityEngine;
using System.Collections;

public class SplitterEnemyAI : NewBaseEnemyAI
{
    [Header("Görsel Referanslar (Splitter Özel)")]
    // Not: visualBody ve spriteRenderer Base class'tan geliyor.

    [Header("Isırma (Chomp) Animasyonu")]
    public Transform jawPivot;
    public SpriteRenderer upperJaw;
    public SpriteRenderer lowerJaw;
    public float jawOpenOffset = 0.5f;
    public float chompDuration = 0.15f;

    // 🔴 YENİ: DÖNME AYARLARI
    [Header("Dönme Ayarları")]
    [Tooltip("Düşmanın dönüş hızı (Yüksek sayı = daha hızlı döner)")]
    public float rotationSpeed = 10f;

    private SplitterEnemyData splitterData;
    private Coroutine chompRoutine;
    private float nextAttackTime;

    protected override void Start()
    {
        base.Start();

        splitterData = baseData as SplitterEnemyData;

        // Oyun başlarken çeneleri gizle
        if (upperJaw != null) upperJaw.enabled = false;
        if (lowerJaw != null) lowerJaw.enabled = false;
    }

    // 🔴 YENİ: Update fonksiyonunu ezip (override), dönme mantığını ekliyoruz
    protected override void Update()
    {
        // Base class'taki hareket ve hedef bulma mantığını bozmamak için önce onu çalıştır
        base.Update();

        // Eğer düşman ölüyse, donmuşsa veya oyuncu yoksa dönmeye çalışma
        if (isDead || isFrozen || player == null) return;

        // 🔴 OYUNCUYA DÖNME MANTIĞI (Prosedürel / Lerp ile yumuşak)
        // 1. Oyuncuya doğru olan yön vektörünü hesapla
        Vector2 direction = player.transform.position - transform.position;

        // Vektörün açısını hesapla (Radyandan Dereceye çevir)
        // Not: Genelde 2D sprite'ların ön yüzü 'Sağ' (Right) taraftır. 
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 2. Mevcut açıdan hedef açıya doğru yumuşak bir geçiş (Lerp) hesapla
        // Bu, düşmanın anında değil, yağ gibi süzülerek dönmesini sağlar.
        float smoothedAngle = Mathf.LerpAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime);

        // 3. Hesaplanan yeni açıyı düşmanın Z eksenine uygula
        transform.rotation = Quaternion.Euler(0, 0, smoothedAngle);
    }
    protected override void HandleMovement()
    {
        if (player == null || isDead) return;

        // 1. ÖNGÖRÜLÜ TAKİP (Predictive Interception)
        // Oyuncunun 0.4 saniye sonraki konumunu hedefle (Yolunu kesmek için)
        Vector2 targetPos = GetPredictedPlayerPosition(0.4f);

        // 2. KATMANLI KUŞATMA (Layered Surrounding)
        // Hepsi oyuncunun tam göbeğine gitmesin. 
        // personalSpaceOffset değerini yön vektörüne küçük bir sapma olarak ekliyoruz.
        Vector3 surroundOffset = new Vector3(personalSpaceOffset, -personalSpaceOffset, 0);
        targetPos += (Vector2)surroundOffset;

        Vector2 direction = (targetPos - (Vector2)transform.position).normalized;

        // 3. HAREKET UYGULAMA
        // Eğer oyuncuya çok yakınsa (ısırıyorsa) biraz yavaşla, uzaktaysa tam hız git
        float distance = Vector2.Distance(transform.position, player.position);

        // Eğer 'ApplyTimeSlow' çalıştıysa, 'currentMoveSpeed' zaten küçülmüş olacaktır.
        float finalSpeed = (distance < splitterData.attackRange) ? currentMoveSpeed * 0.5f : currentMoveSpeed;

        rb.linearVelocity = direction * finalSpeed;
        // Animasyon ve Görsel Dönüş (Mevcut rotasyon kodunla uyumlu)
        UpdateAnimation(direction);
    }
    // --- SALDIRI VE ÇENE ANİMASYONU (Eski kodun aynısı, eksiksiz) ---
    protected override void HandleAttack()
    {
        if (Time.time >= nextAttackTime && player != null && !isDead)
        {
            IDamageable playerDamageable = player.GetComponent<IDamageable>();

            if (playerDamageable != null)
            {
                playerDamageable.TakeDamage(splitterData.meleeDamage);

                if (splitterData.meleeSound != null && audioSource != null)
                {
                    audioSource.pitch = Random.Range(0.9f, 1.1f);
                    audioSource.PlayOneShot(splitterData.meleeSound);
                }

                if (jawPivot != null && upperJaw != null && lowerJaw != null)
                {
                    if (chompRoutine != null) StopCoroutine(chompRoutine);
                    chompRoutine = StartCoroutine(ChompAnimationRoutine());
                }

                nextAttackTime = Time.time + splitterData.attackCooldown;
            }
        }
    }

    private IEnumerator ChompAnimationRoutine()
    {
        // Not: Ana obje (transform) zaten oyuncuya döndüğü için,
        // JawPivot'un açısını 0 yaparak ana objeyle aynı yöne bakmasını sağlıyoruz.
        jawPivot.localRotation = Quaternion.identity;

        upperJaw.enabled = true;
        lowerJaw.enabled = true;

        Vector3 upperOpenPos = new Vector3(0, jawOpenOffset, 0);
        Vector3 lowerOpenPos = new Vector3(0, -jawOpenOffset, 0);

        upperJaw.transform.localPosition = upperOpenPos;
        lowerJaw.transform.localPosition = lowerOpenPos;

        float elapsed = 0f;
        while (elapsed < chompDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / chompDuration;

            upperJaw.transform.localPosition = Vector3.Lerp(upperOpenPos, Vector3.zero, percent);
            lowerJaw.transform.localPosition = Vector3.Lerp(lowerOpenPos, Vector3.zero, percent);

            yield return null;
        }

        upperJaw.transform.localPosition = Vector3.zero;
        lowerJaw.transform.localPosition = Vector3.zero;
        yield return new WaitForSeconds(0.05f);

        upperJaw.enabled = false;
        lowerJaw.enabled = false;
    }

    // --- ÖLÜM, KÜÇÜLME VE BÖLÜNME (Eski kodun aynısı, eksiksiz) ---
    public override void Die()
    {
        if (isDead) return;
        isDead = true;

        if (chompRoutine != null) StopCoroutine(chompRoutine);

        if (upperJaw != null) upperJaw.enabled = false;
        if (lowerJaw != null) lowerJaw.enabled = false;

        rb.linearVelocity = Vector2.zero;
        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;

        if (baseData != null && baseData.deathSound != null)
        {
            AudioSource.PlayClipAtPoint(baseData.deathSound, Camera.main.transform.position, 1f);
        }
        DropLoot();
        ScatterDeathPieces();

        StartCoroutine(ShrinkAndSplitRoutine());
    }

    private IEnumerator ShrinkAndSplitRoutine()
    {
        float elapsed = 0f;
        float duration = 0.2f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.zero;

        if (splitterData != null && splitterData.nextStagePrefab != null)
        {
            for (int i = 0; i < splitterData.spawnCount; i++)
            {
                // 1. Önce fırlayacağı rastgele yönü belirliyoruz
                Vector2 randomDir = Random.insideUnitCircle.normalized;

                // 2. Doğma mesafesi (Bu değeri kafana göre 0.5f, 1f gibi ayarlayabilirsin)
                float spawnOffset = 3f;

                // 3. Merkezin tam üstünde değil, o yöne doğru biraz kaymış bir pozisyon hesaplıyoruz
                Vector3 spawnPos = transform.position + (Vector3)(randomDir * spawnOffset);

                // 4. Çocuğu yeni hesapladığımız mesafeli noktada yaratıyoruz
                GameObject child = Instantiate(splitterData.nextStagePrefab, spawnPos, Quaternion.identity);

                // 5. Yine o yöne doğru fırlatıyoruz
                Rigidbody2D childRb = child.GetComponent<Rigidbody2D>();
                if (childRb != null)
                {
                    childRb.linearVelocity = randomDir * 6f;
                }

            }
        }

        Destroy(gameObject);
    }


    // SplitterEnemyAI.cs içine eklenecek kısım

    // --- 🔴 ZAMAN YIRTIĞI TAM UYUM (OVERRIDE) ---
    private float originalRotationSpeed;
    private float originalChompDuration;

// SplitterEnemyAI.cs içindeki metodları bu şekilde güncelle:

public override void ApplyTimeSlow(float slowPercentage)
{
    // 🔴 KRİTİK: Eğer zaten yavaşlatıldıysak, alt kısımdaki kodları çalıştırma!
    if (isTimeSlowed || isDead) return;

    // Temel sınıftaki hız yavaşlatmasını çalıştır
    base.ApplyTimeSlow(slowPercentage);

    // Splitter'a özel değerleri yedekle ve yavaşlat
    originalRotationSpeed = rotationSpeed;
    rotationSpeed *= (1f - slowPercentage);

    originalChompDuration = chompDuration;
    chompDuration /= (1f - slowPercentage);
}

public override void RemoveTimeSlow()
{
    // 🔴 KRİTİK: Eğer yavaşlatılmamışsak, geri döndürme işlemi yapma!
    if (!isTimeSlowed || isDead) return;

    // Temel sınıftaki değerleri geri yükle
    base.RemoveTimeSlow();

    // Özel değerleri eski haline getir
    rotationSpeed = originalRotationSpeed;
    chompDuration = originalChompDuration;
}

  
}