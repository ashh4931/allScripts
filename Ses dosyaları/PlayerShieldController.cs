using UnityEngine;

public class PlayerShieldController : MonoBehaviour
{
    [Header("Kalkan Referansları")]
    public GameObject shieldVisual; 
    public Transform shieldPivot;   
    
    [Header("Kalkan Hareket Ayarları")]
    public float rotationSpeed = 15f; 
    
    [Header("Stamina Ayarları")]
    public float activeDrainRate = 15f; 
    public float hitDrainAmount = 20f;  
    public float minStaminaToActivate = 25f; 

    [Header("Kırılma (Guard Break) Ayarları")]
    public float breakDamageThreshold = 40f; 
    public float breakCooldown = 2f;         
    public AudioClip shieldBreakSound;       
    private AudioSource audioSource;

    private PlayerStats stats;
    public bool isShieldActive { get; private set; }
    public float activationTime { get; private set; }

    private Vector3 originalScale;
    private float currentCooldown = 0f;      

    void Start()
    {
        stats = GetComponent<PlayerStats>();
        audioSource = gameObject.AddComponent<AudioSource>();
        
        if (shieldVisual != null)
        {
            originalScale = shieldVisual.transform.localScale;
            shieldVisual.SetActive(false); 
        }
    }

    void Update()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }

        HandleShieldInput();
        
        if (isShieldActive)
        {
            AimShieldAtMouse();
            DrainStaminaOverTime();
        }
    }

    void HandleShieldInput()
    {
        // 🔴 2. PROBLEM ÇÖZÜLDÜ: Sadece tuşa İLK basıldığında tetiklenir
        if (Input.GetMouseButtonDown(1))
        {
            if (!isShieldActive && currentCooldown <= 0f && stats.stamina >= minStaminaToActivate)
            {
                isShieldActive = true;
                shieldVisual.SetActive(true);
                activationTime = Time.time; 
            }
        }
        // Tuş BIRAKILDIĞINDA kapat
        else if (Input.GetMouseButtonUp(1))
        {
            if (isShieldActive)
            {
                isShieldActive = false;
                shieldVisual.SetActive(false);
            }
        }
    }

    void AimShieldAtMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePos - shieldPivot.position;
        
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

        // Kalkanın süzülerek hedefe gitmesi
        shieldPivot.rotation = Quaternion.Slerp(shieldPivot.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // 🔴 1. PROBLEM ÇÖZÜLDÜ: Kalkanın O ANKİ gerçek açısını buluyoruz
        float currentZ = shieldPivot.eulerAngles.z;
        
        // Unity açıları 0-360 arası verir, bunu -180 ile +180 arasına çekiyoruz
        if (currentZ > 180f) currentZ -= 360f;

        if (shieldVisual != null)
        {
            // Kalkan FİZİKSEL OLARAK sol tarafa geçtiğinde (90 dereceyi aştığında) ters çevir
            if (Mathf.Abs(currentZ) > 90f)
            {
                shieldVisual.transform.localScale = new Vector3(originalScale.x, -Mathf.Abs(originalScale.y), originalScale.z);
            }
            else
            {
                shieldVisual.transform.localScale = new Vector3(originalScale.x, Mathf.Abs(originalScale.y), originalScale.z);
            }
        }
    }

    void DrainStaminaOverTime()
    {
        stats.stamina -= activeDrainRate * Time.deltaTime;
        if (stats.stamina <= 0 && isShieldActive)
        {
            stats.stamina = 0;
            TriggerShieldBreak(); 
        }
    }

    public bool TakeShieldDamage(float damageAmount)
    {
        stats.stamina -= damageAmount;

        if (damageAmount >= breakDamageThreshold || stats.stamina <= 0)
        {
            stats.stamina = 0;
            TriggerShieldBreak();
            return true; 
        }
        return false; 
    }

    private void TriggerShieldBreak()
    {
        isShieldActive = false;
        shieldVisual.SetActive(false);
        currentCooldown = breakCooldown;

        if (audioSource != null && shieldBreakSound != null)
        {
            audioSource.PlayOneShot(shieldBreakSound, 1f);
        }
    }
}