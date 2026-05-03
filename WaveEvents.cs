using System;
using UnityEngine;
public static class WaveEvents
{
    // İlk parametreye 'int waveIndex' ekledik
    public static Action<int, int, WaveType> OnWaveStarted;      
    public static Action OnWaveFinished;           
    public static Action<float> OnBreakTimerTick; 
    public static Action<int, int> OnEnemyCountChanged; 
}
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