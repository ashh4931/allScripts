using UnityEngine;

public class bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;
    public float maxDamage = 20f;
    public float minDamage = 5f;
    public float damageDropOffIntensity = 0.5f;

    public GameObject hitEffectPrefab;
    public AudioClip hitmark;
    [Range(0f, 1f)] public float hitmarkVolume = 0.5f;

    [Header("Penetration")]
    public int penetration = 1;

    private Vector2 startPosition;
    private bool hasPlayedSound = false; // 🔊 SES KONTROLÜ İÇİN YENİ DEĞİŞKEN

    void Start()
    {
        startPosition = transform.position;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

   // Doğru kullanım: Kod mutlaka bir fonksiyonun ({ }) içinde olmalı!
    private void playsoundeffect()
    {
        // Eğer ses zaten çalınmışsa veya ses yoksa fonksiyonu erkenden bitir
        if (hasPlayedSound || hitmark == null) return;

        // ESKİ KOD:
        // AudioSource.PlayClipAtPoint(hitmark, Camera.main.transform.position, hitmarkVolume);
        
        // YENİ KOD:
        AudioManager.instance.PlaySFXAtPosition(hitmark, Camera.main.transform.position, hitmarkVolume);
        
        hasPlayedSound = true; // Sesi çaldık, işaretle!
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable dmg = other.GetComponent<IDamageable>();

        if (dmg != null)
        {
            float distanceTraveled = Vector2.Distance(startPosition, transform.position);
            float calculatedDamage = maxDamage - (distanceTraveled * damageDropOffIntensity);
            calculatedDamage = Mathf.Max(calculatedDamage, minDamage);

            dmg.TakeDamage(calculatedDamage, minDamage, maxDamage);

            // Sesi çalmayı dener, fonksiyon içindeki hasPlayedSound kontrolü sayesinde sadece 1 kez çalışır
            playsoundeffect();

            penetration--;

            if (other.CompareTag("DestructibleObject") || other.CompareTag("Shield"))
            {
                penetration -= 3;
            }

            if (hitEffectPrefab != null) Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

            if (penetration <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}