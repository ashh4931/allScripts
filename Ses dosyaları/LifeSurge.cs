using UnityEngine;

public class LifeSurge : MonoBehaviour
{
    [Header("Görsel ve Ses")]
    public GameObject surgeEffectPrefab; // Devasa bir iyileşme efekti
    public AudioClip surgeSound;
    
    private AudioSource audioSource;
    private PlayerStats playerStats;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        // Can sisteminin olduğu scripti buluyoruz (PlayerStats olduğunu varsaydım)
        playerStats = GetComponentInParent<PlayerStats>(); 
    }

    // Dikkat: Artık miktar dışarıdan (ScriptableObject'ten) geliyor!
    public void Use(float amount)
    {
        // 1. Canı ver
        if (playerStats != null)
        {
            // Kendi can verme fonksiyonun neyse onunla değiştir. Örnek:
            // playerStats.currentHealth += amount; 
            // playerStats.currentHealth = Mathf.Clamp(playerStats.currentHealth, 0, playerStats.maxHealth);
            Debug.Log($"Life Surge kullanıldı! {amount} can yenilendi.");
        }

        // 2. Sesi çal
        if (surgeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(surgeSound);
        }

        // 3. Partikül efektini yarat
        if (surgeEffectPrefab != null)
        {
            Instantiate(surgeEffectPrefab, transform.position, Quaternion.identity, transform);
        }
    }
}