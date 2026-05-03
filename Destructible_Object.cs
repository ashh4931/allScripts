using UnityEngine;
using SpriteFracture;
using System.Collections; // Coroutine için gerekli

public class Destructible_Object : MonoBehaviour, IDamageable
{
    public float health = 50f;
    
    [Header("Ses Ayarları")]
    public AudioClip fractureSound;
    [Range(0f, 1f)] public float volume = 1f;

    private SpriteFracturer2D fracturer;
    private bool isDead = false;

    void Awake()
    {
        fracturer = GetComponent<SpriteFracturer2D>();
    }

    void Start()
    {
        // Menajerleri aramaya başla
        StartCoroutine(SearchForManagers());
    }

    IEnumerator SearchForManagers()
    {
        // İkisi birden bulunana kadar döngü devam eder
        while (AudioManager.instance == null || Camera.main == null)
        {
           // Debug.Log($"<color=orange>[MANAGER SEARCH]</color> {gameObject.name} menajerleri arıyor...");
            yield return new WaitForSeconds(3f); // 3 saniye bekle
        }

       // Debug.Log("<color=green>[MANAGER FOUND]</color> AudioManager ve Camera.main başarıyla bağlandı!");
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
        if (isDead) return;
        isDead = true;

        // SES ÇALMA (Null Check ile Güvenli Hale Getirildi)
        if (fractureSound != null && AudioManager.instance != null)
        {
            // Kamera o an yoksa bile hata vermemesi için pozisyon kontrolü
            Vector3 soundPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioManager.instance.PlaySFXAtPosition(fractureSound, soundPos, volume);
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