using UnityEngine;
using System.Collections;

public class gun1_script : MonoBehaviour
{
    [Header("Gereksinimler")]
    public GameObject muzzleFlashPrefab;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Animator animator;

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
            if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo && !isReloading)
            {
                StartCoroutine(Reload());
            }

            // 2. ATEŞ ETME MANTIĞI
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

        // --- RECOIL VE DÖNÜŞ HESAPLAMA ---
        // Eğer silah özel bir dönüş (reload) YAPMIYORSA, normal recoil işlemini uygula
        if (!isSpinning)
        {
            targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
            // DİKKAT: Burada fixedDeltaTime yerine deltaTime kullanmalıyız. Slerp yerine Lerp daha stabildir.
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
        StartCoroutine(PlayCylinderSound());
        StartCoroutine(ResetShootAnimation());
        Recoil();

        Debug.Log("Kalan Mermi: " + currentAmmo);
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reload başladı...");

        if (animator != null) animator.SetTrigger("reload");

        if (reloadSound != null && audioSource != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(reloadSound);
        }

        // --- DÖNÜŞ SÜRESİ HESAPLAMALARI ---
        float spinDuration = 0.3f;
        float nekadaröncebaşlayacak = 1f;
        float waitTime = reloadTime - nekadaröncebaşlayacak;

        if (waitTime > 0)
        {
            yield return new WaitForSeconds(waitTime);
        }
        else
        {
            spinDuration = reloadTime;
        }

        // --- SON KISIMDA 360 DERECE DÖNÜŞ EFEKTİ ---
        isSpinning = true; // Update'teki normal hareketi DURDUR
        float elapsed = 0f;

        // Dönüşe başladığımız anki mevcut Z açısını alıyoruz
        float startZRotation = transform.localEulerAngles.z;

        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;

            // Sıfırdan 360'a doğru giden bir açı hesaplıyoruz
            float zRotation = Mathf.Lerp(0, 360f, elapsed / spinDuration);

            // Dönüşü manuel olarak Z eksenine uyguluyoruz
            transform.localRotation = Quaternion.Euler(0, 0, startZRotation + zRotation);

            yield return null;
        }

        // Dönüş bittiğinde her şeyi sıfırla ve normale döndür
        isSpinning = false;
        targetRotation = Vector3.zero;
        currentRotation = Vector3.zero; // Mevcut rotasyonu da sıfırlamalıyız ki aniden atlama yapmasın
        transform.localRotation = Quaternion.identity; // Silahı tam düz konuma getir (0,0,0)

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

    IEnumerator ResetShootAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("shoot", true);
            yield return new WaitForSeconds(0.1f);
            animator.SetBool("shoot", false);
        }
    }
}