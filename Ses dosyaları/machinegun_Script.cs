using UnityEngine;
using System.Collections;

public class gun6_script : MonoBehaviour
{
    [Header("Gereksinimler")]
    public GameObject muzzleFlashPrefab;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Şarjör (Magazine) Ayarları")]
    public Transform magazinePart; 
    public float reloadTime = 1.5f;
    public float dropDelay = 0.2f;

    private Vector3 magInitialLocalPos;
    private Quaternion magInitialLocalRot;
    private MeshRenderer magRenderer;

    [Header("Ses Ayarları")]
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip emptyClickSound;
    public AudioSource audioSource;

    [Header("Mermi Ayarları")]
    public int maxAmmo = 12;
    private int currentAmmo;
    private bool isReloading = false;

    private PlayerStats playerStats;
    private HandController handController; // YENİ: Performans

    private float nextFireTime = 0f;
    public float baseFireRate = 2.5f;
    private float fireRate = 1f;

    [Header("Geri Tepme (Recoil) Ayarları")]
    public float visualHandKick = 0.8f;     // Makineli için biraz daha yüksek sarsıntı
    public float bodyKnockbackForce = 1.5f; // Her mermide karakteri ne kadar iteceği
    public float gunUpwardRecoil = 8f;      // Silahın namlusunun kalkma açısı
    public float recoilSnappiness = 15f;    
    public float recoilReturnSpeed = 10f;

    private float currentZRotation;
    private float targetZRotation;

    public void Start()
    { if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        currentAmmo = maxAmmo;

        handController = GetComponentInParent<HandController>();
        playerStats = GetComponentInParent<PlayerStats>();

        if (magazinePart != null)
        {
            magInitialLocalPos = magazinePart.localPosition;
            magInitialLocalRot = magazinePart.localRotation;
            magRenderer = magazinePart.GetComponent<MeshRenderer>();
        }

        if (audioSource == null)
            Debug.LogError("Gun: AudioSource bileşeni eksik!");
    }

    void Update()
    {
        weapon weaponComponent = GetComponent<weapon>();
        if (weaponComponent != null && weaponComponent.isEquipped)
        {
            if (playerStats == null)
                playerStats = GetComponentInParent<PlayerStats>();

            if (playerStats != null)
                fireRate = CombatMaths.CalculateFireRate(baseFireRate, playerStats.attackSpeed);

            if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo && !isReloading)
            {
                StartCoroutine(ReloadRoutine());
            }

            if (Input.GetMouseButton(0) && Time.time >= nextFireTime && !isReloading)
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

        // --- SİLAHIN KENDİ ŞAHLANMASI (Görsel Recoil) ---
        targetZRotation = Mathf.Lerp(targetZRotation, 0f, recoilReturnSpeed * Time.deltaTime);
        currentZRotation = Mathf.Lerp(currentZRotation, targetZRotation, recoilSnappiness * Time.deltaTime);
        
        transform.localRotation = Quaternion.Euler(0, 0, currentZRotation);
    }

    void Attack()
    {
        currentAmmo--;
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            Destroy(flash, 0.5f);
        }

        PlayFireSound();
        Recoil();
    }

    private void Recoil()
    {
        targetZRotation += gunUpwardRecoil;

        if (handController != null)
        {
            // Merminin gerçek gidiş yönü
            Vector2 fireDirection = firePoint.right;
            handController.TriggerRecoil(fireDirection, visualHandKick, bodyKnockbackForce);
        }
    }

    IEnumerator ReloadRoutine()
    {
        isReloading = true;

        if (reloadSound != null && audioSource != null)
            audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(dropDelay);

        if (magazinePart != null)
        {
            Transform oldParent = magazinePart.parent;
            magazinePart.SetParent(null);

            Rigidbody rb = magazinePart.gameObject.GetComponent<Rigidbody>();
            if (rb == null) rb = magazinePart.gameObject.AddComponent<Rigidbody>();

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddRelativeForce(Vector3.down * 2f, ForceMode.Impulse);

            yield return new WaitForSeconds(reloadTime - dropDelay - 0.2f);

            if (magRenderer != null) magRenderer.enabled = false;

            rb.isKinematic = true;
            magazinePart.SetParent(oldParent);
            magazinePart.localPosition = magInitialLocalPos;
            magazinePart.localRotation = magInitialLocalRot;

            yield return new WaitForSeconds(0.1f);
            if (magRenderer != null) magRenderer.enabled = true;
        }

        currentAmmo = maxAmmo;
        isReloading = false;
    }

    void PlayEmptySound() { if (emptyClickSound != null && audioSource != null) audioSource.PlayOneShot(emptyClickSound); }
    
    void PlayFireSound() 
    { 
        if (fireSound != null && audioSource != null) 
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(fireSound); 
        }
    }
}