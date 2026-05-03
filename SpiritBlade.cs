using UnityEngine;

public class SpiritBlade : MonoBehaviour
{
    [Header("Referanslar")]
    public SpriteRenderer spriteRenderer; // 🔴 YENİ: Inspector'dan kılıcın görselini sürükle

    [Header("Saldırı ve Hasar")]
    public float damage = 15f;
    public GameObject hitEffectPrefab;
    public AudioClip hitSound;

    [Header("Zincirleme (Sekme) Ayarları")]
    public int maxHitsPerAttack = 3;
    public float bounceRadius = 6f;
    public LayerMask enemyLayer;

    [Header("Uçuş Hızları")]
    public float attackSpeed = 35f;
    public float returnSpeed = 15f;

    public enum BladeState { Trailing, Attacking, Returning }
    public BladeState currentState = BladeState.Trailing;

    [HideInInspector] public Vector3 targetRestPos;
    [HideInInspector] public float targetRestAngle;

    private Transform targetEnemy;
    private int currentHitCount = 0;
    private float hitCooldown = 0.2f; 
    private float lastHitTime;
    private Transform playerTransform; // 🔴 YENİ: Y-Sorting için oyuncuyu takip eder

    // Manager tarafından yaratılırken çağrılır
    public void Setup(Transform player)
    {
        playerTransform = player;
    }

    void Update()
    {
        // --- 🔴 YENİ: Y Eksenine Göre Sprite Katmanı (Sorting Order) ---
        if (spriteRenderer != null && playerTransform != null)
        {
            // Eğer kılıç oyuncunun daha altındaysa (Y değeri küçükse) öne al (4), değilse arkaya at (2)
            spriteRenderer.sortingOrder = transform.position.y < playerTransform.position.y ? 4 : 2;
        }

        switch (currentState)
        {
            case BladeState.Trailing:
                transform.position = Vector3.Lerp(transform.position, targetRestPos, Time.deltaTime * 10f);
                
                // 🔴 YENİ: Artık doğrudan Manager'ın verdiği (Dik veya eğik) açıyı kullanıyor
                Quaternion targetRot = Quaternion.Euler(0, 0, targetRestAngle);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * 8f);
                break;

            case BladeState.Attacking:
                if (targetEnemy == null || !targetEnemy.gameObject.activeInHierarchy)
                {
                    currentState = BladeState.Returning;
                    break;
                }

                transform.position = Vector3.MoveTowards(transform.position, targetEnemy.position, attackSpeed * Time.deltaTime);

                Vector2 dir = targetEnemy.position - transform.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                // 🔴 YENİ: -90 yerine +90 yaptık, böylece 180 derece dönüp ucuyla saplanacak
                transform.rotation = Quaternion.Euler(0, 0, angle + 90f);

                if (Vector2.Distance(transform.position, targetEnemy.position) < 0.5f)
                {
                    DealDamage(targetEnemy.GetComponent<Collider2D>());
                }
                break;

            case BladeState.Returning:
                transform.position = Vector3.MoveTowards(transform.position, targetRestPos, returnSpeed * Time.deltaTime);

                Vector2 retDir = targetRestPos - transform.position;
                float retAngle = Mathf.Atan2(retDir.y, retDir.x) * Mathf.Rad2Deg;
                // 🔴 YENİ: Dönüşte de sivri ucu önde olacak şekilde (+90)
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, retAngle + 90f), Time.deltaTime * 15f);

                if (Vector2.Distance(transform.position, targetRestPos) < 0.2f)
                {
                    currentState = BladeState.Trailing;
                }
                break;
        }
    }

    public void AttackTarget(Transform enemy)
    {
        if (currentState == BladeState.Trailing)
        {
            targetEnemy = enemy;
            currentHitCount = 0;
            currentState = BladeState.Attacking;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        DealDamage(other);
    }

    private void DealDamage(Collider2D other)
    {
        if (Time.time < lastHitTime + hitCooldown || other == null) return;

        IDamageable dmgInterface = other.GetComponent<IDamageable>();
        if (dmgInterface != null)
        {
            dmgInterface.TakeDamage(damage);
            lastHitTime = Time.time;
            currentHitCount++;

            if (hitEffectPrefab != null) Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            if (hitSound != null && AudioManager.instance != null)
                AudioManager.instance.PlaySFXAtPosition(hitSound, transform.position, 0.6f, Random.Range(0.9f, 1.1f));

            if (currentState == BladeState.Attacking)
            {
                if (currentHitCount < maxHitsPerAttack)
                {
                    Transform nextTarget = FindNextTarget(other.transform);
                    if (nextTarget != null) targetEnemy = nextTarget;
                    else currentState = BladeState.Returning;
                }
                else
                {
                    currentState = BladeState.Returning;
                }
            }
        }
    }

    private Transform FindNextTarget(Transform excludeEnemy)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, bounceRadius, enemyLayer);
        Transform bestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.transform == excludeEnemy) continue;

            if (hit.GetComponent<IDamageable>() != null)
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    bestTarget = hit.transform;
                }
            }
        }
        return bestTarget;
    }
}