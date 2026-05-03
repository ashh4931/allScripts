using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{// ... diğer değişkenlerin olduğu yer ...
    private int _currentWaveIndex;
    public bool isSpawningFinished { get; private set; }
    public int minBossPoint = 80;
    private EnemyTracker _enemyTracker; // BU SATIRI EKLE
    [Header("Düşman Havuzu")]
    public List<EnemySpawnData> enemyPool;

    [Header("Spawn Alanları")]
    public BoxCollider2D mainSpawnArea;    // Genel alan
    public BoxCollider2D initialSpawnArea; // İlk dalgalar için özel alan
    public int initialWaveCount = 4;       // Kaçıncı dalgaya kadar özel alan kullanılacak?




    private void Awake()
    {
        _enemyTracker = FindObjectOfType<EnemyTracker>();
    }
    private void OnEnable()
    {
        WaveEvents.OnWaveStarted += HandleWaveStarted;
    }
    private void OnDisable()
    {
        WaveEvents.OnWaveStarted -= HandleWaveStarted;
    }

    private void HandleWaveStarted(int waveIndex, int totalPoints, WaveType type)
    {
        _currentWaveIndex = waveIndex;
        isSpawningFinished = false; // YENİ: Coroutine başlamadan hemen önce false yap
        StopAllCoroutines();
        StartCoroutine(SpawnRoutine(totalPoints, type));
    }

    IEnumerator SpawnRoutine(int points, WaveType type)
    {
        isSpawningFinished = false;
        int remainingPoints = points;

        // Güvenlik: Puan çok azsa veya liste boşsa direkt bitir
        if (remainingPoints <= 0 || enemyPool.Count == 0)
        {
            isSpawningFinished = true;
            yield break;
        }

        while (remainingPoints > 0)
        {
            EnemySpawnData data = GetRandomEnemyWeighted(remainingPoints, type);

            if (data != null && data.prefab != null) // Prefab kontrolü ekle
            {
                SpawnEnemy(data.prefab);
                remainingPoints -= data.cost;
            }
            else
            {
                Debug.LogWarning("[EnemySpawner] Uygun düşman bulunamadı veya prefab eksik!");
                break;
            }

            yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
        }

        isSpawningFinished = true;
        Debug.Log("<color=yellow>[EnemySpawner] Doğum işlemi tamamlandı.</color>");
    }
    void SpawnEnemy(GameObject prefab)
    {
        Vector3 spawnPos;

        // Hangi alanı kullanacağımıza karar veriyoruz
        BoxCollider2D selectedArea = mainSpawnArea;

        if (initialSpawnArea != null && _currentWaveIndex <= initialWaveCount)
        {
            selectedArea = initialSpawnArea;
        }

        if (selectedArea != null)
        {
            spawnPos = new Vector2(
                Random.Range(selectedArea.bounds.min.x, selectedArea.bounds.max.x),
                Random.Range(selectedArea.bounds.min.y, selectedArea.bounds.max.y)
            );
        }
        else return;

        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        _enemyTracker?.RegisterEnemy(enemy);
    }

    EnemySpawnData GetRandomEnemyWeighted(int maxCost, WaveType type)
    {
        float totalWeight = 0;
        List<EnemySpawnData> possibleEnemies = new List<EnemySpawnData>();

        // 1. ADIM: Hem maliyeti, hem tipi, HEM DE mevcut dalga numarasını kontrol et
        foreach (var e in enemyPool)
        {
            if (e.prefab == null) continue;
            // Dalga kontrolü burada devreye giriyor
            bool waveMatch = _currentWaveIndex >= e.minWaveToSpawn;
            bool costMatch = e.cost <= maxCost;
            bool typeMatch = false;

            switch (type)
            {
                case WaveType.Swarm: typeMatch = e.isWeakEnemy; break;
                case WaveType.Elite: typeMatch = !e.isWeakEnemy; break;
                case WaveType.Boss: typeMatch = e.cost >= minBossPoint; break;
                default: typeMatch = true; break;
            }

            if (waveMatch && costMatch && typeMatch)
            {
                possibleEnemies.Add(e);
                totalWeight += e.spawnWeight;
            }
        }

        // Eğer kriterlere uyan düşman bulunamadıysa (örneğin puan çok az ama dalga yüksek)
        // En azından maliyeti yeten ilk düşmanı döndür ki oyun kilitlenmesinden
        if (possibleEnemies.Count == 0)
        {
            return enemyPool.Find(e => e.cost <= maxCost);
        }

        // 2. ADIM: Ağırlıklı seçim yap
        float randomValue = Random.Range(0, totalWeight);
        float currentSum = 0;

        foreach (var e in possibleEnemies)
        {
            currentSum += e.spawnWeight;
            if (randomValue <= currentSum) return e;
        }

        return possibleEnemies[0];
    }
}