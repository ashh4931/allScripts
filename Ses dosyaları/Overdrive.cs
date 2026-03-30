using System.Collections;
using UnityEngine;

public class Overdrive : MonoBehaviour
{

    public PlayerStats stats;

    [Header("Buff Ayarları")]
    public float speedBonus = 4f;
    public float defenseBonus = 4f;

    [Header("Görsel ve Ses")]
    public AudioClip overdriveSound;
    public float breathSpeed = 5f; // Rengin ne kadar hızlı yanıp söneceği
    public Color overdriveColor = Color.red;

    private AudioSource audioSource;
    public SpriteRenderer spriteRenderer;

    // Kendi oyunundaki stat scriptlerinin referanslarını buraya girmelisin:
    // private PlayerController playerController;
    // private PlayerStats playerStats;

    private bool isActive = false;
    private float duration;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // SpriteRenderer ve stat scriptleri genelde ana oyuncu objesinde (Parent) bulunur:


        // playerController = GetComponentInParent<PlayerController>();
        // playerStats = GetComponentInParent<PlayerStats>();
    }

    // Yetenek tetiklendiğinde burası çalışır
    public void Use(float activeTime)
    {
        if (isActive) return; // Eğer yetenek zaten aktifse üst üste basmayı engelle

        duration = activeTime; // ScriptableObject'ten gelen aktif kalma süresi
        StartCoroutine(OverdriveRoutine());

    }

    private IEnumerator OverdriveRoutine()
    {
        isActive = true;

        // 1. SES ÇAL (Sadece ilk basıldığında oynatılır)
        if (overdriveSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(overdriveSound);
        }

        // 2. STATLARI ARTIR
        // KENDİ OYUNUNA GÖRE BURAYI AÇMALISIN:
        stats.movSpeed += speedBonus;
         
        stats.defense += defenseBonus;


        float timer = 0f;
        Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        // 3. SÜRE BOYUNCA BEKLE VE "BREATH" EFEKTİ UYGULA
        while (timer < duration)
        {
            timer += Time.deltaTime;

            if (spriteRenderer != null)
            {
                // PingPong metodu, zaman geçtikçe 0 ile 1 arasında gidip gelir (Nefes alma hissi verir)
                float lerp = Mathf.PingPong(Time.time * breathSpeed, 1f);

                // Normal renk ile Kırmızı arasında yumuşak geçiş yap
                spriteRenderer.color = Color.Lerp(originalColor, overdriveColor, lerp);
            }

            yield return null; // Bir sonraki Frame'e geç (Oyunun donmaması için şart)
        }

        // 4. SÜRE BİTTİ: STATLARI GERİ AL VE RENGİ SIFIRLA
        // KENDİ OYUNUNA GÖRE BURAYI AÇMALISIN:
        stats.movSpeed -= speedBonus;
         stats.defense -= defenseBonus;
      

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor; // Rengi tamamen eski haline döndür
        }

        Debug.Log("Overdrive Bitti! Statlar normale döndü.");
        isActive = false;
    }
}