using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [Header("Müzik Kaynağı")]
    public AudioSource musicSource;

    [Header("Müzik Listeleri")]
    public List<AudioClip> waveMusicList;
    public List<AudioClip> breakMusicList;

    [Header("Ayarlar")]
    public float fadeDuration = 1.5f;
    private float targetVolume;

    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
        
        if (musicSource == null) musicSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (musicSource != null) targetVolume = musicSource.volume;
    }

    private void OnEnable()
    {
        // DÜZELTME: (points, type) ekleyerek iki parametreyi de kabul ediyoruz
        WaveEvents.OnWaveStarted += HandleWaveStarted;
        WaveEvents.OnWaveFinished += HandleWaveFinished;
    }

    private void OnDisable()
    {
        WaveEvents.OnWaveStarted -= HandleWaveStarted;
        WaveEvents.OnWaveFinished -= HandleWaveFinished;
    }

    // DÜZELTME: Fonksiyon artık iki parametre alıyor (int ve WaveType)
   private void HandleWaveStarted(int index, int points, WaveType type){
        PlayWaveMusic();
    }

    private void HandleWaveFinished()
    {
        PlayBreakMusic();
    }

    public void PlayWaveMusic()
    {
        StopAllCoroutines();
        StartCoroutine(TransitionMusicRoutine(waveMusicList));
    }

    public void PlayBreakMusic()
    {
        StopAllCoroutines();
        StartCoroutine(TransitionMusicRoutine(breakMusicList));
    }

    IEnumerator TransitionMusicRoutine(List<AudioClip> musicList)
    {
        if (musicList == null || musicList.Count == 0 || musicSource == null) yield break;

        float startVolume = musicSource.volume;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }
        musicSource.volume = 0;

        AudioClip nextClip = musicList[Random.Range(0, musicList.Count)];
        musicSource.clip = nextClip;
        musicSource.Play();

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0, targetVolume, t / fadeDuration);
            yield return null;
        }
        musicSource.volume = targetVolume;
    }
}