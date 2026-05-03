using UnityEngine;
using System.Collections;

public enum ShieldEnemyState { Defense, Prep, Attack, Cooldown }

public class TriShieldEnemyAI : NewBaseEnemyAI
{
    [Header("Özel Boss Ayarları")]
    public ShieldEnemyState currentState = ShieldEnemyState.Defense;
    
    [Header("Görsel Referanslar")]
    public Transform shieldPivot;      
    public Transform coreVisual;       
    public EnemyShield[] shields;      

    [Header("Saldırı Noktaları")]
    public Transform[] firePoints;     
    public AudioClip attackSound;
   
    // --- 🔴 CANLI KALKANLAR (NEFES ALMA) AYARLARI ---
    [Header("Canlı Kalkan Ayarları")]
    public float breathingSpeed = 2f;    // Yakınlaşıp uzaklaşma hızı (Nefes sıklığı)
    public float maxDistanceOffset = 0.4f; // Kalkanlar ne kadar uzağa/yakına gitsin? (0.4 birim)

    // --- 🔴 YUMUŞAK SQUASH (ESNEME) AYARLARI ---
    [Header("Yumuşak Atış (Squash) Ayarları")]
    [Range(0f, 1f)] public float squashEffectAmount = 0.5f; // Mermi fırlatınca ne kadar basıklaşsın? (0 = hiç, 1 = çok)
    public float squashReturnSpeed = 10f; // Ne kadar hızla eski haline dönsün?

    private TriShieldEnemyData triData;

    private float stateTimer;
    private float currentRotSpeed;
    private float flashTimer;
    private bool isFlashOrange;

    // Animasyon değişkenleri
    private float targetCoreSquash = 1f; // Çekirdeğin hedef basıklık değeri (1 = normal, 0.5 = basık)
    private float currentCoreSquash = 1f;
    private Vector3 originalCoreScale;
    private float breathingTimer;

    protected override void Start()
    {
        base.Start();
        
        triData = baseData as TriShieldEnemyData;
        
        if (triData == null)
        {
            Debug.LogError("TriShieldEnemyAI: Lütfen Base Data kısmına bir 'TriShieldEnemyData' sürükleyin!");
            return;
        }

        // Orijinal boyutları kaydet
        if (coreVisual != null) originalCoreScale = coreVisual.localScale;

        SwitchState(ShieldEnemyState.Defense); 
    }

    protected override void Update()
    {
        base.Update();
        if (isDead || isFrozen || player == null || triData == null) return;

        stateTimer -= Time.deltaTime;

        // --- 🔴 1. CANLI KALKANLAR (NEFES ALMA VE DÖNÜŞ) KONTROLÜ ---
        if (shieldPivot != null)
        {
            // Sinüs dalgası kullanarak kalkanların pozisyonunu sürekli değiştiriyoruz
            breathingTimer += Time.deltaTime * breathingSpeed;
            float currentDistanceEffect = Mathf.Sin(breathingTimer) * maxDistanceOffset;
            shieldPivot.localPosition = new Vector3(0, currentDistanceEffect, 0);

            // Döngü durumuna göre dönüşü yap
            switch (currentState)
            {
                case ShieldEnemyState.Defense:
                    shieldPivot.Rotate(0, 0, currentRotSpeed * Time.deltaTime);
                    if (stateTimer <= 0) SwitchState(ShieldEnemyState.Prep);
                    break;

                case ShieldEnemyState.Prep:
                    currentRotSpeed = Mathf.Lerp(currentRotSpeed, 0, Time.deltaTime * 5f);
                    shieldPivot.Rotate(0, 0, currentRotSpeed * Time.deltaTime);
                    break;
            }
        }

        // --- 🔴 2. YUMUŞAK SQUASH (ESNEME) KONTROLÜ (UPDATE İÇİNDE) ---
        if (coreVisual != null)
        {
            // Hedef basıklığa Lerp ile yumuşak geçiş
            currentCoreSquash = Mathf.Lerp(currentCoreSquash, targetCoreSquash, Time.deltaTime * squashReturnSpeed);
            
            // X'i genişlet, Y'yi basıklaştır (Squash)
            coreVisual.localScale = new Vector3(originalCoreScale.x * (1f + (1f - currentCoreSquash)), originalCoreScale.y * currentCoreSquash, originalCoreScale.z);

            // Hedef basıklığı sürekli "1" (Normal) durumuna çekmeye çalış, böylece bir "yay" etkisi oluşur
            targetCoreSquash = Mathf.MoveTowards(targetCoreSquash, 1f, Time.deltaTime * squashReturnSpeed);
        }

        // --- DÖNGÜ VE FLAŞ KONTROLÜ (MEVCUT KODUN) ---
        if (currentState == ShieldEnemyState.Prep)
        {
            flashTimer -= Time.deltaTime;
            if(flashTimer <= 0)
            {
                isFlashOrange = !isFlashOrange;
                SetAllShieldsColor(isFlashOrange ? triData.warningColor : triData.defenseColor);
                flashTimer = 0.15f; 
            }
            if (stateTimer <= 0) SwitchState(ShieldEnemyState.Attack);
        }
        else if (currentState == ShieldEnemyState.Attack)
        {
            if (stateTimer <= 0) SwitchState(ShieldEnemyState.Cooldown);
        }
        else if (currentState == ShieldEnemyState.Cooldown)
        {
            if (stateTimer <= 0) SwitchState(ShieldEnemyState.Defense);
        }
    }

    protected override void HandleMovement()
    {
        if (currentState == ShieldEnemyState.Defense)
        {
            base.HandleMovement(); 
        }
        else
        {
            rb.linearVelocity = Vector2.zero; 
        }
    }

    void SwitchState(ShieldEnemyState newState)
    {
        currentState = newState;
        switch (newState)
        {
            case ShieldEnemyState.Defense:
                stateTimer = triData.defenseTime;
                currentRotSpeed = triData.fastRotationSpeed;
                SetAllShieldsColor(triData.defenseColor);
                break;

            case ShieldEnemyState.Prep:
                stateTimer = triData.prepTime;
                flashTimer = 0f;
                rb.linearVelocity = Vector2.zero; 
                break;

            case ShieldEnemyState.Attack:
                stateTimer = triData.attackTime;
                SetAllShieldsColor(triData.warningColor); 
                FireBullets();
                SquashCoreAnim();
                break;

            case ShieldEnemyState.Cooldown:
                stateTimer = triData.cooldownTime;
                SetAllShieldsColor(triData.defenseColor); 
                break;
        }
    }

    void FireBullets()
    {
        if (triData.bulletPrefab == null) return;
        
        if (attackSound != null && audioSource != null) 
            audioSource.PlayOneShot(attackSound);

        foreach (Transform fp in firePoints)
        {
            if (fp != null)
            {
                GameObject bullet = Instantiate(triData.bulletPrefab, fp.position, fp.rotation);
                Rigidbody2D brb = bullet.GetComponent<Rigidbody2D>();
                if (brb != null) brb.linearVelocity = fp.up * triData.bulletSpeed; 
            }
        }
    }

    // 🔴 ESKİ SERT SQUASH'I SİLDİK, YERİNE BU YUMUŞAK SİSTEMİ KURDUK
    void SquashCoreAnim()
    {
        // Fırlatma anında çekirdeği anında basıklaştır (SQUASH)
        // Update fonksiyonundaki Lerp onu yumuşakça eski haline geri döndürecek.
        currentCoreSquash = 1f - squashEffectAmount; // Örn: 0.5f
    }

    void SetAllShieldsColor(Color c)
    {
        foreach (EnemyShield shield in shields)
        {
            if (shield != null && shield.gameObject.activeSelf)
            {
                shield.SetColor(c);
            }
        }
    }
}