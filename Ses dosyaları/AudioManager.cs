using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Ana Mikser ve Kanallar")]
    public AudioMixer mainMixer;
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup ambienceGroup;
    public AudioMixerGroup GunShot;

    [Header("Ses Ayarları")]
    [Range(0.0001f, 1f)] public float masterVolume = 1f;
    [Range(0.0001f, 1f)] public float musicVolume = 1f;
    [Range(0.0001f, 1f)] public float sfxVolume = 1f;
    [Range(0.0001f, 1f)] public float bulletVolume = 1f;
    [Range(0.0001f, 1f)] public float ambienceVolume = 1f;

    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Update()
    {
        if (mainMixer != null)
        {
            SetVolume("MasterVolume", masterVolume);
            SetVolume("MusicVolume", musicVolume);
            SetVolume("SFXVolume", sfxVolume);
            SetVolume("BulletsVolume", bulletVolume);
            SetVolume("AmbienceVolume", ambienceVolume);
        }
    }

    public void SetVolume(string parameterName, float sliderValue)
    {
        if (mainMixer != null)
            mainMixer.SetFloat(parameterName, Mathf.Log10(sliderValue) * 20);
    }

    // --- 🔴 FONKSİYONLAR (Hataları Gideren Kısım) ---

    // Normal Efektler (Yılanın patlaması, taş kırılması vb.)
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        PlayClipWithGroup(clip, position, volume, sfxGroup, pitch);
    }

    // Mermi Sesleri
    public void PlayBulletSFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        PlayClipWithGroup(clip, position, volume, GunShot, pitch);
    }

    // Ambiyans Sesleri (Hata aldığın LightningSpawner burayı kullanıyor)
    public void PlayAmbienceAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        PlayClipWithGroup(clip, position, volume, ambienceGroup, pitch);
    }

    // --- ÖZEL SES YARATICI ---
    private void PlayClipWithGroup(AudioClip clip, Vector3 position, float volume, AudioMixerGroup group, float pitch = 1f)
    {
        if (clip == null) return;

        GameObject tempAudioObj = new GameObject("TempSFX_" + clip.name);
        tempAudioObj.transform.position = position;

        AudioSource audioSource = tempAudioObj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = group;
        audioSource.volume = volume;
        audioSource.spatialBlend = 0f;

        // EĞER özel bir pitch (yılan patlaması gibi) gelmediyse hafif rastgelelik ekle
        if (pitch == 1f)
            audioSource.pitch = Random.Range(0.9f, 1.1f);
        else
            audioSource.pitch = pitch;

        audioSource.Play();
        Destroy(tempAudioObj, clip.length / audioSource.pitch);
    }
}