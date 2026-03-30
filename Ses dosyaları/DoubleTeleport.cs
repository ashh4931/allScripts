using UnityEngine;

public class DoubleTeleport : MonoBehaviour
{
    [Header("Teleport Ayarları")]
    [Tooltip("Oyuncu tek seferde kaç birim uzağa ışınlanacak?")]
    public float teleportDistance = 5f; 
    
    [Tooltip("Işınlanma anında çıkacak partikül efekti (Opsiyonel)")]
    public GameObject teleportEffectPrefab; 
    public AudioClip teleportSound;

    private AudioSource audioSource;
    private Transform playerTransform;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        // Işınlanacak ana objeyi (Player) buluyoruz. Bu script child objesindeyse Parent'ı alır.
        playerTransform = transform.parent != null ? transform.parent : transform;
    }

    public void Use()
    {
        // 1. Ses Çal (Üst üste atınca aynı ses çıkmasın diye pitch değiştiriyoruz)
        if (teleportSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.15f);
            audioSource.PlayOneShot(teleportSound);
        }

        // 2. Eski konumda bir efekt yarat
        if (teleportEffectPrefab != null)
        {
            Instantiate(teleportEffectPrefab, playerTransform.position, Quaternion.identity);
        }

        // 3. Yönü Bul (Fare imlecine doğru)
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f; // 2D oyun olduğu için Z eksenini sıfırlıyoruz

        Vector3 direction = (mousePos - playerTransform.position).normalized;

        // 4. Işınla! (Mevcut konumdan, fare yönüne 'teleportDistance' kadar ileri at)
        playerTransform.position += direction * teleportDistance;

        // 5. Yeni (gittiği) konumda da bir efekt yarat
        if (teleportEffectPrefab != null)
        {
            Instantiate(teleportEffectPrefab, playerTransform.position, Quaternion.identity);
        }
    }
}