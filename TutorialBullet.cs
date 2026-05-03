using UnityEngine;

public class TutorialBulet : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;
    public float maxDamage = 20f;
    public float minDamage = 5f;
    public float damageDropOffIntensity = 0.5f;

    [Header("Tutorial Settings")]
    public float activationDistance = 5f; // Kontrol edilecek mesafe
    private Transform playerTransform;   // Oyuncunun transformu (cache)
    private bool tutorialTriggered = false; // Fonksiyonun sadece bir kez çalışması için

    public GameObject hitEffectPrefab;
    public AudioClip hitmark;

    [Header("Penetration")]
    public int penetration = 1;

    private Vector2 startPosition;

    void Start()
    {
        startPosition = transform.position;

        // Oyuncuyu tag üzerinden bul ve transformunu al
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);

        // Mesafe kontrolü fonksiyonunu çağırıyoruz
        CheckProximityToPlayer();
    }

    private void CheckProximityToPlayer()
    {
        if (playerTransform == null || tutorialTriggered) return;

        // Oyuncu ile mermi arasındaki güncel mesafeyi hesapla
        float currentDistance = Vector2.Distance(transform.position, playerTransform.position);

        if (currentDistance <= activationDistance)
        {
            OnProximityReached();
        }
    }

    private void OnProximityReached()
    {
        tutorialTriggered = true; // Tekrar tekrar tetiklenmesini engelle

 
HintManager.Instance.ShowHint("hint_shield", true, 5f);
    }

    [Range(0f, 1f)] public float hitmarkVolume = 0.5f;

    private void playsoundeffect()
    {
        if (hitmark != null)
        {
            AudioManager.instance.PlaySFXAtPosition(hitmark, Camera.main.transform.position, hitmarkVolume);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        IDamageable dmg = other.GetComponent<IDamageable>();

        if (dmg != null)
        {
            float distanceTraveled = Vector2.Distance(startPosition, transform.position);
            float calculatedDamage = maxDamage - (distanceTraveled * damageDropOffIntensity);
            calculatedDamage = Mathf.Max(calculatedDamage, minDamage);

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