using UnityEngine;
using System.Collections;

public class wand1 : MonoBehaviour
{
    [Header("Gereksinimler")]
    public GameObject muzzleFlashPrefab;
    public GameObject[] bulletPrefabs; // 🔴 Birden fazla bullet
    public Transform firePoint;
    public Animator animator;
    
    [Header("Ses")]
    public AudioClip fireSound;
    private AudioSource audioSource;
    public float pitchMin = 0.8f;
    public float pitchMax = 1.3f;
    PlayerStats playerStats;

    [Header("Geri Tepme (Recoil)")]
    [Tooltip("Asa ile ateş edildiğinde ele uygulanacak geri tepme şiddeti")]
    public float recoilIntensity = 0.2f; // Hafif bir geri tepme değeri
    private HandController handController; // HandController referansı

    private float nextFireTime = 0f;

    [Tooltip("İki atış arası temel süre (saniye)")]
    public float baseFireRate = 0.25f;

    private float fireRate;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Asanın bağlı olduğu ebeveyn (parent) objeden HandController'ı bul
        handController = GetComponentInParent<HandController>();
    }

    void Update()
    {
        if (!GetComponent<weapon>().isEquipped)
            return;

        if (playerStats == null)
            playerStats = GetComponentInParent<PlayerStats>();

        if (playerStats != null)
        {
            fireRate = CombatMaths.CalculateFireRate(
                baseFireRate,
                playerStats.attackSpeed
            );
        }

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Attack();
        }
    }

    void Attack()
    {
        // 1. Mermi Oluşturma
        if (bulletPrefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, bulletPrefabs.Length);
            Instantiate(
                bulletPrefabs[randomIndex],
                firePoint.position,
                firePoint.rotation
            );
        }

        // 2. Namlu Ateşi (Muzzle Flash)
        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(
                muzzleFlashPrefab,
                firePoint.position,
                firePoint.rotation
            );
            Destroy(flash, 1f);
        }

        // 3. Geri Tepme (Recoil) Uygulama
        if (handController != null)
        {
            // Ateş edilen yönün tam tersine (-transform.right) geri tepme gücü uygula
            handController.ApplyRecoil(-transform.right, recoilIntensity);
        }

        // 4. Ses Çalma
        PlayFireSound();
    }

    void PlayFireSound()
    {
        if (fireSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(pitchMin, pitchMax);
            audioSource.PlayOneShot(fireSound);
        }
    }
}