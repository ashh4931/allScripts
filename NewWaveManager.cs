using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
/*
[System.Serializable]
public class EnemySpawnData
{
    public string enemyName;
    public GameObject prefab;
    public int cost = 1;
    public float spawnWeight = 50f;
    public int minWaveToSpawn = 1;
    public bool isWeakEnemy;
}

public enum WaveType { Normal, WeaklingSwarm, EliteFocus }

public class NewWaveManager : MonoBehaviour
{
    public List<GameObject> spawnedEnemies = new List<GameObject>();
    private bool isSpawningEnemies = false;

    [Header("Debug Ayarları")]
    public bool debugMode = false;

    [Header("Wave Ayarları")]
    public int currentWave = 1;
    public int startingPoints = 10;
    public float pointsMultiplierPerWave = 1.3f;
    public List<float> customBreakTimes;
    [Range(0f, 1f)] public float waveFinishThreshold = 0.8f;

    [Header("Süre Ayarları")]
    public float timeBeforeFirstWave = 5f;
    public float breakTimeBetweenWaves = 10f;
    private float waveTimer;

    [Header("Spawn Zamanlama Ayarları")]
    public float minSpawnDelay = 0.5f;
    public float maxSpawnDelay = 1.5f;

    [Header("Oyun Durumu")]
    public bool isWaveActive = false;
    public int activeEnemyCount = 0;
    private int totalEnemiesInCurrentWave = 0;

    [Header("Arayüz (UI)")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI waveInfoText;

    [Header("Spawner & Pool")]
    public List<FloorSpawner> spawners;
    public List<EnemySpawnData> enemyPool;

    [Header("Kule Sistemi (2D)")]
    public GameObject towerPrefab;
    public BoxCollider2D spawnArea;
    public LayerMask towerLayer;
    private List<TowerController> activeTowers = new List<TowerController>();
    private List<EnemySpawnData> availableEnemies = new List<EnemySpawnData>();

    public void BeginWaveSystem()
    {
        Debug.Log("<color=cyan><b>[WaveManager]</b> Dalga sistemi başlatılıyor...</color>");
        
        if (debugMode)
        {
            SpawnDebugTower();
            if (waveInfoText != null) waveInfoText.text = "Debug Modu Aktif";
        }
        else
        {
            waveTimer = timeBeforeFirstWave;
        }

        if (MusicManager.instance != null) MusicManager.instance.PlayBreakMusic();

        StopAllCoroutines(); // Çakışmaları önlemek için
        StartCoroutine(CheckEnemyStatusRoutine());
    }

    void Update()
    {
     //Debug.Log("update çalışıyor");
       
        if (debugMode)
        {
            if (Input.GetKeyDown(KeyCode.X)) SpawnTowers();
            return;
        }

        // Dalga arası geri sayım
        if (!isWaveActive && !isSpawningEnemies)
        {
            if (waveTimer > 0)
            {
                waveTimer -= Time.deltaTime;
                UpdateTimerUI();
                if (waveTimer <= 0) StartNextWave();
            }
        }

        // Test tuşları
        if (Input.GetKeyDown(KeyCode.Z)) StartCoroutine(SpawnTestEnemiesRoutine(150));
        if (Input.GetKeyDown(KeyCode.X)) SpawnTowers();
    }

    public void StartNextWave()
    {
        Debug.Log($"<color=orange><b>[WaveManager]</b> Dalga {currentWave} başlatılıyor!</color>");
        
        isWaveActive = true;
        totalEnemiesInCurrentWave = 0;
        spawnedEnemies.Clear();

        // Puan hesaplama
        int basePoints = Mathf.RoundToInt(startingPoints * Mathf.Pow(pointsMultiplierPerWave, currentWave - 1));
        activeTowers.RemoveAll(t => t == null);
        int currentWavePoints = basePoints + Mathf.RoundToInt(activeTowers.Count * (basePoints / 3f));

        Debug.Log($"<b>[WaveManager]</b> Dalga Puanı: {currentWavePoints} (Baz: {basePoints}, Kule Bonus: {activeTowers.Count})");

        WaveType currentWaveType = (currentWave % 4 == 0) ? WaveType.WeaklingSwarm : WaveType.Normal;

        if (waveInfoText != null) waveInfoText.text = "Dalga " + currentWave + " Hazırlanıyor...";
        if (MusicManager.instance != null) MusicManager.instance.PlayWaveMusic();
        
        StartCoroutine(GenerateAndSpawnWaveRoutine(currentWaveType, currentWavePoints));
    }

    IEnumerator GenerateAndSpawnWaveRoutine(WaveType waveType, int totalPoints)
    {
        isSpawningEnemies = true;
        Debug.Log($"<color=yellow><b>[WaveManager]</b> Düşmanlar oluşturuluyor... Tip: {waveType}</color>");

        try
        {
            int remainingPoints = totalPoints;
            List<EnemySpawnData> enemiesToSpawn = new List<EnemySpawnData>();

            while (remainingPoints > 0)
            {
                availableEnemies.Clear();
                foreach (var enemy in enemyPool)
                {
                    if (currentWave >= enemy.minWaveToSpawn && remainingPoints >= enemy.cost)
                    {
                        if (waveType == WaveType.WeaklingSwarm && !enemy.isWeakEnemy) continue;
                        availableEnemies.Add(enemy);
                    }
                }

                if (availableEnemies.Count == 0) break;
                
                EnemySpawnData selected = GetRandomEnemyWeighted(availableEnemies);
                remainingPoints -= selected.cost;
                enemiesToSpawn.Add(selected);
            }

            if (enemiesToSpawn.Count == 0)
            {
                Debug.LogError("<b>[WaveManager]</b> KRİTİK: Hiç düşman üretilemedi! EnemyPool ayarlarını kontrol edin.");
                isSpawningEnemies = false;
                FinishWave();
                yield break;
            }

            totalEnemiesInCurrentWave = enemiesToSpawn.Count;
            Debug.Log($"<b>[WaveManager]</b> Toplam {totalEnemiesInCurrentWave} düşman spawn edilecek.");

            foreach (var enemyData in enemiesToSpawn)
            {
                SpawnSingleEnemy(enemyData.prefab);
                yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));
            }
        }
        finally
        {
            isSpawningEnemies = false;
            Debug.Log("<color=green><b>[WaveManager]</b> Tüm düşmanlar sahaya sürüldü.</color>");
        }
    }

    void SpawnSingleEnemy(GameObject prefab)
    {
        if (spawners != null && spawners.Count > 0)
        {
            FloorSpawner spawner = spawners[Random.Range(0, spawners.Count)];
            GameObject enemy = spawner.SpawnSpecificEnemy(prefab);
            if (enemy != null)
            {
                spawnedEnemies.Add(enemy);
                activeEnemyCount = spawnedEnemies.Count;
            }
        }
        else
        {
            Debug.LogError("<b>[WaveManager]</b> HATA: Sahnede hiç Spawner atanmamış veya liste boş!");
        }
    }

    IEnumerator CheckEnemyStatusRoutine()
    {
        Debug.Log("<color=green><b>[WaveManager]</b> Düşman takip sistemi aktif.</color>");

        while (true)
        {
            yield return new WaitForSecondsRealtime(0.5f);

            if (spawnedEnemies == null) continue;

            // Listeyi temizle
            spawnedEnemies.RemoveAll(enemy => enemy == null || !enemy.activeInHierarchy);
            activeEnemyCount = spawnedEnemies.Count;

            if (isWaveActive && !isSpawningEnemies)
            {
                if (totalEnemiesInCurrentWave > 0)
                {
                    int killedCount = totalEnemiesInCurrentWave - activeEnemyCount;
                    float killProgress = (float)killedCount / totalEnemiesInCurrentWave;

                    // UI Güncelleme (Ayrıntılı)
                    if (waveInfoText != null)
                    {
                        waveInfoText.text = $"Dalga {currentWave}: {killedCount}/{totalEnemiesInCurrentWave} Elendi (%{Mathf.RoundToInt(killProgress * 100)})";
                    }

                    // Dalga bitiş kontrolü
                    if (killProgress >= waveFinishThreshold)
                    {
                        Debug.Log($"<color=red><b>[WaveManager]</b> Bitiş Eşiği Geçildi!</color> İlerleme: %{killProgress * 100} | Kalan Düşman: {activeEnemyCount}");
                        FinishWave();
                    }
                }
                else
                {
                    // Güvenlik: Eğer dalga aktif ama düşman yoksa (spawn hatası vb.)
                    Debug.LogWarning("<b>[WaveManager]</b> Dalga aktif ancak toplam düşman sayısı 0! Dalga sonlandırılıyor.");
                    FinishWave();
                }
            }
        }
    }

    void FinishWave()
    {
        if (!isWaveActive) return;

        Debug.Log($"<color=white><b>[WaveManager]</b> Dalga {currentWave} başarıyla tamamlandı!</color>");
        
        isWaveActive = false;
        spawnedEnemies.Clear();
        activeEnemyCount = 0;
        totalEnemiesInCurrentWave = 0;

        int finishedWaveIndex = currentWave - 1;

        if (customBreakTimes != null && finishedWaveIndex < customBreakTimes.Count)
        {
            waveTimer = customBreakTimes[finishedWaveIndex];
        }
        else
        {
            waveTimer = breakTimeBetweenWaves;
        }

        currentWave++;
        if (waveInfoText != null) waveInfoText.text = "Dalga Temizlendi! Yeni dalga bekleniyor...";

        SpawnTowers();
        if (MusicManager.instance != null) MusicManager.instance.PlayBreakMusic();
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;
        timerText.text = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(waveTimer / 60), Mathf.FloorToInt(waveTimer % 60));
    }

    EnemySpawnData GetRandomEnemyWeighted(List<EnemySpawnData> pool)
    {
        float totalWeight = 0;
        foreach (var e in pool) totalWeight += e.spawnWeight;
        float randomValue = Random.Range(0, totalWeight);
        float currentSum = 0;
        foreach (var e in pool)
        {
            currentSum += e.spawnWeight;
            if (randomValue <= currentSum) return e;
        }
        return pool[0];
    }

    public void SpawnTowers()
    {
        activeTowers.RemoveAll(t => t == null);
        if (activeTowers.Count >= 5) return;
        if (towerPrefab == null || spawnArea == null) return;

        int remainingSlot = 5 - activeTowers.Count;
        int randomCount = Random.Range(1, 6);
        int countToSpawn = Mathf.Min(randomCount, remainingSlot);

        int successCount = 0;

        for (int i = 0; i < countToSpawn; i++)
        {
            Vector2 randomPos = Vector2.zero;
            bool foundValidPos = false;
            int maxAttempts = 20;
            int currentAttempt = 0;

            while (!foundValidPos && currentAttempt < maxAttempts)
            {
                randomPos = new Vector2(
                    Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x),
                    Random.Range(spawnArea.bounds.min.y, spawnArea.bounds.max.y)
                );

                LayerMask mask = LayerMask.GetMask("Tower");
                Collider2D hit = Physics2D.OverlapCircle(randomPos, 2.5f, mask);

                if (hit == null) foundValidPos = true;
                currentAttempt++;
            }

            if (foundValidPos)
            {
                GameObject towerObj = Instantiate(towerPrefab, randomPos, Quaternion.identity);
                TowerController controller = towerObj.GetComponent<TowerController>();

                if (controller != null)
                {
                    controller.SetupTower();
                    activeTowers.Add(controller);
                    successCount++;
                }
            }
        }

        if (successCount > 0 && HintManager.Instance != null)
        {
            HintManager.Instance.ShowHint("tower_info", "Kadim kuleler ortaya çıktı!", "KULE UYARISI", true, 5f);
        }
    }

    IEnumerator TransitionMusic(List<AudioClip> target) { yield return null; }
    void SpawnDebugTower() { Debug.Log("<b>[Debug]</b> Kule spawn edildi."); }
    IEnumerator SpawnTestEnemiesRoutine(int pts) { yield return null; }
}*/