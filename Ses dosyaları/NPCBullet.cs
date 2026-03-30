using UnityEngine;

public class NPCBullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;
    public float maxDamage = 20f;
    public float minDamage = 5f; // Hasarın düşebileceği minimum değer
    public float damageDropOffIntensity = 0.5f; // Metre başına ne kadar hasar düşecek?

    public GameObject hitEffectPrefab;
    public AudioClip hitmark;

    [Header("Penetration")]
    public int penetration = 1;

    private Vector2 startPosition; // Merminin ateşlendiği nokta

    void Start()
    {
        startPosition = transform.position; // Doğduğu yeri kaydet
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    [Range(0f, 1f)] public float hitmarkVolume = 0.5f; // Inspector'dan 0.5 yaparsan %50 ses verir

    private void playsoundeffect()
    {
        if (hitmark != null)
        {
            // 3. parametre olarak sesi ekliyoruz (0.5f %50 ses seviyesidir)
            AudioSource.PlayClipAtPoint(hitmark, Camera.main.transform.position, hitmarkVolume);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return; 
        }
        IDamageable dmg = other.GetComponent<IDamageable>();

        if (dmg != null)
        {
            float distanceTraveled = Vector2.Distance(startPosition, transform.position);
            float calculatedDamage = maxDamage - (distanceTraveled * damageDropOffIntensity);
            calculatedDamage = Mathf.Max(calculatedDamage, minDamage);

            // 🔴 DEĞİŞTİRİLEN SATIR: Artık min ve max bilgisini de yolluyoruz!
            dmg.TakeDamage(calculatedDamage, minDamage, maxDamage);

            playsoundeffect();
            penetration--;

            if (other.CompareTag("DestructibleObject"))
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