using UnityEngine;
using UnityEngine.SceneManagement;
// Eğer URP kullanıyorsan bu kütüphaneyi eklemeliyiz:
using UnityEngine.Rendering.Universal; 

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panelleri")]
    public GameObject mainMenuPanel;
    public GameObject storyPanel;

    [Header("Kamera Ayarları")]
    public Camera mainCamera; // Ana kameranı buraya sürükle

    public void ShowStory()
    {
        mainMenuPanel.SetActive(false); 
        storyPanel.SetActive(true);     
        
        // Kameradaki tüm post-process (Lens bükülmesi, Bloom vs.) efektlerini kökten kapatır
        if(mainCamera != null)
        {
            var cameraData = mainCamera.GetComponent<UniversalAdditionalCameraData>();
            if(cameraData != null)
            {
                cameraData.renderPostProcessing = false;
            }
        }
    }

    public void RunZorboExe()
    {
        SceneManager.LoadScene("GameScene"); 
    }
}