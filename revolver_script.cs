using UnityEngine;
using System.Collections;

public class gun1_script : MonoBehaviour
{
    [Header("Gereksinimler")]
    public GameObject muzzleFlashPrefab;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Ses Ayarları")]
    public AudioClip fireSound;
    public AudioClip cylinderSpinSound;
    public AudioClip reloadSound;
    public AudioClip emptyClickSound;
    public AudioSource audioSource;
    public float cylinderSoundDelay = 0.2f;

    [Header("Şarjör Ayarları")]
    public int maxAmmo = 6;
    private int currentAmmo;
    public float reloadTime = 1.5f;
    private bool isReloading = false;

    [Header("Reload Animasyon Ayarları")]
    public float spinDuration = 0.4f; // Sadece dönüşün ne kadar süreceği (reloadTime'dan küçük olmalı)

    PlayerStats playerStats;
    private float nextFireTime = 0f;
    public float baseFireRate = 1;
    private float fireRate = 1f;

    [Header("Recoil Ayarları")]
    public float recoilAmount = 10.0f;
    public float recoilRandomness = 10.0f;
    public float snappiness = 10f;
    public float returnSpeed = 1f;

    private Vector3 currentRotation;
    private Vector3 targetRotation;

    // Silahın sadece dönerken recoil düzeltmesini durdurmak için bir kontrol
    private bool isSpinning = false;

    public void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        currentAmmo = maxAmmo;

        if (audioSource == null)
            Debug.LogError("Gun1: AudioSource bileşeni eksik!", this);
    }

    void Update()
    {
        if (GetComponent<weapon>().isEquipped)
        {
            if (playerStats == null)
            {
                playerStats = GetComponentInParent<PlayerStats>();
            }

            fireRate = CombatMaths.CalculateFireRate(baseFireRate, playerStats.attackSpeed);

            // 1. MANUEL RELOAD (R Tuşu)
            if (InputManager.GetKeyDown("Reload") && currentAmmo < maxAmmo && !isReloading)
            {
                StartCoroutine(Reload());
            }

            // 2. ATEŞ ETME MANTIĞI
            if  (InputManager.GetKeyDown("Attack") && Time.time >= nextFireTime && !isReloading)
            {
                if (currentAmmo > 0)
                {
                    nextFireTime = Time.time + 1f / fireRate;
                    Attack();
                }
                else
                {
                    PlayEmptySound();
                    nextFireTime = Time.time + 0.5f;
                }
            }
        }

        // --- RECOIL VE DÖNÜŞ HESAPLAMA ---
        if (!isSpinning)
        {
            targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
            currentRotation = Vector3.Lerp(currentRotation, targetRotation, snappiness * Time.deltaTime);
            transform.localRotation = Quaternion.Euler(currentRotation);
        }
    }

 void Attack()
    {
        currentAmmo--;

        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            Destroy(flash, 1f);
        }

        PlayFireSound();
        // cylinderSpinSound buradan kaldırıldı!
        Recoil();

        Debug.Log("Kalan Mermi: " + currentAmmo);
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reload başladı...");

        float soundDuration = 0f;

        // 1. SESİ ÇAL VE SÜRESİNİ HESAPLA (Mermi dolum sesi)
        if (reloadSound != null && audioSource != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(reloadSound);
            soundDuration = reloadSound.length; 
        }
        else
        {
            soundDuration = reloadTime; 
        }

        // 2. MERMİ DOLUM SESİNİN BİTMESİNİ BEKLE
        yield return new WaitForSeconds(soundDuration);

        // 3. ANİMASYON BAŞLARKEN SİLİNDİR DÖNÜŞ SESİNİ ÇAL
        if (cylinderSpinSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.98f, 1.02f);
            audioSource.PlayOneShot(cylinderSpinSound);
        }

        // 4. DÖNÜŞ (ANİMASYON) BAŞLASIN
        isSpinning = true; 
        float elapsed = 0f;
        float startZRotation = transform.localEulerAngles.z;

        while (elapsed < spinDuration) // spinDuration, Inspector'dan ayarladığın dönüş süresi
        {
            elapsed += Time.deltaTime;
            float zRotation = Mathf.Lerp(0, 360f, elapsed / spinDuration);
            transform.localRotation = Quaternion.Euler(0, 0, startZRotation + zRotation);
            yield return null;
        }

        // 5. DÖNÜŞ BİTTİ, MERMİYİ DOLDUR VE SİLAH YERİNE OTURSUN
        isSpinning = false;
        targetRotation = Vector3.zero;
        currentRotation = Vector3.zero; 
        transform.localRotation = Quaternion.identity; 

        currentAmmo = maxAmmo;
        isReloading = false;

        Debug.Log("Reload tamamlandı.");
    }
 

    void PlayEmptySound()
    {
        if (emptyClickSound != null && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(emptyClickSound);
        }
    }

    void PlayFireSound()
    {
        if (fireSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(fireSound);
        }
    }

    IEnumerator PlayCylinderSound()
    {
        yield return new WaitForSeconds(cylinderSoundDelay);
        if (cylinderSpinSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.98f, 1.02f);
            audioSource.PlayOneShot(cylinderSpinSound);
        }
    }

    private void Recoil()
    {
        targetRotation += new Vector3(0, 0, recoilAmount + Random.Range(-recoilRandomness, recoilRandomness));
        HandController hand = GetComponentInParent<HandController>();
        if (hand != null)
        {
            hand.ApplyRecoil(firePoint.right, 0.4f);
        }
    }
}