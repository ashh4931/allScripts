using UnityEngine;
using System.Collections.Generic;

public enum WaveType { Normal, Swarm, Elite, Boss }
public enum WaveState { STARTUP, BREAK, SPAWNING, IN_PROGRESS }

public class WaveManager : MonoBehaviour
{
    [Header("Wave Ayarları")]
    public int currentWave = 0;
    public int basePoints = 10;
    public float multiplier = 1.2f;

    [Header("Süre Ayarları")]
    public float standardBreakTime = 10f;
    [Tooltip("İlk dalgalar için özel süreler. 1. dalga bitince kaç sn beklenecek?")]
    public List<float> customBreakTimes = new List<float>() { 3f, 3f, 5f, 5f, 7f };

    [Header("Bitirme Eşiği")]
    [Range(0f, 1f)] public float waveFinishThreshold = 0.8f; // %80'i ölünce
    [Tooltip("Sahnede bu sayıya eşit veya daha az düşman kalırsa dalga biter.")]
    public int waveFinishEnemyCount = 3; // VEYA sahnede 3 düşman kalırsa

    [Header("Durum İzleyici")]
    public WaveState currentState = WaveState.STARTUP;
    private float timer;

    public void BeginWaveSystem()
    {
       // Debug.Log("<color=cyan>[WaveManager] Dalga sistemi manuel olarak başlatıldı!</color>");
        StartBreak();
    }

    void Update()
    {
        if (currentState == WaveState.BREAK)
        {
            timer -= Time.deltaTime;
            WaveEvents.OnBreakTimerTick?.Invoke(timer);

            if (timer <= 0)
            {
                StartWave();
            }
        }
    }

    public void StartBreak()
    {
        currentState = WaveState.BREAK;

        if (currentWave < customBreakTimes.Count)
        {
            timer = customBreakTimes[currentWave];
           // Debug.Log($"<color=white>[WaveManager] Özel dinlenme süresi kullanılıyor: {timer}s</color>");
        }
        else
        {
            timer = standardBreakTime;
        }

        WaveEvents.OnWaveFinished?.Invoke();
    }

    private void StartWave()
    {
        currentWave++;
        currentState = WaveState.IN_PROGRESS;

        WaveType currentType = WaveType.Normal;
        if (currentWave % 10 == 0) currentType = WaveType.Boss;
        else if (currentWave % 5 == 0) currentType = WaveType.Elite;
        else if (currentWave > 3 && Random.value > 0.7f) currentType = WaveType.Swarm;

        int wavePoints = Mathf.RoundToInt(basePoints * Mathf.Pow(multiplier, currentWave - 1));
        Debug.Log($"<color=orange>[WaveManager] DALGA {currentWave} ({currentType}) BAŞLADI!</color>");

       WaveEvents.OnWaveStarted?.Invoke(currentWave, wavePoints, currentType);
    }

    public void AllEnemiesDefeated()
    {
        if (currentState == WaveState.IN_PROGRESS)
        {
            StartBreak();
        }
    }
}