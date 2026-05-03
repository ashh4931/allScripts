using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI; // Buton bileşeni için bunu ekledik

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panelleri")]
    public GameObject mainMenuPanel;
    public GameObject storyPanel;
    public GameObject creditsPanel;

    [Header("Kamera Ayarları")]
    public Camera mainCamera;

    [Header("Demo Ayarları")]
    public Button playButton; // Unity Inspector'dan 'Play' butonunu buraya sürükle

    void Start()
    {
        // Oyun açıldığında demo bitmiş mi diye bakıyoruz
        CheckDemoStatus();
    }

    private void CheckDemoStatus()
    {
        // PlayerPrefs'ten "DemoFinished" değerini oku. Yoksa varsayılan 0'dır.
        if (PlayerPrefs.GetInt("DemoFinished", 0) == 1)
        {
            if (playButton != null)
            {
                playButton.interactable = false; // Butona tıklanmasını engeller
                // İstersen burada butonun üzerindeki yazıyı "DEMO BİTTİ" yapabilirsin
            }
        }
    }

    public void ShowCredits()
    {
        mainMenuPanel.SetActive(false);
        creditsPanel.SetActive(true);
        TogglePostProcessing(false);
    }

    public void BackFromCredits()
    {
        creditsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        TogglePostProcessing(true);
    }

    public void ShowStory()
    {
        Debug.Log("Button tıklandı");
        mainMenuPanel.SetActive(false); 
        storyPanel.SetActive(true);     
        TogglePostProcessing(false);
    }

    private void TogglePostProcessing(bool state)
    {
        if(mainCamera != null)
        {
            var cameraData = mainCamera.GetComponent<UniversalAdditionalCameraData>();
            if(cameraData != null)
            {
                cameraData.renderPostProcessing = state;
            }
        }
    }

    public void RunZorboExe()
    {
        // Güvenlik önlemi: Demo bittiyse bu fonksiyon kodla çağrılsa bile çalışmasın
        if (PlayerPrefs.GetInt("DemoFinished", 0) == 1) return;

        SceneManager.LoadScene("GameScene"); 
    }

    // TEST İÇİN: Eğer demoyu sıfırlayıp tekrar denemek istersen bu fonksiyonu kullanabilirsin
    [ContextMenu("Reset Demo Status")]
    public void ResetDemo()
    {
        PlayerPrefs.DeleteKey("DemoFinished");
        if (playButton != null) playButton.interactable = true;
        Debug.Log("Demo durumu sıfırlandı!");
    }
}