using UnityEngine;

public class TutorialGyro : NewBaseEnemyAI
{[Header("GyroGunner Referansları")]
    //public Transform visualBody; // Sürekli dönen çark görseli
    public Transform handPivot;  // Silahı tutan obje
    public GameObject npcBulletPrefab; 

    [Header("El (Hand) Ayarları")]
    public float handDistance = 1.0f; // Elin gövdeden ne kadar uzakta duracağı
    public float handFollowSpeed = 15f; // Elin o pozisyona giderkenki hızı (Yumuşaklık)

    private NewRangedEnemyData rangedData;
    private NPCWeaponHandler weaponHandler;
    private float nextFireTime;

    protected override void Start()
    {
        base.Start();
        rangedData = baseData as NewRangedEnemyData;

        // Silah yöneticisini HandPivot'a ekle veya bul
        weaponHandler = handPivot.GetComponent<NPCWeaponHandler>();
        if (weaponHandler == null) weaponHandler = handPivot.gameObject.AddComponent<NPCWeaponHandler>();

        // Oyundan rastgele bir silah kuşan
        if (rangedData != null)
        {
            weaponHandler.EquipRandomWeapon(rangedData.weaponPrefabs);
        }
    }

    protected override void Update()
    {
        base.Update();

        // Çark görselini kendi etrafında sürekli döndür (Güzel bir efekt için)
        if (visualBody != null && !isDead && !isFrozen)
        {
            visualBody.Rotate(0, 0, 100f * Time.deltaTime);
        }
    }

    // 🔴 HAREKET: Oyuncuyla mesafeyi koru
    protected override void HandleMovement()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 directionToPlayer = (player.position - transform.position).normalized;

        if (distanceToPlayer > rangedData.preferredDistance)
        {
            // Çok uzak, yaklaş
            rb.linearVelocity = directionToPlayer * rangedData.moveSpeed;
        }
        else if (distanceToPlayer < rangedData.retreatDistance)
        {
            // Çok yakın, geri kaç (Kiting)
            rb.linearVelocity = -directionToPlayer * rangedData.moveSpeed;
        }
        else
        {
            // İdeal mesafede, dur (Veya istersen oyuncunun etrafında çember çizebilir)
            rb.linearVelocity = Vector2.zero; 
            
            // Çember çizmek istersen üstteki satırı silip şunu açabilirsin:
            // Vector2 orbitDirection = new Vector2(-directionToPlayer.y, directionToPlayer.x);
            // rb.velocity = orbitDirection * rangedData.orbitSpeed;
        }

        // Elin (ve silahın) yönünü oyuncuya çevir
        AimAtPlayer(directionToPlayer);
    }

    // 🔴 SALDIRI: Sürekli ateş et
    protected override void HandleAttack()
    {
        // Saldırırken de hareket etmeye devam etsin diye hareketi tekrar çağırıyoruz
        HandleMovement(); 

        if (Time.time >= nextFireTime)
        {
           weaponHandler.Fire(npcBulletPrefab, audioSource, rangedData.fireSound);
            nextFireTime = Time.time + (1f / rangedData.fireRate);
        }
    }

  private void AimAtPlayer(Vector2 direction)
    {
        if (handPivot == null) return;

        // --- 1. ROTASYON MANTIĞI (Silahın ters dönmesini engeller) ---
        float rotAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (direction.x < 0) // Oyuncu soldaysa (Silah sola bakıyorsa)
        {
            // Y ekseninde 180 derece döndürerek silahın baş aşağı olmasını engelliyoruz
            handPivot.rotation = Quaternion.Euler(0, 180, -rotAngle + 180);
        }
        else // Oyuncu sağdaysa (Silah sağa bakıyorsa)
        {
            // Normal rotasyon
            handPivot.rotation = Quaternion.Euler(0, 0, rotAngle);
        }

        // --- 2. POZİSYON MANTIĞI (Elin gövdenin etrafında yörünge çizmesi) ---
        // Gövde merkezinden, oyuncuya doğru 'handDistance' kadar ileride bir hedef nokta belirliyoruz
        Vector3 targetPosition = transform.position + (Vector3)direction * handDistance;
        
        // Eli o noktaya yumuşak bir şekilde (Lerp ile) götürüyoruz
        handPivot.position = Vector3.Lerp(handPivot.position, targetPosition, Time.deltaTime * handFollowSpeed);
    }
}