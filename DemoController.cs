using UnityEngine;
using UnityEngine;
using UnityEngine.UI;

public class DemoController : MonoBehaviour
{
    public Button playButton;

    void Start()
    {
        // Oyun başlar başlamaz demo bitmiş mi kontrol et
        CheckDemoStatus();
    }

    void CheckDemoStatus()
    {
        // "DemoFinished" değeri 1 ise buton deaktif olur
        if (PlayerPrefs.GetInt("DemoFinished", 0) == 1)
        {
            playButton.interactable = false;
            Debug.Log("Demo süresi bittiği için Play butonu devre dışı bırakıldı.");
        }
    }

    // Test amaçlı: Süreyi sıfırlamak istersen bu fonksiyonu bir butona atayabilirsin
    public void ResetDemo()
    {
        PlayerPrefs.DeleteKey("DemoFinished");
        playButton.interactable = true;
    }
}