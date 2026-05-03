using UnityEngine;
using System.Collections;

public class CircularShield : MonoBehaviour
{
    private float currentHealth;
    private bool isActive = false;

    [Header("Görsel ve Ses")]
    public GameObject shieldVisual; 
    public AudioClip breakSound;
    public GameObject breakEffectPrefab; // Kırılırken dökülen parçalar
    private AudioSource audioSource;

    [Header("Materyal & Opaklık Animasyonu")]
    [Tooltip("Shader'daki opaklık değişkeninin referans adı (örn: _Opacity veya _Alpha)")]
    public string opacityPropertyName = "_Opacity"; 
    public float minOpacity = 0.02f;
    public float maxOpacity = 0.8f;
    public float hitOpacity = 1f;
    public float breathSpeed = 3f; // Nefes alma hızı (Değiştirebilirsin)
    public float hitRecoverySpeed = 0.7f; // 0.3'ten normale dönme hızı

    private Material shieldMaterial;
    private float hitIntensity = 0f; // Hasar parlama durumunu takip eder

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        if (shieldVisual != null) 
        {
            shieldVisual.SetActive(false);
            
            // Materyali alıp kopyasını oluşturuyoruz (sharedMaterial kullanıp diğer objeleri bozmamak için)
            Renderer renderer = shieldVisual.GetComponent<Renderer>();
            if (renderer != null)
            {
                shieldMaterial = renderer.material;
            }
        }
    }

    void Update()
    {
        // Eğer kalkan aktifse ve materyal başarıyla bulunduysa opaklık animasyonunu oynat
        if (isActive && shieldMaterial != null)
        {
            // 1. Nefes Alma Animasyonu: Zamanla 0 ile 1 arasında gidip gelir
            float t = (Mathf.Sin(Time.time * breathSpeed) + 1f) / 2f; 
            float breathingOpacity = Mathf.Lerp(minOpacity, maxOpacity, t);

            // 2. Hasar Parlaması Sönümlemesi: Eğer hasar alındıysa hitIntensity yavaşça 0'a düşer
            if (hitIntensity > 0f)
            {
                hitIntensity -= Time.deltaTime * hitRecoverySpeed;
                hitIntensity = Mathf.Clamp01(hitIntensity);
            }

            // 3. Nihai Opaklık: Normal nefes alma ile hasar parlaması (0.3) arasında yumuşak geçiş
            float currentOpacity = Mathf.Lerp(breathingOpacity, hitOpacity, hitIntensity);

            // 4. Materyale Uygulama
            shieldMaterial.SetFloat(opacityPropertyName, currentOpacity);
        }
    }

    public void ActivateShield(float health, float duration)
    {
        if (isActive) return; // Zaten açıksa tekrar açma

        currentHealth = health;
        isActive = true;
        hitIntensity = 0f; // Başlangıçta hasar efekti kapalı olsun
        shieldVisual.SetActive(true);

        // Belirli bir süre sonra hasar almasa bile kendi kendine kapansın
        StopAllCoroutines();
        StartCoroutine(DurationRoutine(duration));
    }

    public void TakeDamage(float amount, Vector3 hitPoint)
    {
        if (!isActive) return;

        currentHealth -= amount;
        
        // Hasar alındığında opaklığı anında 0.3'e çekmek için yoğunluğu (hitIntensity) fulle
        hitIntensity = 1f; 

        if (currentHealth <= 0)
        {
            TriggerBreak();
        }
    }

    private void TriggerBreak()
    {
        isActive = false;
        shieldVisual.SetActive(false);

        // Kırılma Sesi
        if (breakSound != null) audioSource.PlayOneShot(breakSound);

        // Kırılma Parçacıkları (Debris)
        if (breakEffectPrefab != null)
        {
            Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
        }
        
        StopAllCoroutines();
    }

    IEnumerator DurationRoutine(float time)
    {
        yield return new WaitForSeconds(time);
        if (isActive) TriggerBreak(); // Süre bitince kırılma efektiyle kapansın
    }

    public bool IsShieldActive() => isActive;
}