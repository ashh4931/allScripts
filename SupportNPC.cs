using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SupportNPC : NewBaseEnemyAI
{
    [Header("Görsel Parçalar (5 Parça)")]
    public Transform visualsParent;
    public Transform headVisual;
    public Transform leftArmVisual;
    public Transform rightArmVisual;
    public Transform lowerModuleVisual;
    public Transform shadowSprite;

    [Header("Canlı Animasyon (Breathing) Ayarları")]
    public float breatheSpeed = 3f;
    public float breatheAmount = 0.15f;

    [Header("Heal Efektleri ve Yükselme")]
    public GameObject healEffectPrefab;
    public AudioClip healSound;
    public float healParcaCekmeOffset = 0.3f;
    public float healYukselmeMikatar = 1.0f;
    public float animSpeed = 8f;

    [Header("Mesafe ve Kaçış Ayarları")]
    public float minDistance = 5f;       // Oyuncuya 5 birime kadar yaklaşmasına izin ver
    public float maxDistance = 8f;       // Oyuncudan en fazla 8 birim uzaklaşsın (Ekranda kalır)
    public float catchUpDistance = 12f;  // Eğer 12 birimden uzaksa "koşarak" gelsin
    public float orbitStrength = 2.0f;   // Etrafında dönme hızı biraz daha belirgin olsun
    public float movementSmoothness = 2f; // Hareketin yumuşaklığı

    // Hafızaya alınacak orijinal yerleşimler
    private Vector3 headHomePos;
    private Vector3 leftArmHomePos;
    private Vector3 rightArmHomePos;
    private Vector3 lowerModuleHomePos;

    private SupportEnemyData supportData;
    private float nextHealTime;
    private float breathingTimer;

    private float targetBreatheMultiplier = 1f;
    private float targetHealPull = 0f;
    private float currentHealPull = 0f;
    private float targetRise = 0f;
    private float currentRise = 0f;

    // Orbit için rastgele yön (1 veya -1)
    private float orbitDirection = 1f;

    protected override void Start()
    {
        base.Start();
        supportData = baseData as SupportEnemyData;

        if (supportData == null)
            Debug.LogError(gameObject.name + ": Lütfen Inspector'dan bir SupportEnemyData atayın!");

        if (headVisual != null) headHomePos = headVisual.localPosition;
        if (leftArmVisual != null) leftArmHomePos = leftArmVisual.localPosition;
        if (rightArmVisual != null) rightArmHomePos = rightArmVisual.localPosition;
        if (lowerModuleVisual != null) lowerModuleHomePos = lowerModuleVisual.localPosition;

        // Her NPC farklı yöne dönsün
        orbitDirection = Random.value > 0.5f ? 1f : -1f;
        // Belirli aralıklarla orbit yönünü değiştirerek daha doğal hareket etmesini sağlayabiliriz
        InvokeRepeating(nameof(ChangeOrbitDirection), 3f, 5f);
    }

    private void ChangeOrbitDirection()
    {
        if (Random.value > 0.7f) orbitDirection *= -1f;
    }

    protected override void Update()
    {
        base.Update();
        if (isDead || isFrozen || player == null || supportData == null) return;

        HandleAliveAndRiseAnimations();

        if (Time.time >= nextHealTime)
        {
            StartCoroutine(PerformHealSequence());
        }
    }

    protected override void HandleMovement()
    {
        if (isDead || isFrozen || player == null || supportData == null) return;

        Vector2 vectorToPlayer = player.transform.position - transform.position;
        float distanceToPlayer = vectorToPlayer.magnitude;
        Vector2 directionToPlayer = vectorToPlayer.normalized;

        Vector2 desiredVelocity = Vector2.zero;
        float actualMoveSpeed = currentMoveSpeed;

        // --- YENİ: Yakalama Mantığı ---
        // Eğer oyuncu çok uzaklaştıysa NPC vites yükseltir
        if (distanceToPlayer > catchUpDistance)
        {
            actualMoveSpeed *= 2f; // İki kat hızla yaklaş
            desiredVelocity = directionToPlayer * actualMoveSpeed;
        }
        // Normal Takip/Kaçış Mesafeleri
        else if (distanceToPlayer < minDistance)
        {
            desiredVelocity = -directionToPlayer * actualMoveSpeed;
        }
        else if (distanceToPlayer > maxDistance)
        {
            desiredVelocity = directionToPlayer * actualMoveSpeed;
        }

        // --- Orbit (Etrafında Süzülme) ---
        // Oyuncuya yaklaştıkça süzülme (orbit) daha belirgin olur
        Vector2 orbitVec = new Vector2(-directionToPlayer.y, directionToPlayer.x) * orbitDirection;
        desiredVelocity += orbitVec * orbitStrength;

        // Yumuşak geçiş
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, desiredVelocity, Time.deltaTime * movementSmoothness);
    }

    // --- Görsel ve Heal Kodları Değişmedi ---

    private void HandleAliveAndRiseAnimations()
    {
        breathingTimer += Time.deltaTime * breatheSpeed;
        currentHealPull = Mathf.Lerp(currentHealPull, targetHealPull, Time.deltaTime * animSpeed);
        currentRise = Mathf.Lerp(currentRise, targetRise, Time.deltaTime * animSpeed);
        float sinValue = Mathf.Sin(breathingTimer) * breatheAmount * targetBreatheMultiplier;

        if (headVisual != null)
            headVisual.localPosition = headHomePos + new Vector3(0, sinValue + currentRise - currentHealPull, 0);
        if (leftArmVisual != null)
            leftArmVisual.localPosition = leftArmHomePos + new Vector3(-sinValue + currentHealPull, currentRise, 0);
        if (rightArmVisual != null)
            rightArmVisual.localPosition = rightArmHomePos + new Vector3(sinValue - currentHealPull, currentRise, 0);
        if (lowerModuleVisual != null)
            lowerModuleVisual.localPosition = lowerModuleHomePos + new Vector3(0, (-sinValue * 0.5f) + currentHealPull, 0);
    }

    private IEnumerator PerformHealSequence()
    {
        nextHealTime = Time.time + supportData.healCooldown;
        targetBreatheMultiplier = 0.2f;
        targetHealPull = healParcaCekmeOffset;
        targetRise = healYukselmeMikatar;

        yield return new WaitForSeconds(0.5f);

        if (healSound != null && AudioManager.instance != null)
        {
            // Sesi tam bulunduğu noktada (transform.position) 3D olarak çaldır. 
            // Volume: 1f, Min Distance: 5f (bu mesafeye kadar tam duyulur), Max Distance: 20f (bu mesafeden sonra duyulmaz)
            AudioManager.instance.Play3DSFXAtPosition(healSound, transform.position, 1f, 5f, 20f);
        }
        if (healEffectPrefab != null && headVisual != null)
            Instantiate(healEffectPrefab, transform.position, Quaternion.identity);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, supportData.healRange);
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            if (enemyCollider.gameObject.layer == LayerMask.NameToLayer("Enemy") && enemyCollider.gameObject != gameObject)
            {
                NewBaseEnemyAI targetEnemy = enemyCollider.GetComponent<NewBaseEnemyAI>();
                if (targetEnemy != null) targetEnemy.Heal(supportData.healAmount);
            }
        }

        yield return new WaitForSeconds(0.5f);

        targetBreatheMultiplier = 1f;
        targetHealPull = 0f;
        targetRise = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        // Mesafe sınırlarını editörde görmek için
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDistance);

        if (supportData != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, supportData.healRange);
        }
    }
}