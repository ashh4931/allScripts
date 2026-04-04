using UnityEngine;
using System.Collections;

public class gun_4 : MonoBehaviour
{
    [Header("Gereksinimler")]
    public GameObject muzzleFlashPrefab; 
    public GameObject bulletPrefab;
    public Transform firePoint;
    

    [Header("Mermi ve Şarjör Ayarları")]
    public int clipSize = 30;           
    public float reloadTime = 1.5f;     
    public float autoReloadDelay = 0.5f; // Ateş ve Reload arasındaki o ufak gecikme
    private int currentAmmo;            
    private bool isReloading = false;   

    [Header("Zoom Out Ayarları")]
    public Camera mainCamera; 
    public float zoomOutAmount = 2f;
    public float zoomSpeed = 5f;
    private float initialSize;

    [Header("Ses Ayarları")]
    public AudioClip fireSound;
    public AudioClip reloadSound; 
    private AudioSource audioSource;

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

    public void Start()
    {
        currentAmmo = clipSize;
        audioSource = GetComponent<AudioSource>();
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera != null) initialSize = mainCamera.orthographicSize;
    }

    void Update()
    {
        if (GetComponent<weapon>().isEquipped == true)
        {
            if (playerStats == null) playerStats = GetComponentInParent<PlayerStats>();
            fireRate = CombatMaths.CalculateFireRate(baseFireRate, playerStats.attackSpeed);

            HandleZoomOut();

            if (Input.GetMouseButton(0) && Time.time >= nextFireTime && !isReloading)
            {
                if (currentAmmo > 0)
                {
                    nextFireTime = Time.time + 1f / fireRate;
                    Attack();
                }
                else
                {
                    StartCoroutine(DelayedReload(autoReloadDelay));
                }
            }
        }

        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    void HandleZoomOut()
    {
        if (mainCamera == null) return;
        float targetSize = Input.GetMouseButton(1) ? (initialSize + zoomOutAmount) : initialSize;
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetSize, zoomSpeed * Time.deltaTime);
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
        StartCoroutine(ResetShootAnimation());
        Recoil();

        if (currentAmmo <= 0 && !isReloading)
        {
            StartCoroutine(DelayedReload(autoReloadDelay));
        }
    }

    IEnumerator DelayedReload(float delay)
    {
        isReloading = true; // Bekleme anında bile ateş edilmesini engellemek için
        yield return new WaitForSeconds(delay);
        StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        // isReloading zaten DelayedReload içinde true yapıldı
        if (reloadSound != null && audioSource != null)
            audioSource.PlayOneShot(reloadSound);

       

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = clipSize; 
        isReloading = false;
    }
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
        
        yield return new WaitForSeconds(0.1f);
        
    }

    void PlayFireSound()
    {
        if (fireSound != null && audioSource != null)
            audioSource.PlayOneShot(fireSound);
    }
}