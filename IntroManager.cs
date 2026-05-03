using UnityEngine;
using TMPro;
using System.Collections;

public class GameIntroSequence : MonoBehaviour 
{
    [Header("UI Elemanları")]
    public GameObject terminalCanvas; // Tüm ekranı kaplayan siyah panel
    public TextMeshProUGUI terminalText;
    
    [Header("Oyun Objeleri")]
    public GameObject zorboPlayer;
    public GameObject glitchLine;

    void Start() 
    {
        // Sahne yüklendiği anda Zorbo'yu gizle, terminali başlat
        zorboPlayer.SetActive(false);
        glitchLine.SetActive(false);
        terminalCanvas.SetActive(true); 
        terminalText.text = ""; // Metni temizle
        
        StartCoroutine(PlayTerminalIntro());
    }

    IEnumerator PlayTerminalIntro() 
    {
        // 1. Terminal Yazıları Akar
        string[] lines = {
            "> ROOT_ACCESS_GRANTED...",
            "> C:\\System\\Core\\Execute_Zorbo.exe",
            "> Loading Environment Variables... [OK]",
            "> Warning: High Virus Threat Detected!",
            "> DEPLOYING DEBUGGER_UNIT..."
        };

        foreach (string line in lines) {
            terminalText.text += line + "\n";
            // Burada klavye "tık tık" sesi çaldırabilirsin
            yield return new WaitForSeconds(0.6f); 
        }

        yield return new WaitForSeconds(0.5f);
        
        // 2. Siyah Ekranı Kapat ve Glitch Çizgisini Aç
        terminalCanvas.SetActive(false); 
        glitchLine.SetActive(true);     
        
        // (İsteğe bağlı) Burada bir "cızırtı/bzzzt" ses efekti harika olur
        yield return new WaitForSeconds(0.3f);
        
        // 3. Zorbo Fırlıyor!
        zorboPlayer.SetActive(true);    
        glitchLine.SetActive(false);    

        // 4. Silah paketleri düşüyor (Bunu bir sonraki adımda yazarız)
        Debug.Log("Zorbo oyuna girdi, silahlar düşüyor!");
        
        // Düşmanların (virüslerin) doğmaya başlamasını tetikle
        // EnemySpawner.StartSpawning(); 
    }
}