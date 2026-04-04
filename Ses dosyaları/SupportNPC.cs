using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SupportNPC : NewBaseEnemyAI
{
    [Header("Görsel Parçalar (5 Parça)")]
    // Not: visualsParent objesi artık sadece hiyerarşi düzeni için, kodu onu hareket ettirmeyecek.
    public Transform visualsParent;
    public Transform headVisual;
    public Transform leftArmVisual;
    public Transform rightArmVisual;
    public Transform lowerModuleVisual;
    public Transform shadowSprite;

    [Header("Canlı Animasyon (Breathing) Ayarları")]
    public float breatheSpeed = 3f;       // 🔴 DAHA HAREKETLİ: Hız artırıldı
    public float breatheAmount = 0.15f;   // 🔴 DAHA HAREKETLİ: Genlik artırıldı

    [Header("Heal Efektleri ve Yükselme")]
    public GameObject healEffectPrefab;
    public AudioClip healSound;
    public float healParcaCekmeOffset = 0.3f; // Merkeze kapanma şiddeti
    public float healYukselmeMikatar = 1.0f;  // 🔴 %100 ARTIRILDI: Sadece üst gövde bu kadar yükselecek
    public float animSpeed = 8f;             // Animasyonların süzülme (Lerp) hızı

    // Hafızaya alınacak orijinal yerleşimler
    private Vector3 headHomePos;
    private Vector3 leftArmHomePos;
    private Vector3 rightArmHomePos;
    private Vector3 lowerModuleHomePos;

    private SupportEnemyData supportData;
    private float nextHealTime;
    private float breathingTimer;

    // 🔴 KUSURSUZ ANİMASYON İÇİN HEDEF VE MEVCUT DEĞERLER
    private float targetBreatheMultiplier = 1f;

    private float targetHealPull = 0f;
    private float currentHealPull = 0f;

    private float targetRise = 0f;
    private float currentRise = 0f;

    protected override void Start()
    {
        base.Start();

        supportData = baseData as SupportEnemyData;

        if (supportData == null)
            Debug.LogError(gameObject.name + ": Lütfen Inspector'dan bir SupportEnemyData atayın!");

        // Prefab'daki konumları hafızaya al
        if (headVisual != null) headHomePos = headVisual.localPosition;
        if (leftArmVisual != null) leftArmHomePos = leftArmVisual.localPosition;
        if (rightArmVisual != null) rightArmHomePos = rightArmVisual.localPosition;
        if (lowerModuleVisual != null) lowerModuleHomePos = lowerModuleVisual.localPosition;
    }

    protected override void Update()
    {
        base.Update();
        if (isDead || isFrozen || player == null || supportData == null) return;

        // Bütün animasyon hesaplamaları ve Lerp geçişleri sadece burada yapılıyor
        HandleAliveAndRiseAnimations();

        if (Time.time >= nextHealTime)
        {
            StartCoroutine(PerformHealSequence());
        }
    }

    protected override void HandleMovement()
    {
        if (isDead || isFrozen || player == null || supportData == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= supportData.fleeRange)
        {
            Vector2 fleeDirection = (transform.position - player.transform.position).normalized;
            rb.linearVelocity = fleeDirection * supportData.moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, Time.deltaTime * 5f);
        }
    }

    private void HandleAliveAndRiseAnimations()
    {
        breathingTimer += Time.deltaTime * breatheSpeed;

        // 1. Hedef değerlere yumuşakça süzül (Lerp)
        currentHealPull = Mathf.Lerp(currentHealPull, targetHealPull, Time.deltaTime * animSpeed);
        currentRise = Mathf.Lerp(currentRise, targetRise, Time.deltaTime * animSpeed);

        // 2. Nefes alma sinüs dalgasını hesapla
        float sinValue = Mathf.Sin(breathingTimer) * breatheAmount * targetBreatheMultiplier;

        // 3. PARÇALARI HESAPLANAN MATEMATİĞE GÖRE YERLEŞTİR

        // Kafa: Yukarı/Aşağı nefes alır + Yükselme eklenir - İçeri/Aşağı kapanır
        if (headVisual != null)
            headVisual.localPosition = headHomePos + new Vector3(0, sinValue + currentRise - currentHealPull, 0);

        // Sol Kol: Sağa/Sola nefes alır + Yükselme eklenir + İçeri/Sağa kapanır
        if (leftArmVisual != null)
            leftArmVisual.localPosition = leftArmHomePos + new Vector3(-sinValue + currentHealPull, currentRise, 0);

        // Sağ Kol: Sağa/Sola nefes alır + Yükselme eklenir - İçeri/Sola kapanır
        if (rightArmVisual != null)
            rightArmVisual.localPosition = rightArmHomePos + new Vector3(sinValue - currentHealPull, currentRise, 0);

        // 🔴 ALT MODÜL: Sadece nefes alır ve içeri/yukarı kapanır. "currentRise" EKLENMEDİĞİ İÇİN YERDE KALIR!
        if (lowerModuleVisual != null)
            lowerModuleVisual.localPosition = lowerModuleHomePos + new Vector3(0, (-sinValue * 0.5f) + currentHealPull, 0);
    }

    private IEnumerator PerformHealSequence()
    {
        nextHealTime = Time.time + supportData.healCooldown;

        // 🔴 1. ANİMASYONU BAŞLAT (Update'e hedef veriyoruz, o kendisi süzülecek)
        targetBreatheMultiplier = 0.2f; // Kapanınca titremeyi çok aza indir
        targetHealPull = healParcaCekmeOffset; // Parçaları merkeze çek
        targetRise = healYukselmeMikatar; // Sadece üst gövdeyi havaya kaldır

        // Parçaların süzülerek hedefe varması için yarım saniye bekle
        yield return new WaitForSeconds(0.5f);

        // 🔴 2. HEAL ATMA ANI
        if (audioSource != null && healSound != null) audioSource.PlayOneShot(healSound);

        // Efekti düşmanın kafasında veya merkezinde çıkar
        if (healEffectPrefab != null && headVisual != null)
            Instantiate(healEffectPrefab, transform.position, Quaternion.identity);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, supportData.healRange);
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            if (enemyCollider.gameObject.layer == LayerMask.NameToLayer("Enemy") && enemyCollider.gameObject != gameObject)
            {
                NewBaseEnemyAI targetEnemy = enemyCollider.GetComponent<NewBaseEnemyAI>();
                if (targetEnemy != null)
                {
                    // Heal mantığı (Örn: targetEnemy.TakeDamage(-supportData.healAmount);)
                    targetEnemy.Heal(supportData.healAmount);
                }
            }
        }

        // Havada asılı kalarak şarj olmuş hissi vermek için biraz bekle
        yield return new WaitForSeconds(0.5f);

        // 🔴 3. ANİMASYONU BİTİR (Eski haline dön)
        targetBreatheMultiplier = 1f;
        targetHealPull = 0f;
        targetRise = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        if (supportData != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, supportData.healRange);
        }
    }
}