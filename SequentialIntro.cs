using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class IntroSequence : MonoBehaviour
{
    [Header("Ayarlar")]
    public CanvasGroup logoGroup;
    public CanvasGroup disclaimerGroup;
    public float fadeDuration = 1.5f;
    public float waitTime = 2.0f;
    public string nextSceneName = "MainMenu";

    private bool isSkipping = false; // Çift tetiklenmeyi önlemek için

    void Start()
    {
        logoGroup.alpha = 0;
        disclaimerGroup.alpha = 0;
        StartCoroutine(PlaySequence());
    }

    void Update()
    {
        // Fare sol tık (0) yapıldığında sahneyi direkt geç
        if (Input.GetMouseButtonDown(0) && !isSkipping)
        {
            SkipToNextScene();
        }
    }

    IEnumerator PlaySequence()
    {
        // 1. LOGO FADE IN
        yield return StartCoroutine(Fade(logoGroup, 0, 1));
        yield return new WaitForSeconds(waitTime);

        // 2. LOGO FADE OUT
        yield return StartCoroutine(Fade(logoGroup, 1, 0));

        // 3. DISCLAIMER FADE IN
        yield return StartCoroutine(Fade(disclaimerGroup, 0, 1));
        yield return new WaitForSeconds(waitTime + 1.5f); 

        // 4. OTOMATİK GEÇİŞ
        SkipToNextScene();
    }

    void SkipToNextScene()
    {
        isSkipping = true;
        StopAllCoroutines(); // Çalışan fade işlemlerini durdur
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator Fade(CanvasGroup group, float start, float end)
    {
        float counter = 0f;
        while (counter < fadeDuration)
        {
            counter += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, end, counter / fadeDuration);
            yield return null;
        }
        group.alpha = end;
    }
}