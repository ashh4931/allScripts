using UnityEngine;

public class SettingsDebugger : MonoBehaviour
{
    private int lastWidth;
    private int lastHeight;
    private int lastQualityLevel;

    void Start()
    {
        // Başlangıç değerlerini kaydet ve ilk durumu yazdır
        CaptureCurrentSettings();
        LogCurrentStatus("Oyun Başlatıldı");
    }

    void Update()
    {
        // Her frame'de değişiklik olup olmadığını kontrol et
        if (Screen.width != lastWidth || Screen.height != lastHeight || QualitySettings.GetQualityLevel() != lastQualityLevel)
        {
            LogCurrentStatus("AYARLAR DEĞİŞTİ!");
            CaptureCurrentSettings();
        }

        // Manuel kontrol için 'Space' tuşuna basabilirsin
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LogCurrentStatus("Manuel Kontrol");
        }
    }

    void CaptureCurrentSettings()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;
        lastQualityLevel = QualitySettings.GetQualityLevel();
    }

    void LogCurrentStatus(string triggerReason)
    {
        string qualityName = QualitySettings.names[QualitySettings.GetQualityLevel()];
        int currentLevel = QualitySettings.GetQualityLevel();
        
        string logMessage = $"<b>[{triggerReason}]</b>\n" +
                            $"Çözünürlük: <color=yellow>{Screen.width}x{Screen.height}</color>\n" +
                            $"Kalite Seviyesi: <color=cyan>{qualityName} (Index: {currentLevel})</color>\n" +
                            $"Tam Ekran: {Screen.fullScreenMode}";

        Debug.Log(logMessage);
    }
}