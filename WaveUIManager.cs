using UnityEngine;
using TMPro;

public class WaveUIManager : MonoBehaviour
{
    [Header("Yazı Referansları")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI enemyCountText;

    private WaveManager _waveManager;

    private void Awake()
    {
        _waveManager = FindObjectOfType<WaveManager>(); // Referansı bir kez al
    }

    private void OnEnable()
    {
        // İsimlendirilmiş metotlarla abone ol
        WaveEvents.OnWaveStarted += HandleWaveStartedUI;
        WaveEvents.OnBreakTimerTick += UpdateTimerDisplay;
        WaveEvents.OnEnemyCountChanged += UpdateEnemyDisplay;
        WaveEvents.OnWaveFinished += HandleWaveFinished;


    }

    private void OnDisable()
    {
        // Metot ismini OnEnable'daki ile aynı yapmalısın
        WaveEvents.OnWaveStarted -= HandleWaveStartedUI;
        WaveEvents.OnBreakTimerTick -= UpdateTimerDisplay;
        WaveEvents.OnEnemyCountChanged -= UpdateEnemyDisplay;
        WaveEvents.OnWaveFinished -= HandleWaveFinished;
    }

    // Parametre imzası WaveEvents.OnWaveStarted (int, WaveType) ile eşleşmeli
    private void HandleWaveStartedUI(int index, int points, WaveType type)
    {
        if (waveText != null) waveText.text = index.ToString(); // Sadece sayı görünür
    }

    private void UpdateTimerDisplay(float time)
    {
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(time).ToString() + "s";
    }

    private void UpdateEnemyDisplay(int remaining, int total)
    {
        if (enemyCountText != null)
            enemyCountText.text = remaining.ToString(); // Sadece sayı görünür
    }

    private void HandleWaveFinished()
    {
        if (enemyCountText != null) enemyCountText.text = "Clear!";
        if (timerText != null) timerText.gameObject.SetActive(true);
    }
}