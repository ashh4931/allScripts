using UnityEngine;
using System.Collections;

public class gun5 : MonoBehaviour
{
    [Header("Gereksinimler")]
    public GameObject muzzleFlashPrefab;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Animator animator;

    [Header("Sesler")]
    public AudioClip fireSound;
    public AudioClip reloadSound; 
    private AudioSource audioSource;

    PlayerStats playerStats;
    private float nextFireTime = 0f;
    public float baseFireRate = 1;
    private float fireRate = 1f;

    [Header("Shotgun Ayarları")]
    public int maxAmmo = 2; 
    private int currentAmmo;
    public float reloadTimePerShell = 0.6f; 
    private bool isReloading = false;

    [Header("Shotgun Saçılma Ayarları")]
    public int pelletCount = 6;
    public float spreadAngle = 15f;

    [Header("Recoil Ayarları")]
    public float recoilAmount = 15.0f; 
    public float recoilRandomness = 5.0f;
    public float snappiness = 12f;
    public float returnSpeed = 2f;

    private Vector3 currentRotation;
    private Vector3 targetRotation;

    // --- YENİ EKLENEN DÖNÜŞ DEĞİŞKENİ ---
    private bool isSpinning = false;

    public void Start()
    {
        currentAmmo = maxAmmo;
        audioSource = GetComponent<AudioSource>();
    }

   void Update()
    {
        if (GetComponent<weapon>().isEquipped)
        {
            if (playerStats == null) playerStats = GetComponentInParent<PlayerStats>();
            fireRate = CombatMaths.CalculateFireRate(baseFireRate, playerStats.attackSpeed);

            // ATEŞ ETME MANTIĞI
            if (InputManager.GetKeyDown("Attack") && Time.time >= nextFireTime && !isReloading && currentAmmo > 0)
            {
                nextFireTime = Time.time + 1f / fireRate;
                Attack();
            }
            // RELOAD MANTIĞI (GÜNCELLENDİ)
            else if (!isReloading && (currentAmmo <= 0 || (InputManager.GetKeyDown("Reload") && currentAmmo < maxAmmo)))
            {
                // Artık doğrudan Reload() çağırmak yerine gecikmeli bir metod çağırıyoruz
                StartCoroutine(StartReloadWithDelay(0.3f));
            }
        }

        // --- RECOIL VE DÖNÜŞ MANTIĞI ---
        if (!isSpinning)
        {
            targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
            currentRotation = Vector3.Lerp(currentRotation, targetRotation, snappiness * Time.deltaTime);
            transform.localRotation = Quaternion.Euler(currentRotation);
        }
    }

    // --- YENİ: GECİKMEYİ SAĞLAYAN ARA METHOD ---
    IEnumerator StartReloadWithDelay(float delay)
    {
        isReloading = true; // Diğer Update döngülerinin tekrar girmesini engellemek için hemen true yapıyoruz
        yield return new WaitForSeconds(delay);
        StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        // isReloading zaten yukarıda true yapıldı, tekrar etmeye gerek yok
       // Debug.Log("Reload ve Dönüş Başladı...");

        if (reloadSound != null && audioSource != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(reloadSound);
        }

        float spinDuration = (reloadSound != null) ? reloadSound.length - 0.3f : 0.6f;
        
        isSpinning = true; 
        float elapsed = 0f;
        float startZRotation = transform.localEulerAngles.z;

        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float zRotation = Mathf.Lerp(0, 360f, elapsed / spinDuration);
            transform.localRotation = Quaternion.Euler(0, 0, startZRotation + zRotation);
            yield return null;
        }

        isSpinning = false;
        targetRotation = Vector3.zero;
        currentRotation = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        currentAmmo = maxAmmo;
        isReloading = false;
        //Debug.Log("Reload tamamlandı.");
    }

    void Attack()
    {
        currentAmmo--;

        for (int i = 0; i < pelletCount; i++)
        {
            float randomSpread = Random.Range(-spreadAngle, spreadAngle);
            Quaternion pelletRotation = firePoint.rotation * Quaternion.Euler(0, 0, randomSpread);
            Instantiate(bulletPrefab, firePoint.position, pelletRotation);
        }

        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            Destroy(flash, 0.5f);
        }

        PlayFireSound();
        StartCoroutine(ResetShootAnimation());
        Recoil();
    }

    // --- YENİLENMİŞ RELOAD VE DÖNÜŞ COROUTINE ---
   public float bodyKnockbackForce = 0.6f; // Makineli için daha güçlü
    private void Recoil()
    {
        targetRotation += new Vector3(0, 0, recoilAmount);

        HandController hand = GetComponentInParent<HandController>();
        if (hand != null)
        {
            // Namlu yönünü (firePoint.right) gönderiyoruz. 
            // HandController bunu otomatik olarak ters çevirip karakteri itecek.
            hand.ApplyRecoil(firePoint.right, bodyKnockbackForce);
        }
    }

    IEnumerator ResetShootAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("shoot", true);
            yield return new WaitForSeconds(0.1f);
            animator.SetBool("shoot", false);
        }
    }

    void PlayFireSound()
    {
        if (fireSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.85f, 1.15f); 
            audioSource.PlayOneShot(fireSound);
        }
    }
}