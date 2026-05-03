using UnityEngine;
using System.Collections.Generic;

public class SnakeSpell : MonoBehaviour
{
    private enum SnakeState { Slinking, Lunging, Retreating }
    private SnakeState currentState = SnakeState.Slinking;

    [Header("Hareket Ayarları")]
    public float speed = 8f;
    public float lungeSpeed = 16f;
    public float retreatSpeed = 10f; // Vurduktan sonra kaçma hızı
    public float amplitude = 3f; 
    public float frequency = 10f;
    public float lifeTime = 5f;

    [Header("Yılan Gövdesi")]
    public GameObject segmentPrefab;
    public int segmentCount = 3;
    public int positionDelay = 5;

    [Header("Saldırı Ayarları")]
    public float detectionRadius = 6f;
    public LayerMask hitLayer;
    public float damage = 15f;
    public int maxHits = 3; // Toplam kaç kere vurabilir?
    
    private int currentHits = 0;
    private float retreatTimer = 0f;
    private float retreatDuration = 0.6f; // Ne kadar süre kaçacak?
    private Vector2 retreatDirection;

    private List<GameObject> segments = new List<GameObject>();
    private List<Vector3> posHistory = new List<Vector3>();
    private List<Quaternion> rotHistory = new List<Quaternion>();
    
    private Transform target;
    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;
        for (int i = 0; i < segmentCount; i++)
        {
            segments.Add(Instantiate(segmentPrefab, transform.position, transform.rotation));
        }
        Invoke("DestroySnake", lifeTime);
    }

    void Update()
    {
        switch (currentState)
        {
            case SnakeState.Slinking:
                HandleSlinking();
                break;
            case SnakeState.Lunging:
                HandleLunging();
                break;
            case SnakeState.Retreating:
                HandleRetreating();
                break;
        }

        RecordAndMoveSegments();
    }

    // 1. Durum: Normal Süzülme ve Av Arama
    void HandleSlinking()
    {
        float elapsed = Time.time - spawnTime;
        float wiggle = Mathf.Sin(elapsed * frequency) * amplitude;
        transform.Translate(new Vector3(wiggle, speed, 0) * Time.deltaTime, Space.Self);

        // Hedef ara
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, hitLayer);
        if (hit != null)
        {
            target = hit.transform;
            currentState = SnakeState.Lunging;
        }
    }

    // 2. Durum: Hedefe Atılma
    void HandleLunging()
    {
        if (target == null) { currentState = SnakeState.Slinking; return; }

        Vector2 direction = (target.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 12f);
        
        transform.Translate(Vector3.up * lungeSpeed * Time.deltaTime, Space.Self);
    }

    // 3. Durum: Vurduktan Sonra Kaçma
    void HandleRetreating()
    {
        retreatTimer -= Time.deltaTime;
        
        // Rastgele veya geldiği yönün tersine doğru kaçış
        transform.Translate(Vector3.up * retreatSpeed * Time.deltaTime, Space.Self);
        
        if (retreatTimer <= 0)
        {
            target = null; // Eski hedefi unut
            currentState = SnakeState.Slinking;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || currentState == SnakeState.Retreating) return;

        IDamageable dmg = other.GetComponent<IDamageable>();
        if (dmg != null)
        {
            // Hasar ver
            dmg.TakeDamage(damage, damage * 0.5f, damage * 2f);
            
            currentHits++;
            
            if (currentHits >= maxHits)
            {
                DestroySnake();
            }
            else
            {
                // Vur-Kaç Moduna Geç
                StartRetreat();
            }
        }
    }

    void StartRetreat()
    {
        currentState = SnakeState.Retreating;
        retreatTimer = retreatDuration;
        
        // Vurduğu noktadan uzaklaşmak için rastgele bir açıya dön (90-270 derece arası ters yön)
        float randomTurn = Random.Range(120f, 240f);
        transform.Rotate(0, 0, randomTurn);
    }

    void RecordAndMoveSegments()
    {
        posHistory.Insert(0, transform.position);
        rotHistory.Insert(0, transform.rotation);

        if (posHistory.Count > (segmentCount + 1) * positionDelay)
        {
            posHistory.RemoveAt(posHistory.Count - 1);
            rotHistory.RemoveAt(rotHistory.Count - 1);
        }

        for (int i = 0; i < segments.Count; i++)
        {
            int index = Mathf.Min((i + 1) * positionDelay, posHistory.Count - 1);
            segments[i].transform.position = posHistory[index];
            segments[i].transform.rotation = rotHistory[index];
        }
    }

    void DestroySnake()
    {
        foreach (var s in segments) if (s) Destroy(s);
        Destroy(gameObject);
    }
}