using UnityEngine;

public partial class AltarSpawner : MonoBehaviour
{
    [Header("Spawn Nesneleri")]
    public GameObject[] prefabsToSpawn;

    [Header("Zamanlama Ayarları (Saniye)")]
    [Tooltip("Minimum spawn süresi")]
    public float minSpawnInterval = 20f;
    [Tooltip("Maksimum spawn süresi")]
    public float maxSpawnInterval = 40f;

    [Header("Fırlatma ve Açı Ayarları")]
    public float launchForce = 7f;
    [Range(0f, 180f)]
    [Tooltip("Yukarı yön eksen alınarak sağa ve sola ne kadar sapabileceğini belirler.")]
    public float spreadAngle = 45f;

    [Header("Ses Ayarları")]
    public AudioClip spawnSound;
    [Range(0f, 1f)] public float soundVolume = 0.7f;

    private float timer;

    void Start()
    {
        // İlk spawn süresini rastgele belirle
        ResetTimer();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnAndLaunch();
            ResetTimer(); // Bir sonraki spawn için süreyi tekrar rastgele belirle
        }
    }

    void ResetTimer()
    {
        timer = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    void SpawnAndLaunch()
    {
        if (prefabsToSpawn.Length == 0) return;

        // 1. Obje Seçimi ve Oluşturma
        int randomIndex = Random.Range(0, prefabsToSpawn.Length);
        GameObject spawnedObject = Instantiate(prefabsToSpawn[randomIndex], transform.position, Quaternion.identity);

        // 2. Rastgele Açı Hesaplama
        // Yukarı yönü (Vector2.up) baz alarak sağa veya sola rastgele sapma ekliyoruz
        float randomAngle = Random.Range(-spreadAngle / 2f, spreadAngle / 2f);

        // Açıyı rotasyona çevirip yukarı yön vektörüyle çarpıyoruz
        Vector2 launchDirection = Quaternion.Euler(0, 0, randomAngle) * Vector2.up;

        // 3. Fizik Uygulama
        Rigidbody2D rb = spawnedObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(launchDirection * launchForce, ForceMode2D.Impulse);
        }

        // 4. Ses Çalma (Senin AudioManager'ın üzerinden)
        // Eski satır yerine bunu kullan:
        if (spawnSound != null && AudioManager.instance != null)
        {
            // Artık PlaySFXAtPosition yerine Play3DSFXAtPosition çağırıyoruz
            // En sondaki 2f (min) ve 20f (max) mesafeleri isteğine göre değiştirebilirsin
            AudioManager.instance.Play3DSFXAtPosition(spawnSound, transform.position, soundVolume, 2f, 20f);
        }
    }
}