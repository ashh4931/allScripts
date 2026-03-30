using UnityEngine;
using SpriteFracture;

public class Destructible_Object : MonoBehaviour, IDamageable
{
    public float health = 50f;
    
    [Header("Ses Ayarları")]
    public AudioClip fractureSound; // Kırılma sesi
    [Range(0f, 1f)] public float volume = 1f;

    private SpriteFracturer2D fracturer;
    private bool isDead = false; // Sesin birden fazla kez çalmasını engellemek için

    void Awake()
    {
        fracturer = GetComponent<SpriteFracturer2D>();
    }

  public void TakeDamage(float damage, float minDamage = 0f, float maxDamage = 0f)
    {
        if (isDead) return;

        health -= damage;

        if (health <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        // Sesi dünyada, objenin bulunduğu pozisyonda çalar. 
        // Obje yok olsa bile ses çalmaya devam eder.
        if (fractureSound != null)
        {
          //AudioSource.PlayClipAtPoint(fractureSound, transform.position, volume);
            AudioSource.PlayClipAtPoint(fractureSound, Camera.main.transform.position, volume);
        }

        if (fracturer != null)
        {
            StartCoroutine(fracturer.Fracture());
        }
        else
        {
            Destroy(gameObject);
        }
    }
}