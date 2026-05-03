using UnityEngine;
using UnityEngine.XR;
using System.Collections;

public class SwordAttack : MonoBehaviour
{
    public float swingAngle = 90f;      // Toplam savurma açısı
    public float swingDuration = 0.15f;  // Savurmanın süresi

    public AudioClip hitmarketSound;
    private AudioSource audioSource;
   
    bool isSwinging = false;


    private void Awake()
    {      
        audioSource = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag != "Hitable")
        {
            return;
        }
        // Çarpışan collider'ın kendi pozisyonuna en yakın nokta
        Vector2 contactPoint = collision.ClosestPoint(transform.position);

        GetComponent<SwordEffectController>().PlayVfx(contactPoint);
        audioSource.PlayOneShot(hitmarketSound);
    }

     


   

    ///////////////////////////////////////////////////////////////////////////////////////
    /// ///////////////////////////////////////////////////////////////////////////////////////
    /// ///////////////////////////////////////////////////////////////////////////////////////
    /// ///////////////////////////////////////////////////////////////////////////////////////
    /// ///////////////////////////////////////////////////////////////////////////////////////
    /// //AI KODU BURASI
    /// ///////////////////////////////////////////////////////////////////////////////////////
    /// ///////////////////////////////////////////////////////////////////////////////////////       // Toplam savurma açısı

}
