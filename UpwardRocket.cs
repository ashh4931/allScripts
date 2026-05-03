using UnityEngine;

public class UpwardRocket : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float startSpeed = 5f;        // İlk çıkış hızı
    public float acceleration = 25f;     // Giderek hızlanma miktarı
    public float lifeTime = 1.5f;        // Silinme süresi

    [Header("Görsel Efektler")]
    public Transform firePoint;          // 🔴 YENİ: Ateşin çıkacağı tam nokta
    public GameObject fireVFXPrefab;     // 🔴 YENİ: Oynatılacak ateş efekti prefabı

    private float currentSpeed;

    private void Start()
    {
        currentSpeed = startSpeed;

        // Ateş noktası ve efekti belirlenmişse, efekti tam o noktada yarat
        if (firePoint != null && fireVFXPrefab != null)
        {
            // Efekti firePoint'in içine (child olarak), onun pozisyonu ve açısıyla yaratıyoruz.
            // Böylece roket hareket ettikçe efekt de onunla birlikte gelir.
            Instantiate(fireVFXPrefab, firePoint.position, firePoint.rotation, firePoint);
        }

        // Belirlenen süre sonunda objeyi sahneden tamamen sil
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // Roketi her karede ivmelendir
        currentSpeed += acceleration * Time.deltaTime;

        // Yukarı doğru hareket ettir
        transform.Translate(Vector2.up * currentSpeed * Time.deltaTime);
    }
}