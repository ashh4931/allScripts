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

    // Bu değişkenleri Inspector'dan test için kullanıyorsan tutabilirsin,
    // ancak Update içinde mikseri ezmek için KULLANMAMALISIN.
    [Header("Ses Ayarları (Varsayılanlar)")]
    [Range(0.0001f, 1f)] public float masterVolume = 0.3f;
    [Range(0.0001f, 1f)] public float musicVolume = 0.3f;
    [Range(0.0001f, 1f)] public float sfxVolume = 0.3f;
    [Range(0.0001f, 1f)] public float bulletVolume = 0.3f;
    [Range(0.0001f, 1f)] public float ambienceVolume = 0.3f;

    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        // Oyun başladığında kaydedilmiş ses ayarlarını PlayerPrefs'ten çekiyoruz.
        // Eğer daha önce kaydedilmemişse varsayılan değerleri (örneğin 1f) kullanıyoruz.
        if (mainMixer != null)
        {
            SetVolume("MasterVolume", PlayerPrefs.GetFloat("MasterVolume", 1f));
            SetVolume("MusicVolume", PlayerPrefs.GetFloat("MusicVolume", 1f));
            SetVolume("SFXVolume", PlayerPrefs.GetFloat("SFXVolume", 1f));
            
            // Bullets ve Ambience için SettingsManager'da bir ayar yapmamışsın,
            // onlara inspector'daki varsayılan değerleri atayabilirsin.
            SetVolume("BulletsVolume", bulletVolume);
            SetVolume("AmbienceVolume", ambienceVolume);
        }
    }

    // UPDATE METODU SİLİNDİ!

    public void SetVolume(string parameterName, float sliderValue)
    {
        if (mainMixer != null)
        {
            // Slider değeri 0 olursa Log10 hata verir, minimum 0.0001f olduğundan eminiz.
            mainMixer.SetFloat(parameterName, Mathf.Log10(sliderValue) * 20);
        }
    }

    // --- DİĞER FONKSİYONLARIN (PlaySFXAtPosition vb.) BURADAN AŞAĞISI AYNI KALACAK ---
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        PlayClipWithGroup(clip, position, volume, sfxGroup, pitch);
    }

    public void PlayBulletSFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        PlayClipWithGroup(clip, position, volume, GunShot, pitch);
    }

    public void PlayAmbienceAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        PlayClipWithGroup(clip, position, volume, ambienceGroup, pitch);
    }

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

        if (pitch == 1f) audioSource.pitch = Random.Range(0.9f, 1.1f);
        else audioSource.pitch = pitch;

        audioSource.Play();
        Destroy(tempAudioObj, clip.length / audioSource.pitch);
    }

    public void Play3DSFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float minDistance = 2f, float maxDistance = 20f)
    {
        if (clip == null) return;

        GameObject tempAudioObj = new GameObject("Temp3DSFX_" + clip.name);
        tempAudioObj.transform.position = position;

        AudioSource audioSource = tempAudioObj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = sfxGroup;
        audioSource.volume = volume;
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.pitch = Random.Range(0.9f, 1.1f);

        audioSource.Play();
        Destroy(tempAudioObj, clip.length / audioSource.pitch);
    }
}