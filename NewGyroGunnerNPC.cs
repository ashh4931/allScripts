using UnityEngine;

public class NewGyroGunnerNPC : NewBaseEnemyAI
{
    [Header("GyroGunner Referansları")]
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
    // NewGyroGunnerNPC.cs içindeki HandleMovement güncellemesi
    protected override void HandleMovement()
    {
        if (player == null || isDead || isFrozen) return;

        // 1. ADIM: Oyuncunun etrafındaki "Kişisel Slot" konumunu hesapla
        float dynamicPreferredDist = rangedData.preferredDistance + personalDistanceOffset;

        // Radyan cinsinden açıyı hesapla
        float angleRad = personalOrbitAngle * Mathf.Deg2Rad;

        // Oyuncunun etrafında, belirlenen mesafede ve açıda bir hedef nokta oluştur
        Vector2 orbitOffset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * dynamicPreferredDist;

        // Tahmini (Predicted) pozisyonu da işin içine katarak hedefi belirle
        Vector2 predictedPlayerPos = GetPredictedPlayerPosition(0.8f);
        Vector2 targetSlotPos = predictedPlayerPos + orbitOffset;

        // 2. ADIM: Context Steering'i bu "Slot" noktasına gitmek için kullan
        // Bu sayede hem slota gider hem de yoldaki diğer NPC'lerden kaçınır
        Vector2 steerDir = GetContextSteeringVelocity(targetSlotPos);

        // 3. ADIM: Mesafe Kontrolü ve Hareket
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Eğer slota çok yakınsa ve ideal mesafedeyse yavaşça orbit (dönüş) yapması için açıyı güncelle
        if (distanceToPlayer <= dynamicPreferredDist + 1f && distanceToPlayer >= dynamicPreferredDist - 1f)
        {
            // Zamanla açıyı değiştirerek oyuncunun etrafında dönmelerini sağla (Opsiyonel)
            personalOrbitAngle += Time.deltaTime * rangedData.orbitSpeed * 10f;
        }

        // Hareket kuvvetini uygula
        rb.linearVelocity = steerDir * currentMoveSpeed;

        // Silah her zaman GERÇEK oyuncu konumuna bakmalı
        Vector2 realDirection = (player.position - transform.position).normalized;
        AimAtPlayer(realDirection);
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