using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DemoIntro : MonoBehaviour
{
    [SerializeField] private float waitTime = 7f; // Bekleme süresi (saniye)
    [SerializeField] private string nextSceneName = "MainMenu"; // Ana menü sahnesinin adı

    void Start()
    {
        StartCoroutine(LoadMainMenu());
    }

    IEnumerator LoadMainMenu()
    {
        // Belirlenen süre kadar bekle
        yield return new WaitForSeconds(waitTime);
        
        // Ana menü sahnesini yükle
        SceneManager.LoadScene(nextSceneName);
    }
}