using UnityEngine;
using System.Collections;

public class NewRocketEnemyAI : NewBaseEnemyAI
{
    private EnemyTracker enemyTracker; // Sahnede kalan düşmanları saymak için
    [Header("Görsel Referanslar (Roketçi)")]
    public Transform headGroup;      // Kafa ve kafa iticisini içeren obje
    public Transform batteryGroup;   // Batarya ve batarya iticisini içeren obje
    public GameObject batteryThrusterVFX; // Saplanınca kapanacak efekt
    public Transform[] firePoints;   // Roketlerin ateşleneceği noktalar
    public GameObject fireSmokeVFX;  // Atış sırasındaki duman efekti

    private NewRocketEnemyData rocketData;
    private bool isArtilleryMode = false; // Yere saplı ve ateş ediyor mu?
    private float hoverTimer = 0f;
    private float cooldownTimer = 0f;

    private Vector3 originalHeadPos;
    private Vector3 originalBatteryPos;
    protected override void Start()
    {
        base.Start();
        rocketData = baseData as NewRocketEnemyData;

        if (headGroup != null) originalHeadPos = headGroup.localPosition;
        if (batteryGroup != null) originalBatteryPos = batteryGroup.localPosition;

        // YENİ EKLENEN SATIR: EnemyTracker'ı sahnede bul
        enemyTracker = FindObjectOfType<EnemyTracker>();
    }

    protected override void Update()
    {
        base.Update();
        if (isDead || isFrozen || player == null) return;

        HoverAnimation();

        if (!isArtilleryMode)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    // Uçma (Hover) Animasyonu
    private void HoverAnimation()
    {
        hoverTimer += Time.deltaTime * 2f;
        float hoverOffset = Mathf.Sin(hoverTimer) * 0.15f;

        // Kafa her zaman uçmaya devam eder
        if (headGroup != null)
        {
            headGroup.localPosition = originalHeadPos + new Vector3(0, hoverOffset, 0);
        }

        // Batarya sadece saldırıda değilken (havadayken) uçar
        if (!isArtilleryMode && batteryGroup != null)
        {
            // Batarya kafadan biraz daha farklı bir dalgayla uçsun
            float batteryHoverOffset = Mathf.Sin(hoverTimer * 1.2f) * 0.1f;
            batteryGroup.localPosition = originalBatteryPos + new Vector3(0, batteryHoverOffset, 0);
        }
    }

    // Hareket Kontrolü (Topçu mantığı: Çok yaklaşılırsa geri çekil, menzilde dur)
    protected override void HandleMovement()
    {
        if (isArtilleryMode || rocketData == null)
        {
            rb.linearVelocity = Vector2.zero; // Saplanmışken kıpırdama
            return;
        }

        Vector2 direction = (player.position - transform.position).normalized;

        // --- YENİ EKLENEN MANTIK ---
        // Sahnede 5'ten az düşman kaldıysa, taktiği boşverip oyuncunun üzerine git
        if (enemyTracker != null && enemyTracker.activeEnemies.Count < 5)
        {
            rb.linearVelocity = direction * currentMoveSpeed;
            return; // Aşağıdaki mesafe kontrollerini atla
        }
        // ---------------------------

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > rocketData.artilleryRange)
        {
            // Menzile gir
            rb.linearVelocity = direction * currentMoveSpeed;
        }
        else if (distanceToPlayer < rocketData.retreatDistance)
        {
            // Fazla yaklaştı, geri kaç
            rb.linearVelocity = -direction * currentMoveSpeed;
        }
        else
        {
            // Tam atış mesafesi, dur ve hafif kay
            rb.linearVelocity = Vector2.zero;
        }
    }

    protected override void HandleAttack()
    {
        if (isArtilleryMode || cooldownTimer > 0f) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Saldırı menzilindeyse topçu moduna geç
        if (distanceToPlayer <= rocketData.artilleryRange)
        {
            StartCoroutine(ArtilleryAttackRoutine());
        }
    }

    private IEnumerator ArtilleryAttackRoutine()
    {
        isArtilleryMode = true;
        rb.linearVelocity = Vector2.zero;

        // 1. BATARYA YERE SAPLANIR
        if (batteryThrusterVFX != null) batteryThrusterVFX.SetActive(false); // İtici kapanır

        Vector3 targetSlamPos = originalBatteryPos + new Vector3(0, -0.6f, 0); // Yere oturt
        float dropSpeed = 10f;
        while (Vector3.Distance(batteryGroup.localPosition, targetSlamPos) > 0.05f)
        {
            batteryGroup.localPosition = Vector3.Lerp(batteryGroup.localPosition, targetSlamPos, Time.deltaTime * dropSpeed);
            yield return null;
        }
        batteryGroup.localPosition = targetSlamPos;

        // Hafif bir bekleme (Oyuncu saldırı geldiğini anlasın)
        yield return new WaitForSeconds(0.5f);

        // 2. PATERN SEÇİMİ (%50 Çember, %50 Çizgi)
        bool useLinePattern = Random.value > 0.5f;

        // Oyuncunun gidiş yönünü al (Eğer hareketsizse, karakterden oyuncuya doğru bir yön al)
        Vector2 playerVelocity = playerRb != null ? playerRb.linearVelocity : Vector2.zero;
        Vector2 attackDirection = playerVelocity.magnitude > 0.5f ? playerVelocity.normalized : ((Vector2)(player.position - transform.position)).normalized;
        Vector2 baseTargetCenter = (Vector2)player.position + (playerVelocity * rocketData.predictiveLeadTime);

        // 3. ROKETLERİ ATEŞLE
        int currentFirePoint = 0;
        for (int i = 0; i < rocketData.rocketsPerVolley; i++)
        {
            // Atış Noktasını Seç
            Transform fp = firePoints.Length > 0 ? firePoints[currentFirePoint % firePoints.Length] : transform;
            currentFirePoint++;

            // Yukarı Çıkan Roket Görselini Yarat
            if (rocketData.upwardRocketPrefab != null)
            {
                Instantiate(rocketData.upwardRocketPrefab, fp.position, Quaternion.identity);
            }

            // Geri Tepme (Recoil) ve Ses
            StartCoroutine(BatteryRecoilAnim());
            if (rocketData.shootSound != null) audioSource.PlayOneShot(rocketData.shootSound);
            if (fireSmokeVFX != null) Instantiate(fireSmokeVFX, fp.position, Quaternion.identity);

            // Hedef Noktasını Hesapla
            Vector2 strikePosition = Vector2.zero;
            if (useLinePattern)
            {
                // Çizgi Formasyonu (Yönde belli aralıklarla sırala)
                // Oyuncunun tahmini pozisyonundan başlayarak arkasına/önüne doğru sıralar
                strikePosition = baseTargetCenter + (attackDirection * (i * rocketData.lineSpacing));
            }
            else
            {
                // Çember Formasyonu (Merkez etrafında rastgele)
                strikePosition = baseTargetCenter + Random.insideUnitCircle * rocketData.circleScatterRadius;
            }

            // Hedef Yöneticisini Doğur
            if (rocketData.strikeManagerPrefab != null)
            {
                GameObject strike = Instantiate(rocketData.strikeManagerPrefab, strikePosition, Quaternion.identity);
                strike.GetComponent<ArtilleryStrike>()?.SetupStrike(rocketData, useLinePattern, attackDirection);
            }

            // Diğer rokete geçmeden önce bekle
            yield return new WaitForSeconds(rocketData.timeBetweenShots);
        }

        // 4. TOPARLANMA
        yield return new WaitForSeconds(0.5f);

        // Batarya Havaya Kalkar
        if (batteryThrusterVFX != null) batteryThrusterVFX.SetActive(true);
        while (Vector3.Distance(batteryGroup.localPosition, originalBatteryPos) > 0.05f)
        {
            batteryGroup.localPosition = Vector3.Lerp(batteryGroup.localPosition, originalBatteryPos, Time.deltaTime * 5f);
            yield return null;
        }
        batteryGroup.localPosition = originalBatteryPos;

        cooldownTimer = rocketData.attackCooldown;
        isArtilleryMode = false;
    }

    // Ateşleme Anında Bataryayı Y ekseninde aşağı bastırıp (geri tepme) eski konumuna yumuşakça alma
    private IEnumerator BatteryRecoilAnim()
    {
        if (batteryGroup == null) yield break;

        Vector3 startP = batteryGroup.localPosition;
        Vector3 recoilP = startP + new Vector3(0, -0.3f, 0); // 0.3 birim aşağı bas

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 15f; // Hızlıca in
            batteryGroup.localPosition = Vector3.Lerp(startP, recoilP, t);
            yield return null;
        }
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f; // Yavaşça geri kalk
            batteryGroup.localPosition = Vector3.Lerp(recoilP, startP, t);
            yield return null;
        }
    }
}