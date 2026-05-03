using UnityEngine;
using System.Collections;

public class gun17 : MonoBehaviour
{
    [Header("Görsel Dönüş Ayarları")]
    public Transform barrelTransform; // Unity'de "namlu" (part_1) objesini buraya sürükle
    public float maxBarrelRotationSpeed = 1500f; // Maksimum dönüş hızı
    public float accelerationSmoothness = 4f; // Hızlanma yumuşaklığı
    public float decelerationSmoothness = 1.5f; // Durma yumuşaklığı
    [Header("Isınma Ayarları")]
    public SpriteRenderer barrelRenderer; // Namlunun SpriteRenderer'ı
    public float heatLevel = 0f;          // Mevcut sıcaklık (0-1 arası)
    public float heatingRate = 1.5f;      // Ateş ederken saniyelik ısınma hızı
    public float coolingRate = 0.15f;     // Dururken saniyelik soğuma hızı
    public Color overheatedColor = Color.red; // Maksimum sıcaklıkta hangi renk olsun?
    private bool isOverheated = false;    // Aşırı ısınma kilidi
    private float currentBarrelRotationSpeed = 0f; // Anlık hız
    [Header("Gereksinimler")]
    public GameObject muzzleFlashPrefab;
    public GameObject bulletPrefab;
    public Transform firePoint;
    //public Animator animator;

    [Header("Minigun Ayarları")]
    public float spinUpTime = 1.2f;
    private float currentSpinTime = 0f;
    private bool isSpunUp = false;
    private bool isSpinning = false;

    [Header("Ses Ayarları")]
    public AudioClip fireSound;
    public AudioClip spinLoopSound;
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.4f;

    private AudioSource spinSource;   // Loop için
    private AudioSource fireSource;   // OneShot için
    private Coroutine soundRoutine;

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

    void Start()
    {
        AudioSource[] sources = GetComponents<AudioSource>();

        if (sources.Length < 2)
        {
            Debug.LogError("Minigun için 2 AudioSource gerekli!");
            return;
        }

        spinSource = sources[0];
        fireSource = sources[1];

        spinSource.loop = true;
        spinSource.playOnAwake = false;
        spinSource.volume = 0f;
        spinSource.clip = spinLoopSound;
    }


    void Update()
    {
        // 1. KUŞANILMA KONTROLÜ
        if (GetComponent<weapon>().isEquipped)
        {
            if (playerStats == null)
                playerStats = GetComponentInParent<PlayerStats>();

            fireRate = CombatMaths.CalculateFireRate(baseFireRate, playerStats.attackSpeed);

            HandleSpinUp();

            // 2. ATEŞ ETME VE ISINMA MANTIĞI
            // Sadece sağ tıkla döndürülmüşse (isSpunUp) ve aşırı ısınmamışsa ateş et
            if (InputManager.GetKeyDown("Attack") && isSpunUp && !isOverheated && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + 1f / fireRate;
                Attack();

                // Ateş ettikçe ısıyı artır
                heatLevel += heatingRate * Time.deltaTime;
            }
            else
            {
                // Ateş etmiyorken her zaman soğut
                heatLevel -= coolingRate * Time.deltaTime;
            }

            // Isı değerini 0-1 arasında tut
            heatLevel = Mathf.Clamp01(heatLevel);

            // 3. RENK DEĞİŞİMİ
            if (barrelRenderer != null)
            {
                barrelRenderer.color = Color.Lerp(Color.white, overheatedColor, heatLevel);
            }

            // 4. AŞIRI ISINMA KİLİDİ
            if (heatLevel >= 1f && !isOverheated)
            {
                isOverheated = true;
                // İsteğe bağlı: Aşırı ısınma sesi buraya eklenebilir
            }

            // Silah %20'ye kadar soğumadan tekrar ateş etmesine izin verme (Güvenlik eşiği)
            if (isOverheated && heatLevel < 0.2f)
            {
                isOverheated = false;
            }

            // 5. NAMLU DÖNDÜRME (X EKSENİ)
            if (isSpinning && !isOverheated) // Aşırı ısınmışsa dönmeyi de durdurabiliriz
            {
                currentBarrelRotationSpeed = Mathf.Lerp(currentBarrelRotationSpeed, maxBarrelRotationSpeed, Time.deltaTime * accelerationSmoothness);
                barrelTransform.Rotate(Vector3.right * currentBarrelRotationSpeed * Time.deltaTime);
            }
            else
            {
                currentBarrelRotationSpeed = Mathf.Lerp(currentBarrelRotationSpeed, 0f, Time.deltaTime * decelerationSmoothness);

                if (currentBarrelRotationSpeed > 10f)
                {
                    barrelTransform.Rotate(Vector3.right * currentBarrelRotationSpeed * Time.deltaTime);
                }
                else if (currentBarrelRotationSpeed > 0f)
                {
                    // Durmaya yakın 0 dereceye (başlangıç konumuna) yavaşça sabitle
                    barrelTransform.localRotation = Quaternion.Lerp(barrelTransform.localRotation, Quaternion.identity, Time.deltaTime * 5f);

                    if (Quaternion.Angle(barrelTransform.localRotation, Quaternion.identity) < 0.1f)
                    {
                        currentBarrelRotationSpeed = 0f;
                        barrelTransform.localRotation = Quaternion.identity;
                    }
                }
            }
        }

        // 6. RECOIL (HER ZAMAN ÇALIŞIR)
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation);
    }
    void HandleSpinUp()
    {
        if (InputManager.GetKeyDown("WeaponSpecial"))
        {
            isSpinning = true;
            if (soundRoutine != null) StopCoroutine(soundRoutine);
            soundRoutine = StartCoroutine(FadeInSound());
            //animator.SetBool("isSpinning", true);
        }

        if (InputManager.GetKeyDown("WeaponSpecial"))
        {
            currentSpinTime += Time.deltaTime;
            if (currentSpinTime >= spinUpTime)
                isSpunUp = true;
        }

        if (InputManager.GetKeyDown("WeaponSpecial"))
        {
            StopSpin();
        }
    }

    void StopSpin()
    {
        isSpinning = false;
        isSpunUp = false;
        currentSpinTime = 0f;
        //animator.SetBool("isSpinning", false);

        if (soundRoutine != null) StopCoroutine(soundRoutine);
        soundRoutine = StartCoroutine(FadeOutSound());
    }

    IEnumerator FadeInSound()
    {
        if (!spinSource.isPlaying)
            spinSource.Play();

        float timer = 0f;

        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeInDuration;
            spinSource.volume = Mathf.Lerp(0f, 1f, t);
            spinSource.pitch = Mathf.Lerp(0.8f, 1f, t);
            yield return null;
        }

        spinSource.volume = 1f;
        spinSource.pitch = 1f;
    }

    IEnumerator FadeOutSound()
    {
        float startVolume = spinSource.volume;
        float startPitch = spinSource.pitch;
        float timer = 0f;

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeOutDuration;
            spinSource.volume = Mathf.Lerp(startVolume, 0f, t);
            spinSource.pitch = Mathf.Lerp(startPitch, 0.6f, t);
            yield return null;
        }

        spinSource.volume = 0f;
        spinSource.Stop();
        spinSource.pitch = 1f;
    }

    void Attack()
    {
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            Destroy(flash, 1f);
        }

        PlayFireSound();
        StartCoroutine(ResetShootAnimation());
        Recoil();
    }

    void PlayFireSound()
    {
        if (fireSound != null)
        {
            // Her mermide çok hafif ton farkı (0.95 - 1.05 arası)
            fireSource.pitch = Random.Range(0.95f, 1.05f);
            fireSource.PlayOneShot(fireSound, 0.6f);
        }
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
        //animator.SetBool("shoot", true);
        yield return new WaitForSeconds(0.1f);
        // animator.SetBool("shoot", false);
    }

    private void OnDisable()
    {
        if (spinSource != null)
            spinSource.Stop();

        isSpinning = false;
        isSpunUp = false;
    }
}