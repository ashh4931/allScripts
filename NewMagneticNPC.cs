using System.Collections;
using UnityEngine;

public class NewMagneticNPC : NewBaseEnemyAI
{
    [Header("Manyetik Parçalar")]
    public Transform headVisual;
    public Transform capeVisual;
    public Transform northHand;
    public Transform southHand;
    public Transform firePoint;

    [Header("Görsel İnce Ayarlar")]
    public float handsYOffset = -0.8f; // Ellerin kafaya göre ne kadar aşağıda duracağı

    private NewMagneticEnemyData magData;
    private bool isUsingAbility = false;
    private float abilityTimer = 0f;

    // Animasyon matematiği
    private float hoverTime;
    private float orbitAngle;
    private bool northHandOrbiting = true;
    private bool southHandOrbiting = true;
    private float stateTimer = 0f;
    private bool isSpinning = false;
    private float spinLerp = 0f;

    protected override void Start()
    {
        base.Start();
        magData = baseData as NewMagneticEnemyData;
    }

    protected override void Update()
    {
        base.Update();
        AnimateBodyParts();

        if (isDead || isFrozen || player == null) return;

        if (!isUsingAbility)
        {
            abilityTimer -= Time.deltaTime;
        }
    }

    // 🔴 DİBİNE GİRİNCE KAÇMA EKLENDİ (KITING)
    // 🔴 HAREKET VE KITING GÜNCELLENDİ (Artık durmuyor, etrafında süzülüyor)
 protected override void HandleMovement()
{
    if (isUsingAbility) return;

    float distanceToPlayer = Vector2.Distance(transform.position, player.position);
    Vector2 direction = (player.position - transform.position).normalized;

    if (distanceToPlayer > magData.magneticRadius)
    {
        // 🔴 DEĞİŞTİ: magData.moveSpeed -> currentMoveSpeed
        rb.linearVelocity = direction * currentMoveSpeed;
    }
    else if (distanceToPlayer < magData.retreatDistance)
    {
        // 🔴 DEĞİŞTİ: 0.8 katı hızı currentMoveSpeed üzerinden hesapla
        rb.linearVelocity = -direction * currentMoveSpeed * 0.8f;
    }
    else
    {
        Vector2 orbitDirection = new Vector2(-direction.y, direction.x);
        // 🔴 DEĞİŞTİ: Orbit hızı da yavaşlamalı
        rb.linearVelocity = orbitDirection * (currentMoveSpeed * 0.5f);
    }
}

    protected override void HandleAttack()
    {
        if (isUsingAbility || abilityTimer > 0f) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= magData.magneticRadius)
        {
            // 3 YETENEK ARASINDA RASTGELE SEÇİM YAPAR (%33 İhtimalle)
            float rand = Random.value;
            if (rand < 0.33f)
                StartCoroutine(RocketPunchRoutine());
            else if (rand < 0.66f)
                StartCoroutine(BlackHoleBulletHellRoutine());
            else
                StartCoroutine(MagneticClapRoutine()); // YENİ YETENEK!
        }
    }

    // --- 1. YETENEK: ROKET YUMRUK (ZORLAŞTIRILDI) ---
    private IEnumerator RocketPunchRoutine()
    {
        isUsingAbility = true;
        rb.linearVelocity = Vector2.zero;
        northHandOrbiting = false;

        // 🔴 SES BURAYA GELECEK: Şarjlanma başlarken!
        if (magData.rocketPunchSound != null && audioSource != null)
            audioSource.PlayOneShot(magData.rocketPunchSound);

        Vector3 startPos = northHand.position;
        northHand.localScale = Vector3.one * 1.8f;
        // ... gerisi aynı ...
        float chargeTime = 0.25f;
        while (chargeTime > 0)
        {
            northHand.position = startPos + (Vector3)Random.insideUnitCircle * 0.15f;
            chargeTime -= Time.deltaTime;
            yield return null;
        }

        // Oyuncunun o anki yerini al ve FÜZE GİBİ UÇ (Hız 25'ten 45'e çıktı!)
        Vector3 targetPos = player.position;
        float dashSpeed = 45f;
        while (Vector3.Distance(northHand.position, targetPos) > 0.5f)
        {
            northHand.position = Vector3.MoveTowards(northHand.position, targetPos, dashSpeed * Time.deltaTime);

            // Çarpışma alanını genişlettik (Hitbox 2f'den 2.5f'e çıktı)
            if (player != null && Vector2.Distance(northHand.position, player.position) <= 2.5f)
            {
                Vector2 pushDir = (player.position - northHand.position).normalized;
                player.GetComponent<PlayerController>()?.ApplyKnockback(pushDir * magData.pushForce);
                break; // Vurursa uçmayı kes
            }
            yield return null;
        }

        // Geri Dönüş
        northHand.localScale = Vector3.one;
        while (Vector3.Distance(northHand.localPosition, GetOrbitPosition(orbitAngle)) > 0.1f)
        {
            northHand.localPosition = Vector3.Lerp(northHand.localPosition, GetOrbitPosition(orbitAngle), Time.deltaTime * 12f);
            yield return null;
        }

        northHandOrbiting = true;
        abilityTimer = magData.abilityCooldown;
        isUsingAbility = false;
    }

    // --- 2. YETENEK: KARA DELİK + MERMİ (SIKLAŞTIRILDI) ---

    private IEnumerator BlackHoleBulletHellRoutine()
    {
        isUsingAbility = true;
        rb.linearVelocity = Vector2.zero;
        southHandOrbiting = false;

        // 1. ADIM: Kara delik oluşum sesi (Kısa bir 'whoosh' veya 'charge' sesi)
        if (magData.blackHoleSound != null && audioSource != null)
            audioSource.PlayOneShot(magData.blackHoleSound);

        // 2. ADIM: Ellerin yukarı kalkma animasyonu
        Vector3 blackHolePos = new Vector3(0, 2f, 0);
        while (Vector3.Distance(southHand.localPosition, blackHolePos) > 0.1f)
        {
            southHand.localPosition = Vector3.Lerp(southHand.localPosition, blackHolePos, Time.deltaTime * 5f);
            southHand.localScale = Vector3.Lerp(southHand.localScale, Vector3.one * 2.5f, Time.deltaTime * 5f);
            southHand.Rotate(0, 0, 500f * Time.deltaTime);
            yield return null;
        }

        // 3. ADIM: TOPLU ATEŞ SESİNİ BAŞLAT
        // Burada PlayOneShot yerine Play() kullanıyoruz çünkü yetenek bitince durdurabilmeliyiz.
        if (magData.shootSound != null && audioSource != null)
        {
            audioSource.clip = magData.shootSound;
            audioSource.loop = true; // Hazırladığın ses döngüye uygunsa true yap
            audioSource.Play();
        }

        float duration = magData.abilityDuration;
        float fireTimer = 0f;
        float bulletAngle = 0f;

        // 4. ADIM: MERMİ YAĞMURU DÖNGÜSÜ
        while (duration > 0)
        {
            // Oyuncuyu merkeze çekme mantığı
            if (player != null && Vector2.Distance(transform.position, player.position) <= magData.magneticRadius)
            {
                Vector2 pullDir = (transform.position - player.position).normalized;
                player.GetComponent<PlayerController>()?.ApplyKnockback(pullDir * magData.pullForce * Time.deltaTime * 60f);
            }

            southHand.Rotate(0, 0, 800f * Time.deltaTime);

            // Mermi yaratma (Burada artık ses çalmıyoruz!)
            if (magData.bulletPrefab != null && firePoint != null)
            {
                fireTimer -= Time.deltaTime;
                if (fireTimer <= 0f)
                {
                    Quaternion rotation = Quaternion.Euler(0, 0, bulletAngle);
                    Instantiate(magData.bulletPrefab, firePoint.position, rotation);
                    bulletAngle += 25f;
                    fireTimer = magData.bulletSpawnRate;
                }
            }

            duration -= Time.deltaTime;
            yield return null;
        }

        // 5. ADIM: SESİ DURDUR VE TEMİZLİK
        if (audioSource != null && audioSource.clip == magData.shootSound)
        {
            audioSource.Stop(); // Yetenek bitti, gürültüyü kes.
        }

        southHand.localScale = Vector3.one;
        southHand.localRotation = Quaternion.identity;
        southHandOrbiting = true;
        abilityTimer = magData.abilityCooldown;
        isUsingAbility = false;
    } // --- 3. YETENEK (YENİ!): MANYETİK ALKIŞ (DUAL CLAP) ---
    private IEnumerator MagneticClapRoutine()
    {
        isUsingAbility = true;
        rb.linearVelocity = Vector2.zero;
        northHandOrbiting = false;
        southHandOrbiting = false;

        // 1. Eller oyuncunun iki yanına açılır (Kaçması zor!)
        Vector3 playerPos = player.position;
        Vector3 leftTarget = playerPos + Vector3.left * 4f;
        Vector3 rightTarget = playerPos + Vector3.right * 4f;

        northHand.localScale = Vector3.one * 1.5f;
        southHand.localScale = Vector3.one * 1.5f;

        float prepTimer = 0.4f; // Yanlara gitme süresi
        while (prepTimer > 0)
        {
            // Oyuncu hareket ederse eller de oyuncuyu hizalamaya devam eder (Zorluk burada!)
            leftTarget = player.position + Vector3.left * 4f;
            rightTarget = player.position + Vector3.right * 4f;

            northHand.position = Vector3.Lerp(northHand.position, leftTarget, Time.deltaTime * 10f);
            southHand.position = Vector3.Lerp(southHand.position, rightTarget, Time.deltaTime * 10f);

            prepTimer -= Time.deltaTime;
            yield return null;
        }
        if (magData.clapSound != null && audioSource != null)
            audioSource.PlayOneShot(magData.clapSound);
        // 2. ŞİDDETLE ÇARPIŞMA (ALKIŞ)
        float dashSpeed = 50f;
        Vector3 clapCenter = player.position; // Oyuncunun o anki konumu
        while (Vector3.Distance(northHand.position, clapCenter) > 0.5f)
        {
            northHand.position = Vector3.MoveTowards(northHand.position, clapCenter, dashSpeed * Time.deltaTime);
            southHand.position = Vector3.MoveTowards(southHand.position, clapCenter, dashSpeed * Time.deltaTime);
            yield return null;
        }
        StatController playerStats = player.GetComponent<StatController>();
        // 3. Hasar ve İtme Kontrolü (Tam ortada yakalanırsa devasa hasar)
        if (player != null && Vector2.Distance(northHand.position, player.position) <= 3f)
        {
            playerStats.TakeDamage(magData.baseDamage * 2f);
            Vector2 pushDir = (player.position - transform.position).normalized;
            player.GetComponent<PlayerController>()?.ApplyKnockback(pushDir * magData.pushForce * 1.5f); // %50 daha fazla itme!
        }

        // Kamera sarsıntısı efekti için harika bir yer.

        // 4. Geri Dönüş
        northHand.localScale = Vector3.one;
        southHand.localScale = Vector3.one;
        float returnTimer = 0.5f;
        while (returnTimer > 0)
        {
            northHand.localPosition = Vector3.Lerp(northHand.localPosition, GetOrbitPosition(orbitAngle), Time.deltaTime * 10f);
            southHand.localPosition = Vector3.Lerp(southHand.localPosition, GetOrbitPosition(orbitAngle + Mathf.PI), Time.deltaTime * 10f);
            returnTimer -= Time.deltaTime;
            yield return null;
        }

        northHandOrbiting = true;
        southHandOrbiting = true;
        abilityTimer = magData.abilityCooldown;
        isUsingAbility = false;
    }

    // --- ANİMASYONLAR VE HİZALAMA ---
    private void AnimateBodyParts()
    {
        hoverTime += Time.deltaTime * 3f;

        if (capeVisual != null)
        {
            float primaryWave = Mathf.Sin(hoverTime * 1.5f) * 8f;
            float secondaryWave = Mathf.Cos(hoverTime * 4.5f) * 3f;
            capeVisual.localRotation = Quaternion.Euler(0, 0, primaryWave + secondaryWave);
            float windSwelling = 1f + Mathf.Sin(hoverTime * 2.5f) * 0.05f;
            capeVisual.localScale = new Vector3(windSwelling, 1f + (1f - windSwelling) * 0.5f, 1f);
        }

        if (headVisual != null)
            headVisual.localPosition = new Vector3(0, Mathf.Sin(hoverTime) * 0.1f, 0);

        if (!isUsingAbility)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                isSpinning = !isSpinning;
                stateTimer = isSpinning ? 2f : 3f; // 2 sn dön, 3 sn dur
            }

            spinLerp = Mathf.MoveTowards(spinLerp, isSpinning ? 1f : 0f, Time.deltaTime * 2f);
            orbitAngle += Time.deltaTime * magData.orbitSpeed * spinLerp;
        }

        // ELLERİN AŞAĞIYA HİZALANMASI (handsYOffset EKLENDİ)
        if (northHand != null && northHandOrbiting)
        {
            Vector3 idlePos = new Vector3(-magData.orbitRadius, Mathf.Sin(hoverTime * 2f) * 0.2f + handsYOffset, 0);
            Vector3 orbitPos = GetOrbitPosition(orbitAngle);
            northHand.localPosition = Vector3.Lerp(idlePos, orbitPos, spinLerp);
        }

        if (southHand != null && southHandOrbiting)
        {
            Vector3 idlePos = new Vector3(magData.orbitRadius, Mathf.Cos(hoverTime * 2f) * 0.2f + handsYOffset, 0);
            Vector3 orbitPos = GetOrbitPosition(orbitAngle + Mathf.PI);
            southHand.localPosition = Vector3.Lerp(idlePos, orbitPos, spinLerp);
        }
    }

    private Vector3 GetOrbitPosition(float angle)
    {
        float x = Mathf.Cos(angle) * magData.orbitRadius;
        float y = Mathf.Sin(angle) * magData.orbitRadius;
        // Dönüş rotasına da Y offsetini ekliyoruz ki kalkan kafanın etrafında değil, gövdenin etrafında dönsün
        return new Vector3(x, y + handsYOffset, 0);
    }
}