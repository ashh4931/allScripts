using UnityEngine;

public class LifeOverflow : MonoBehaviour
{

    public GameObject VfxPrefab;
     public AudioClip healSound;
    private AudioSource audioSource; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
     
private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    void playSoundEffect()
    {
        audioSource.PlayOneShot(healSound);
    }
    // Update is called once per frame
   public void use()
    {
        playSoundEffect();
        PlayVfx(GetComponent<Rigidbody2D>().position);
        GetComponent<StatController>().Heal(300f);
          
    }

     public void PlayVfx(Vector3 position)
    {
       GameObject vfx = Instantiate(VfxPrefab, position, Quaternion.identity, transform);
// Bu durumda VfxPrefab, scriptin bağlı olduğu objenin child’i olur

        
        ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            Destroy(vfx, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {//Life Overflow
            Destroy(vfx, 3f); // ParticleSystem yoksa 2 saniye sonra yok et
        }
    }
}
