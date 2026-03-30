using UnityEngine;
using System.Collections;

public class gun1 : MonoBehaviour
{
    [Header("Gereksinimler")]
    public GameObject muzzleFlashPrefab;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Animator animator;

    [Header("Ses")]
    public AudioClip fireSound;
    private AudioSource audioSource;

    private PlayerStats playerStats;
    private HandController handController; // YENİ: Performans için baştan alıyoruz

    private float nextFireTime = 0f;
    public float baseFireRate = 1;
    private float fireRate = 1f;

    [Header("Geri Tepme (Recoil) Ayarları")]
    public float visualHandKick = 0.5f;     // Elin görsel olarak geriye kayma miktarı
    public float bodyKnockbackForce = 5f;   // Karakterin fiziksel olarak geriye itilme gücü
    public float gunUpwardRecoil = 15f;     // Silahın kendi namlusunun havaya kalkma açısı
    public float recoilSnappiness = 15f;    // Geri tepmenin aniliği/hızı
    public float recoilReturnSpeed = 8f;    // Silahın namlusunun eski yerine dönme hızı

    private float currentZRotation;
    private float targetZRotation;

    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            Debug.LogError("Gun1: AudioSource yok!", this);

        // Performans: Update yerine Start'ta componentleri bul
        handController = GetComponentInParent<HandController>();
        playerStats = GetComponentInParent<PlayerStats>();
    }

    void Update()
    {
        // Silah elimizde mi kontrolü
        weapon weaponComponent = GetComponent<weapon>();
        if (weaponComponent != null && weaponComponent.isEquipped)
        {
            // PlayerStats yoksa bulmaya çalış (silah sonradan ele alındıysa diye)
            if (playerStats == null)
                playerStats = GetComponentInParent<PlayerStats>();

            if (playerStats != null)
                fireRate = CombatMaths.CalculateFireRate(baseFireRate, playerStats.attackSpeed);

            // Ateş Etme Mantığı
            if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + 1f / fireRate;
                Attack();
            }
        }

        // --- SİLAHIN KENDİ ŞAHLANMASI (Görsel Recoil) ---
        targetZRotation = Mathf.Lerp(targetZRotation, 0f, recoilReturnSpeed * Time.deltaTime);
        currentZRotation = Mathf.Lerp(currentZRotation, targetZRotation, recoilSnappiness * Time.deltaTime);
        
        // Sadece Z ekseninde (namlu havaya kalkacak şekilde) döndürüyoruz
        transform.localRotation = Quaternion.Euler(0, 0, currentZRotation);
    }

    void Attack()
    {
        // 1. Mermiyi Oluştur
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Debug.Log("Gun1: mermi ateşlendi.");

        // 2. Muzzle Flash Oluştur
        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            Destroy(flash, 1f);
        }

        PlayFireSound();
        StartCoroutine(ResetShootAnimation());
        Recoil();
    }

    private void Recoil()
    {
        // 1. Silahın namlusunu havaya kaldır
        targetZRotation += gunUpwardRecoil;

        // 2. HandController'a el görselini ve vücut itişini tetiklemesini söyle
        if (handController != null)
        {
            // firePoint.right, merminin çıkış yönünü verir
            Vector2 fireDirection = firePoint.right; 
            handController.TriggerRecoil(fireDirection, visualHandKick, bodyKnockbackForce);
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
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(fireSound);
        }
    }
}