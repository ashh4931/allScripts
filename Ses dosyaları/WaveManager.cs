using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

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

public class WaveManager : MonoBehaviour
{
    [Header("Wave Ayarları")]
    public int currentWave = 1;
    public int startingPoints = 10;
    public float pointsMultiplierPerWave = 1.3f;

    [Header("Süre Ayarları")]
    public float timeBeforeFirstWave = 5f;
    public float breakTimeBetweenWaves = 10f; 
    private float waveTimer;

    [Header("Oyun Durumu")]
    public bool isWaveActive = false;
    public int activeEnemyCount = 0; 

    [Header("Arayüz (UI)")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI waveInfoText;

    [Header("Müzik Sistemi")]
    public AudioSource musicSource;
    public List<AudioClip> waveMusicList;    
    public List<AudioClip> breakMusicList;   
    public List<AudioClip> eventMusicList;   
    public float fadeDuration = 1.5f;

    [Header("Spawner & Pool")]
    public List<FloorSpawner> spawners;
    public List<EnemySpawnData> enemyPool;

    private int currentWavePoints;
    private Vector3 originalScale;
    private List<EnemySpawnData> availableEnemies = new List<EnemySpawnData>(); 

    void Start()
    {
        if (timerText != null) originalScale = timerText.transform.localScale;
        waveTimer = timeBeforeFirstWave;
        StartCoroutine(TransitionMusic(breakMusicList));
    }

    void Update()
    {
        if (!isWaveActive && activeEnemyCount <= 0)
        {
            if (waveTimer > 0)
            {
                waveTimer -= Time.deltaTime;
                UpdateTimerUI();
                if (waveTimer <= 0) StartNextWave();
            }
        }

        // Test Spawn Tuşu (Z)
        if (Input.GetKeyDown(KeyCode.Z)) // InputManager hatası almamak için standart Input kullandım
        {
            StartCoroutine(SpawnTestEnemiesRoutine(150));
        }
    }

    public void StartNextWave()
    {
        isWaveActive = true;
        currentWavePoints = Mathf.RoundToInt(startingPoints * Mathf.Pow(pointsMultiplierPerWave, currentWave - 1));
        WaveType currentWaveType = (currentWave % 4 == 0) ? WaveType.WeaklingSwarm : WaveType.Normal;

        StartCoroutine(TransitionMusic(waveMusicList));
        StartCoroutine(GenerateAndSpawnWaveRoutine(currentWaveType));
    }

    IEnumerator GenerateAndSpawnWaveRoutine(WaveType waveType)
    {
        if (waveInfoText != null) waveInfoText.text = "Düşmanlar Saldırıyor!";
        List<EnemySpawnData> enemiesToSpawn = new List<EnemySpawnData>();
        int safetyNet = 0;

        while (currentWavePoints > 0 && safetyNet < 5000) 
        {
            safetyNet++;
            availableEnemies.Clear(); 
            foreach (var enemy in enemyPool)
            {
                if (currentWave >= enemy.minWaveToSpawn && currentWavePoints >= enemy.cost)
                {
                    if (waveType == WaveType.WeaklingSwarm && !enemy.isWeakEnemy) continue;
                    availableEnemies.Add(enemy);
                }
            }
            if (availableEnemies.Count == 0) break;
            EnemySpawnData selected = GetRandomEnemyWeighted(availableEnemies);
            currentWavePoints -= selected.cost;
            enemiesToSpawn.Add(selected);
        }

        foreach (var enemyData in enemiesToSpawn)
        {
            SpawnSingleEnemy(enemyData.prefab);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        }
    }

    // --- HATA 1 ÇÖZÜMÜ: TEST ROUTINE GERİ GELDİ ---
    IEnumerator SpawnTestEnemiesRoutine(int testPoints)
    {
        Debug.Log("Test sürüsü geliyor...");
        while (testPoints > 0)
        {
            EnemySpawnData selected = GetRandomEnemyWeighted(enemyPool);
            testPoints -= selected.cost;
            SpawnSingleEnemy(selected.prefab);
            yield return new WaitForSeconds(0.1f);
        }
    }

    // --- HATA 2 & 3 ÇÖZÜMÜ: SPAWN VE TAKİP MANTIĞI ---
    void SpawnSingleEnemy(GameObject prefab)
    {
        if (spawners.Count > 0)
        {
            FloorSpawner spawner = spawners[Random.Range(0, spawners.Count)];
            spawner.SpawnSpecificEnemy(prefab); 
            
            // Eğer EnemyHealth scriptin yoksa burayı yorum satırı yapabilirsin
            // Ancak wave'in bitmesi için sahadaki düşmanları bir şekilde saymalısın.
            activeEnemyCount++;
        }
    }

    // Düşman öldüğünde bu fonksiyonu Düşman Scriptinden çağırmalısın!
    public void EnemyDied()
    {
        activeEnemyCount--;
        if (activeEnemyCount <= 0 && isWaveActive)
        {
            isWaveActive = false;
            currentWave++;
            waveTimer = breakTimeBetweenWaves;
            if (waveInfoText != null) waveInfoText.text = "Dalga Temizlendi!";
            StartCoroutine(TransitionMusic(breakMusicList));
        }
    }

    IEnumerator TransitionMusic(List<AudioClip> targetList)
    {
        if (targetList == null || targetList.Count == 0 || musicSource == null) yield break;
        
        float startVolume = musicSource.volume;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = targetList[Random.Range(0, targetList.Count)];
        musicSource.Play();

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0, startVolume, t / fadeDuration);
            yield return null;
        }
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;
        float displayTime = Mathf.Max(waveTimer, 0f);
        timerText.text = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(displayTime / 60), Mathf.FloorToInt(displayTime % 60));
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
}