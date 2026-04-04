using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NewSnakeAI : NewBaseEnemyAI
{
    private float currentMoveSpeed; // 🔴 Hızı burada tutacağız, datayı bozmayacağız

    public float ghostFadeSpeed = 0.5f;       // Ne kadar süre kalır? (Yüksek sayı = Daha kısa süre)
    public int ghostSpawnInterval = 5;       // Ne kadar yoğun? (Yüksek sayı = Daha seyrek)
    [Header("Juice - AfterImage")]
    // Büyük yılan için 1.3f, küçük yılan için 1.0f veya 1.1f yapabilirsin.
    public float ghostScaleMultiplier = 1.3f;
    [Header("Minion Spawning (Yavrulama)")]
    public bool canSpawnMinions = false; // Ana yılan için işaretle, küçükler için kapat
    public GameObject[] minionPrefabs;    // Doğacak küçük yılan prefabları
    public float spawnInterval = 15f;    // Kaç saniyede bir doğuracak?
    private float spawnTimer;            // Zamanlayıcı
    private NewSnakeEnemyData snakeData;
    private List<Transform> segments = new List<Transform>();
    private List<Vector3> originalScales = new List<Vector3>();
    private List<Vector2> pathPoints = new List<Vector2>();
    private List<float> pathDistances = new List<float>();
    private float totalPathDistance = 0f;
    private Vector2 targetPosition;
    private enum SnakeState { Slithering, Striking, Retreating }
    private SnakeState currentState = SnakeState.Slithering;
    private float stateTimer;
    private float headDamageTimer;

    public NewSnakeEnemyData GetSnakeData() => snakeData;
    public float GetDamageValue() => baseData.baseDamage;

    // --- 🟢 HİTSTOP SİSTEMİ (ARTIK DOĞRU YERDE) ---
    private bool isHitStopping = false; // 🔴 Kilidimiz burada
    public void TriggerHitStop(float duration)
    {
        // Eğer zaten bir Hitstop çalışıyorsa veya obje aktif değilse yeni bir tane başlatma!
        if (isHitStopping || !gameObject.activeInHierarchy) return;

        StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        isHitStopping = true; // Kilidi kapat (Yeni Hitstop'ları engelle)

        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // Gerçek dünya zamanıyla bekle
        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f; // Zamanı kesin olarak normale döndür

        // 🔴 HİLE: Yeni bir Hitstop tetiklenmeden önce çok kısa bir süre bekle 
        // (Döngüye girmeyi engellemek için güvenlik tamponu)
        yield return new WaitForSecondsRealtime(0.1f);

        isHitStopping = false; // Kilidi aç
    }
    public void OnSegmentDestroyed(Transform segment)
    {
        int index = segments.IndexOf(segment);
        if (index != -1) { segments.RemoveAt(index); originalScales.RemoveAt(index); }
        baseData.moveSpeed *= snakeData.enrageSpeedMultiplier;
    }

    protected override void Start()
    {
        spawnTimer = spawnInterval;
        base.Start();

        // Sadece kopyayı al ve onu kullan
        snakeData = Instantiate(baseData as NewSnakeEnemyData);

        // 🔴 Orijinal hızı yerel değişkene aktar
        currentMoveSpeed = snakeData.moveSpeed;

        pathPoints.Add(transform.position);
        pathDistances.Add(0f);
        SpawnSnake();
    }
    void SpawnSnake()
    {
        for (int i = 0; i < snakeData.segmentCount; i++)
        {
            GameObject prefab = (i == snakeData.segmentCount - 1) ? snakeData.tailPrefab : snakeData.bodyPrefabs[Random.Range(0, snakeData.bodyPrefabs.Length)];
            GameObject segmentObj = Instantiate(prefab, transform.position, Quaternion.identity);
            SnakeSegment segScript = segmentObj.AddComponent<SnakeSegment>();
            segScript.Setup(this, baseData.maxHealth * snakeData.segmentHealthMultiplier);
            segments.Add(segmentObj.transform);
            originalScales.Add(segmentObj.transform.localScale);
        }
    }
    void SpawnRandomMinion()
    {
        if (minionPrefabs == null || minionPrefabs.Length == 0) return;

        // Listeden rastgele bir küçük yılan seç
        int randomIndex = Random.Range(0, minionPrefabs.Length);
        GameObject selectedMinion = minionPrefabs[randomIndex];

        // Yavrunun doğacağı yer: Yılanın son boğumu (Kuyruk) olsun ki içinden çıkıyormuş gibi dursun
        Vector3 spawnPos = transform.position;
        if (segments.Count > 0 && segments[segments.Count - 1] != null)
        {
            spawnPos = segments[segments.Count - 1].position;
        }

        // Yavruyu oluştur
        GameObject minion = Instantiate(selectedMinion, spawnPos, Quaternion.identity);

        // JUICE: Doğarken minik bir patlama efekti (opsiyonel)
        if (snakeData.explosionEffect != null)
        {
            Instantiate(snakeData.explosionEffect, spawnPos, Quaternion.identity);
        }

        // Küçük yılanın hızını bir anlığına artırmak istersen buraya ekleme yapabilirsin.
    }
    protected override void Update()
    {
        // Yavrulama mantığı
        if (canSpawnMinions && !isDead && !isFrozen)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0)
            {
                SpawnRandomMinion();
                spawnTimer = spawnInterval;
            }
        }
        if (isDead || isFrozen || player == null) return;
        base.Update();
        float distSinceLastPoint = Vector2.Distance(transform.position, pathPoints[0]);
        if (distSinceLastPoint > 0.05f)
        {
            pathPoints.Insert(0, transform.position);
            totalPathDistance += distSinceLastPoint;
            pathDistances.Insert(0, totalPathDistance);
            float maxRequiredDist = (segments.Count + 2) * snakeData.segmentGap + 5f;
            if (pathPoints.Count > 300 && pathDistances[pathDistances.Count - 1] < totalPathDistance - maxRequiredDist)
            {
                pathPoints.RemoveAt(pathPoints.Count - 1);
                pathDistances.RemoveAt(pathDistances.Count - 1);
            }
        }
        MoveSegments();
        if (rb.linearVelocity.magnitude > 0.5f)
        {
            // 🔴 YOĞUNLUK AYARI: Artık 'ghostSpawnInterval' kullanılıyor
            if (Time.frameCount % ghostSpawnInterval == 0)
            {
                CreateGhost();
            }
        }
    }
    void CreateGhost()
    {
        GameObject ghost = new GameObject("Ghost");
        ghost.AddComponent<AfterImage>().Init(
            spriteRenderer.sprite,
            transform.position,
            transform.rotation,
            transform.localScale,
            spriteRenderer.sortingOrder,
            ghostScaleMultiplier, // Boyut
            ghostFadeSpeed        // Süre (Kalıcılık)
        );
    }
    void MoveSegments()
    {
        if (segments.Count == 0) return;
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] == null) continue;
            float targetDistFromHead = (i + 1) * snakeData.segmentGap;
            float absoluteTargetDist = totalPathDistance - targetDistFromHead;
            Vector2 targetPos = GetPointAtDistance(absoluteTargetDist);
            segments[i].position = Vector2.Lerp(segments[i].position, targetPos, Time.deltaTime * 35f);
            float pulse = 1f + Mathf.Sin(Time.time * snakeData.pulseSpeed + i * 0.5f) * snakeData.pulseAmount;
            segments[i].localScale = originalScales[i] * pulse;
            Vector2 leadingPos = (i == 0) ? (Vector2)transform.position : (Vector2)segments[i - 1].position;
            Vector2 dir = leadingPos - (Vector2)segments[i].position;
            if (dir.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                segments[i].rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }
    }

    Vector2 GetPointAtDistance(float targetDistance)
    {
        if (pathDistances.Count < 2) return transform.position;
        if (targetDistance <= pathDistances[pathDistances.Count - 1]) return pathPoints[pathPoints.Count - 1];
        for (int i = 0; i < pathDistances.Count - 1; i++)
        {
            if (targetDistance <= pathDistances[i] && targetDistance >= pathDistances[i + 1])
            {
                float t = (targetDistance - pathDistances[i + 1]) / (pathDistances[i] - pathDistances[i + 1]);
                return Vector2.Lerp(pathPoints[i + 1], pathPoints[i], t);
            }
        }
        return pathPoints[pathPoints.Count - 1];
    }

    protected override void HandleMovement()
    {
        stateTimer -= Time.deltaTime;

        // 🔴 Hızı artık yerel değişkenden alıyoruz
        float speed = currentMoveSpeed;
        switch (currentState)
        {
            case SnakeState.Slithering:
                Vector2 sine = transform.up * Mathf.Sin(Time.time * 4f) * 1.5f;
                MoveTowardsTarget((Vector2)player.position + sine, speed);
                if (Vector2.Distance(transform.position, player.position) < 7f)
                {
                    currentState = SnakeState.Striking;
                    Vector2 dashDir = (player.position - transform.position).normalized;
                    targetPosition = (Vector2)player.position + (dashDir * snakeData.strikeOvershoot);
                    stateTimer = 2.5f;
                }
                break;
            case SnakeState.Striking:
                MoveTowardsTarget(targetPosition, speed * 2.5f);

                if (Vector2.Distance(transform.position, targetPosition) < 1f || stateTimer <= 0)
                {
                    currentState = SnakeState.Retreating;
                    stateTimer = snakeData.idleWaitTime;
                }
                break;
            case SnakeState.Retreating:
                Vector2 escapeDir = (transform.position - player.position).normalized;
                MoveTowardsTarget((Vector2)transform.position + escapeDir, speed * 1.2f);
                if (stateTimer <= 0) currentState = SnakeState.Slithering;
                break;
        }
    }

    void MoveTowardsTarget(Vector2 target, float speed)
    {
        Vector2 dirToTarget = (target - (Vector2)transform.position).normalized;
        if (dirToTarget.sqrMagnitude > 0.0001f)
        {
            float targetAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, snakeData.turnSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        rb.linearVelocity = transform.right * speed;
    }

    public override void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. Yardımcıyı oluştur
        GameObject proxy = new GameObject("SnakeDeathProxy");
        proxy.transform.position = transform.position;
        SnakeDeathProxy deathScript = proxy.AddComponent<SnakeDeathProxy>();

        // 2. Patlamaları ve Zaman Durdurmayı (Hitstop) Proxy'ye devret
        deathScript.StartChainReaction(segments, snakeData);

        // 3. Kafayı hemen yok et
        Destroy(gameObject);
    }
}

// --- 🛠️ YARDIMCI SINIF (Proxy'nin içi artık temiz) ---
public class SnakeDeathProxy : MonoBehaviour
{
    public void StartChainReaction(List<Transform> segments, NewSnakeEnemyData data)
    {
        StartCoroutine(ExecuteExplosions(segments, data));
    }

    IEnumerator ExecuteExplosions(List<Transform> segments, NewSnakeEnemyData data)
    {
        // 🔴 1. ZAMANI DURDUR (Hitstop Başlat)
        Time.timeScale = 0f;

        float currentPitch = 0.8f;

        // İlk patlama (Kafa)
        SpawnExplosion(transform.position, data, currentPitch);

        // 🔴 2. GERÇEK ZAMANLA BEKLE (0.15 saniye donma hissi)
        // WaitForSeconds yerine WaitForSecondsRealtime kullanıyoruz!
        yield return new WaitForSecondsRealtime(0.15f);

        // 🔴 3. ZAMANI GERİ BAŞLAT
        Time.timeScale = 1f;

        // Diğer boğumların patlaması
        foreach (var seg in segments)
        {
            if (seg != null)
            {
                // Burada da Realtime kullanmalısın, yoksa aradaki boşluklar garip gelebilir
                yield return new WaitForSecondsRealtime(0.1f);
                currentPitch += 0.08f;
                SpawnExplosion(seg.position, data, currentPitch);
                Destroy(seg.gameObject);
            }
        }

        Destroy(gameObject);
    }

    void SpawnExplosion(Vector3 pos, NewSnakeEnemyData data, float pitch)
    {
        if (data.explosionEffect != null)
            Instantiate(data.explosionEffect, pos, Quaternion.identity);

        if (data.explosionSound != null && AudioManager.instance != null)
            AudioManager.instance.PlaySFXAtPosition(data.explosionSound, pos, 1f, pitch);
    }

}