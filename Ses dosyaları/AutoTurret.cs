using UnityEngine;
using System.Collections.Generic;

public class AutoTurret : MonoBehaviour
{
    public PlayerTurretData turretData;
    
    [Header("Ateşleme Ayarları (Çoklu Namlu)")]
    public bool fireAllAtOnce = true; 
    private int currentFirePointIndex = 0; 

    [Header("Dönüş Ayarları")]
    public float aimRotationSpeed = 10f; 
    public bool sweepWhenIdle = true;
    public float idleRotationSpeed = 2f;
    public float sweepAngle = 45f;

    [Header("Görsel Efektler (Geri Tepme & Namlu Alevi)")]
    public Transform turretVisual; 
    public float recoilDistance = 0.2f; 
    public float recoilRecoverySpeed = 10f; 
    public GameObject muzzleFlashPrefab; 

    [Header("Isınma (Overheat) Ayarları")]
    public bool useOverheat = true; 
    public float maxHeat = 100f; 
    public float heatPerShot = 15f; 
    public float coolingRate = 25f; 
    
    [Header("Overheat Görsel & Ses Ekstraları")]
    public AudioClip overheatSound; // Taret bozulduğunda çıkacak ses
    public AudioClip reactivateSound; // 🔴 YENİ: Tekrar çalıştığında çıkacak ses
    public GameObject overheatSmokeEffect; // 🔴 YENİ: Duman partikülü objesi
    public float shutdownAngle = -90f; // 🔴 YENİ: Bozulduğunda boynunu bükeceği açı (Aşağı bakması için genelde -90 veya 180'dir)
    public float shutdownSpeed = 15f; // 🔴 YENİ: Yere düşme/Bozulma hızı (Pat diye düşmesi için yüksek tutabilirsin)

    private float currentHeat = 0f;
    private bool isOverheated = false;
    private Vector3 originalVisualPos;
    private float currentRecoil = 0f;
    private SpriteRenderer visualSprite;

    private List<Transform> firePoints = new List<Transform>();
    private float nextFireTime;
    private Transform currentTarget;
    private AudioSource audioSource;
    private Quaternion startingRotation;
    private float randomTimeOffset;

    void Start()
    {
        startingRotation = transform.rotation;
        randomTimeOffset = Random.Range(0f, 1000f);
        audioSource = gameObject.AddComponent<AudioSource>();

        if (turretVisual != null)
        {
            originalVisualPos = turretVisual.localPosition;
            visualSprite = turretVisual.GetComponent<SpriteRenderer>();
        }

        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.name.ToLower().Contains("firepoint"))
            {
                firePoints.Add(child);
            }
        }
        
        // Oyun başlarken duman efektini kapalı tut
        if (overheatSmokeEffect != null) overheatSmokeEffect.SetActive(false);
    }

    void Update()
    {
        if (turretData == null) return;

        HandleVisualsAndHeat();

        // 🔴 GÜNCELLENDİ: Eğer taret aşırı ısındıysa diğer hiçbir şeyi yapma, sadece kapanma (Shutdown) animasyonunu oynat!
        if (isOverheated)
        {
            ShutdownMovement();
            return;
        }

        FindClosestEnemy();

        if (currentTarget != null)
        {
            AimAtTarget();

            if (Time.time >= nextFireTime)
            {
                Fire();
                nextFireTime = Time.time + turretData.fireRate;
            }
        }
        else
        {
            IdleMovement();
        }
    }

    void HandleVisualsAndHeat()
    {
        // Geri Tepme
        if (turretVisual != null)
        {
            currentRecoil = Mathf.Lerp(currentRecoil, 0f, recoilRecoverySpeed * Time.deltaTime);
            turretVisual.localPosition = originalVisualPos - new Vector3(currentRecoil, 0, 0);
        }

        // Isınma ve Soğuma
        if (useOverheat)
        {
            if (currentHeat > 0)
            {
                currentHeat -= coolingRate * Time.deltaTime; 
                
                if (currentHeat <= 0)
                {
                    currentHeat = 0;
                    
                    // 🔴 YENİ: Eğer taret ısınma krizinden YENİ çıktıysa (Recovery)
                    if (isOverheated)
                    {
                        isOverheated = false; 
                        
                        // Aktif olma sesini çal
                        if (audioSource != null && reactivateSound != null)
                        {
                            audioSource.PlayOneShot(reactivateSound, 1f);
                        }
                        
                        // Dumanı kapat
                        if (overheatSmokeEffect != null) overheatSmokeEffect.SetActive(false);
                    }
                }
            }

            // Isıya göre renk değiştirme (Kızarma)
            if (visualSprite != null)
            {
                float heatPercent = currentHeat / maxHeat;
                visualSprite.color = Color.Lerp(Color.white, new Color(1f, 0.3f, 0.3f), heatPercent);
            }
        }
    }

    // 🔴 YENİ: Taret bozulduğunda boynunu bükme hareketi
    void ShutdownMovement()
    {
        // Bozulduğunda bakması gereken açıyı hesapla
        Quaternion targetShutdownRotation = startingRotation * Quaternion.Euler(0, 0, shutdownAngle);
        
        // Hızla yere doğru bükül
        transform.rotation = Quaternion.Slerp(transform.rotation, targetShutdownRotation, shutdownSpeed * Time.deltaTime);
    }

    void IdleMovement()
    {
        if (!sweepWhenIdle)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, startingRotation, aimRotationSpeed * Time.deltaTime);
            return;
        }

        float angleOffset = Mathf.Sin((Time.time + randomTimeOffset) * idleRotationSpeed) * sweepAngle;
        Quaternion targetIdleRotation = startingRotation * Quaternion.Euler(0, 0, angleOffset);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetIdleRotation, aimRotationSpeed * Time.deltaTime);
    }

    void FindClosestEnemy()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, turretData.range);
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                NewBaseEnemyAI enemyAI = enemy.GetComponent<NewBaseEnemyAI>();
                if (enemyAI != null && enemyAI.isActiveAndEnabled) 
                {
                    float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
                    if (distanceToEnemy < closestDistance)
                    {
                        closestDistance = distanceToEnemy; 
                        closestEnemy = enemy.transform;
                    }
                }
            }
        }
        currentTarget = closestEnemy;
    }

    void AimAtTarget()
    {
        Vector2 direction = currentTarget.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetAimRotation = Quaternion.Euler(0, 0, angle);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetAimRotation, aimRotationSpeed * Time.deltaTime);
    }

    void Fire()
    {
        if (firePoints.Count == 0 || turretData.bulletPrefab == null) return;

        // Isı Ekle
        if (useOverheat)
        {
            currentHeat += heatPerShot;
            if (currentHeat >= maxHeat)
            {
                // 🔴 YENİ: AŞIRI ISINMA TETİKLENDİ!
                isOverheated = true;
                
                // Bozulma sesini çal
                if (audioSource != null && overheatSound != null)
                {
                    audioSource.PlayOneShot(overheatSound, 1f); 
                }
                
                // Duman efektini başlat
                if (overheatSmokeEffect != null) overheatSmokeEffect.SetActive(true);
                
                return; // Mermiyi atma
            }
        }

        currentRecoil = recoilDistance;

        if (fireAllAtOnce)
        {
            foreach (Transform fp in firePoints) ShootFromPoint(fp);
        }
        else
        {
            ShootFromPoint(firePoints[currentFirePointIndex]);
            currentFirePointIndex++;
            if (currentFirePointIndex >= firePoints.Count) currentFirePointIndex = 0; 
        }

        // Ateş etme sesi (Turret Data'dan gelir)
        if (audioSource != null && turretData.fireSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(turretData.fireSound, 0.6f);
        }
    }

    void ShootFromPoint(Transform fp)
    {
        Instantiate(turretData.bulletPrefab, fp.position, fp.rotation);

        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, fp.position, fp.rotation, fp);
            Destroy(flash, 0.05f); 
        }
    }
}