using UnityEngine;
using System.Collections;

public class gun2_script : MonoBehaviour
{
    [Header("Gereksinimler")]
    public GameObject muzzleFlashPrefab;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Pistol Sürgü (Slide) Ayarları")]
    public Transform slidePart; // part_1 buraya sürüklenecek
    public float slideRetractDistance = 0.15f; // Sürgünün ne kadar geri gideceği
    public float shootSlideRetractDistance = 0.25f;
    public float slideSpeed = 25f; // Sürgünün geri kapanma hızı (yay sertliği)
    public float slidePullDelay = 0.6f; // Reload sesindeki sürgü çekme anı (saniye)
    private Vector3 slideInitialLocalPos;
    public float slideWaitTime = 0.1f; // Sürgünün geride ne kadar bekleyeceği (saniye)

    [Header("Ses Ayarları")]
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip emptyClickSound;
    public AudioSource audioSource;

    [Header("Şarjör Ayarları")]
    public int maxAmmo = 12;
    private int currentAmmo;
    public float reloadTime = 1.5f;
    private bool isReloading = false;

    private PlayerStats playerStats;
    private HandController handController; // YENİ: Performans için baştan alıyoruz

    private float nextFireTime = 0f;
    public float baseFireRate = 2.5f;
    private float fireRate = 1f;

    [Header("Geri Tepme (Recoil) Ayarları")]
    public float visualHandKick = 0.5f;     // Elin görsel olarak geriye kayma miktarı
    public float bodyKnockbackForce = 5f;   // Karakterin fiziksel olarak geriye itilme gücü
    public float gunUpwardRecoil = 12f;     // Silahın kendi namlusunun havaya kalkma açısı
    public float recoilSnappiness = 15f;    // Geri tepmenin aniliği/hızı
    public float recoilReturnSpeed = 8f;    // Silahın namlusunun eski yerine dönme hızı

    private Vector3 slideTargetLocalPos;
    private float currentZRotation;
    private float targetZRotation;

    public void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        currentAmmo = maxAmmo;

        handController = GetComponentInParent<HandController>();
        playerStats = GetComponentInParent<PlayerStats>();

        if (audioSource == null)
            Debug.LogError("Gun: AudioSource bileşeni eksik!", this);

        if (slidePart != null)
        {
            slideInitialLocalPos = slidePart.localPosition;
            slideTargetLocalPos = slideInitialLocalPos;
        }
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

            // Manuel Reload
            if (InputManager.GetKeyDown("Reload") && currentAmmo < maxAmmo && !isReloading)
            {
                StartCoroutine(Reload());
            }

            // Ateş Etme
            if (InputManager.GetKeyDown("Attack") && Time.time >= nextFireTime && !isReloading)
            {
                if (currentAmmo > 0)
                {
                    nextFireTime = Time.time + 1f / fireRate;
                    Attack();
                }
                else
                {
// Parametre sırası: (ID, Uyarı mı?, Süre)  
HintManager.Instance.ShowHint("hint_firstReload", true , 4f);

                    PlayEmptySound();
                    nextFireTime = Time.time + 0.5f;
                }
            }
        }

        // --- SİLAHIN KENDİ ŞAHLANMASI (Görsel Recoil) ---
        targetZRotation = Mathf.Lerp(targetZRotation, 0f, recoilReturnSpeed * Time.deltaTime);
        currentZRotation = Mathf.Lerp(currentZRotation, targetZRotation, recoilSnappiness * Time.deltaTime);

        // Sadece Z ekseninde (namlu havaya kalkacak şekilde) döndürüyoruz
        transform.localRotation = Quaternion.Euler(0, 0, currentZRotation);

        // --- SÜRGÜ YAY SİSTEMİ (Mevcut kodlarına dokunulmadı) ---
        if (slidePart != null && !isReloading)
        {
            slidePart.localPosition = Vector3.Lerp(
                slidePart.localPosition,
                slideTargetLocalPos,
                Time.deltaTime * slideSpeed
            );
            slideTargetLocalPos = Vector3.Lerp(
                slideTargetLocalPos,
                slideInitialLocalPos,
                Time.deltaTime * slideSpeed
            );
        }
    }

    void Attack()
    {
        currentAmmo--;

        // Mermi Oluştur
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Muzzle Flash
        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            Destroy(flash, 0.5f);
        }

        PlayFireSound();

        // SÜRGÜYÜ GERİ İT (Ateş tepmesi)
        if (slidePart != null)
        {
            // Sürgünün geriye doğru itilmesi (x ekseninde - yönde)
            slideTargetLocalPos = slideInitialLocalPos - slidePart.right * shootSlideRetractDistance;
        }

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

    IEnumerator Reload()
    {
        isReloading = true;

        if (reloadSound != null && audioSource != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(reloadSound);
        }

        yield return new WaitForSeconds(slidePullDelay);

        if (slidePart != null)
        {
            float elapsed = 0;
            float pullDuration = 0.1f;
            // Sürgüyü geri çekme animasyonu
            Vector3 backPos = slideInitialLocalPos + new Vector3(slideRetractDistance * 1.5f, 0, 0); // Eksi (-) işareti silinmiş olabilir, test et

            while (elapsed < pullDuration)
            {
                slidePart.localPosition = Vector3.Lerp(slideInitialLocalPos, backPos, elapsed / pullDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(slideWaitTime);

            isReloading = false;
        }

        currentAmmo = maxAmmo;

        float remainingTime = reloadTime - slidePullDelay - 0.1f - slideWaitTime;
        if (remainingTime > 0)
            yield return new WaitForSeconds(remainingTime);

        isReloading = false;
    }

    void PlayEmptySound()
    {
        if (emptyClickSound != null && !audioSource.isPlaying)
            audioSource.PlayOneShot(emptyClickSound);
    }

    void PlayFireSound()
    {
        if (fireSound != null)
        {
            audioSource.pitch = Random.Range(0.8f, 1.2f);
            audioSource.PlayOneShot(fireSound);
        }
    }
}