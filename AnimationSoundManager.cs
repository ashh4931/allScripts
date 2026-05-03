using UnityEngine;

public class AnimationSoundManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip whoosh;
    

    // Bu fonksiyonu Animation Event çağıracak
    public void PlaySpawnSound()
    {
        if (audioSource != null && whoosh != null)
        {
            // PlayOneShot kullanmak iyidir, sesler üst üste binebilir
            audioSource.PlayOneShot(whoosh);
        }
    }
}