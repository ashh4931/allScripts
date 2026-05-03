using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuSoundManager : MonoBehaviour
{
    [Header("Müzik Listesi")]
    public List<AudioClip> menuPlayList;
    [Range(0.1f, 5f)] public float fadeDuration = 1.5f;
    
    [Header("Buton Sesleri")]
    public AudioClip buttonClickClip;

    private AudioSource musicSource;
    private int currentTrackIndex = 0;
    private bool isFadingOut = false; // Çakışmaları önlemek için kontrol

    void Start()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        
        // AudioManager'ın Awake içinde oluşması için bir kare bekleyelim veya kontrol edelim
        StartCoroutine(InitializeMusic());
    }

    IEnumerator InitializeMusic()
    {
        // AudioManager instance'ı oluşana kadar bekle (NullReference hatasını önler)
        while (AudioManager.instance == null)
        {
            yield return null;
        }

        musicSource.outputAudioMixerGroup = AudioManager.instance.musicGroup;
        musicSource.loop = false;

        if (menuPlayList.Count > 0)
        {
            StartCoroutine(PlayPlaylist());
        }
    }

    IEnumerator PlayPlaylist()
    {
        while (!isFadingOut)
        {
            AudioClip clipToPlay = menuPlayList[currentTrackIndex];
            musicSource.clip = clipToPlay;
            musicSource.Play();

            yield return StartCoroutine(FadeMusic(0, 0.5f, fadeDuration)); // Başlangıçta %50 ses

            float waitTime = clipToPlay.length - fadeDuration;
            if (waitTime > 0) yield return new WaitForSeconds(waitTime);

            if (!isFadingOut)
                yield return StartCoroutine(FadeMusic(musicSource.volume, 0, fadeDuration));

            currentTrackIndex = (currentTrackIndex + 1) % menuPlayList.Count;
        }
    }

    IEnumerator FadeMusic(float startTarget, float endTarget, float duration)
    {
        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startTarget, endTarget, currentTime / duration);
            yield return null;
        }
        musicSource.volume = endTarget;
    }

    public void PlayButtonAndFadeOutMusic()
    {
        if (isFadingOut) return; // Zaten basıldıysa tekrar çalışma
        isFadingOut = true;

        // 1. Buton sesini çal (AudioManager aracılığıyla)
        if (buttonClickClip != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFXAtPosition(buttonClickClip, Camera.main.transform.position, 0.1f);
        }

        // 2. Müziği durdurma işlemini başlat
        StopAllCoroutines(); 
        StartCoroutine(FadeOutAndStop());
    }

    private IEnumerator FadeOutAndStop()
    {
        float startVol = musicSource.volume;
        float currentTime = 0;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0, currentTime / fadeDuration);
            yield return null;
        }

        musicSource.Stop();
    }
}