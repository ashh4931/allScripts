using UnityEngine;

public class CircularShieldDeflector : MonoBehaviour
{
    public CircularShield mainShieldScript;
    public GameObject hitEffectPrefab;
    public AudioClip hitSound;
    private AudioSource audioSource;

    [Header("Darbe Ayarları")]
    public float defaultBulletDamage = 15f;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!mainShieldScript.IsShieldActive()) return;

        if (other.CompareTag("EnemyBullet"))
        {
            HandleHit(other.gameObject);
        }
    }

    void HandleHit(GameObject bullet)
    {
        // Darbe Sesi
        if (hitSound != null) audioSource.PlayOneShot(hitSound);

        // Darbe Partikülü
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, bullet.transform.position, Quaternion.identity);
        }

        // Kalkana Hasar Ver
        mainShieldScript.TakeDamage(defaultBulletDamage, bullet.transform.position);

        // Mermiyi Yok Et
        Destroy(bullet);
    }
}