using UnityEngine;
using System.Collections.Generic;

public class EnemyTracker : MonoBehaviour
{
    public List<GameObject> activeEnemies = new List<GameObject>();
    private int totalSpawnedThisWave;
    private WaveManager _wm;
    private EnemySpawner _spawner;

    void Awake()
    {
        _wm = FindObjectOfType<WaveManager>();
        _spawner = FindObjectOfType<EnemySpawner>();
    }

    private void OnEnable()
    {
        // YENİ: Dalga başladığında sayaçları temizlemek için abone ol
        WaveEvents.OnWaveStarted += ResetTrackerForNewWave;
    }

    private void OnDisable()
    {
        WaveEvents.OnWaveStarted -= ResetTrackerForNewWave;
    }

    void ResetTrackerForNewWave(int i, int p, WaveType t)
    {
        activeEnemies.Clear();
        totalSpawnedThisWave = 0;
        UpdateUI();
    }

    void Start()
    {
        // Daha hızlı kontrol için süreyi düşürdük (0.2sn)
        InvokeRepeating(nameof(CleanList), 0.2f, 0.2f);
    }

    public void RegisterEnemy(GameObject enemy)
    {
        if (enemy == null) return;
        activeEnemies.Add(enemy);
        totalSpawnedThisWave++;
        UpdateUI();
    }

    void CleanList()
    {
        activeEnemies.RemoveAll(e => e == null || !e.activeInHierarchy);
        UpdateUI();
        CheckWaveStatus();
    }

    // EnemyTracker.cs içindeki CheckWaveStatus metodunu bu şekilde değiştir:
    // EnemyTracker.cs içindeki CheckWaveStatus metodunu bu şekilde değiştir:

void CheckWaveStatus()
{
    // Spawner işini bitirdiyse kontrol et
    if (_spawner.isSpawningFinished)
    {
        // YENİ GÜVENLİK: Eğer spawner hiçbir düşman doğuramadıysa dalgayı kilitli bırakma, direkt bitir!
        if (totalSpawnedThisWave == 0)
        {
            Debug.LogWarning("[EnemyTracker] Bu dalgada hiç düşman doğamadı, sistem kilitlenmemesi için dalga geçiliyor.");
            FinishWave();
            return;
        }

        // Ölen düşman sayısını hesapla
        int defeatedEnemies = totalSpawnedThisWave - activeEnemies.Count;
        
        // Ölenlerin toplama oranını bul (Örn: 8/10 = 0.8)
        float progress = (float)defeatedEnemies / totalSpawnedThisWave;

        // Eğer ilerleme, WaveManager'daki eşik değerine ulaştıysa dalgayı bitir
        if (progress >= _wm.waveFinishThreshold)
        {
            FinishWave();
        }
    }
}

    void FinishWave()
    {
        Debug.Log("<color=green>[EnemyTracker] Dalga Tamamlandı.</color>");
        _wm.AllEnemiesDefeated();
        totalSpawnedThisWave = 0; // Bir sonraki kontrol için temizle
    }

    private void UpdateUI()
    {
        WaveEvents.OnEnemyCountChanged?.Invoke(activeEnemies.Count, totalSpawnedThisWave);
    }
}