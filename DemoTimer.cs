using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DemoTimer : MonoBehaviour
{
    public float remainingTime = 900f;
    public TextMeshProUGUI timerText;
    
    [Header("Son 30 Saniye Ayarları")]
    public TextMeshProUGUI lastSecondsText; // Inspector'dan "Son 30 Saniye" yazısını buraya sürükle
    
    public UIManager uiManager; 
    public float warnTime = 885;
    private bool isDemoFinished = false;
    private bool warningTriggered = false;
    private bool lastSecondsTriggered = false; // 🔴 Yeni kontrol değişkeni

    void Start()
    {
        // Oyun başında uyarı yazısını gizle
        if (lastSecondsText != null) lastSecondsText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isDemoFinished) return;

        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            UpdateTimerUI();

            // --- 3 DAKİKA (WARN TIME) KONTROLÜ ---
            if (remainingTime <= warnTime && !warningTriggered)
            {
                warningTriggered = true;
                uiManager.ShowDemoWarning();
            }

            // --- 🔴 SON 30 SANİYE KONTROLÜ ---
            if (remainingTime <= 30f && !lastSecondsTriggered)
            {
                lastSecondsTriggered = true;
                if (lastSecondsText != null)
                {
                    lastSecondsText.gameObject.SetActive(true);
                    lastSecondsText.text = "DEMO BITIYOR! SON 30 SANIYE!";
                }
            }
        }
        else
        {
            remainingTime = 0;
            FinishDemo();
        }
    }

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void FinishDemo()
    {
        isDemoFinished = true;
        PlayerPrefs.SetInt("DemoFinished", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainMenu");
    }
}