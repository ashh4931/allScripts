using UnityEngine;
using System.Collections.Generic;

public class AdvancedMenuSpawner : MonoBehaviour
{
    [Header("Mermi Havuzu")]
    // Buraya istediğin kadar farklı mermi prefabı ekleyebilirsin
    public GameObject[] bulletPrefabs; 

    [Header("Spawn Aralığı")]
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = 5f;  // En düşük spawn yüksekliği
    public float maxY = 10f; // En yüksek spawn yüksekliği
    public float spawnZ = 90f;

    [Header("Zamanlama ve Hız")]
    public float spawnRate = 0.3f; 
    public float minSpeed = 3f;
    public float maxSpeed = 7f;

    [Header("Açı Ayarları")]
    // 180 tam aşağı bakar. 150-210 arası hafif açılı düşmelerini sağlar.
    public float minAngle = 150f; 
    public float maxAngle = 210f;

    private float nextSpawnTime;

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnBullet();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    void SpawnBullet()
    {
        if (bulletPrefabs.Length == 0) return;

        // 1. Rastgele bir mermi prefabı seç
        int randomIndex = Random.Range(0, bulletPrefabs.Length);
        GameObject selectedPrefab = bulletPrefabs[randomIndex];

        // 2. Rastgele Pozisyon ve Açı hesapla
        Vector3 spawnPos = new Vector3(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY),
            spawnZ
        );

        float randomAngle = Random.Range(minAngle, maxAngle);
        Quaternion spawnRot = Quaternion.Euler(0, 0, randomAngle);

        // 3. Mermiyi oluştur
        GameObject newBullet = Instantiate(selectedPrefab, spawnPos, spawnRot);

        // 4. Hareket ve Hız bileşenini ekle
        float speed = Random.Range(minSpeed, maxSpeed);
        
        // Merminin kendi içinde hareket kodu yoksa bu yardımcıyı ekliyoruz:
        MenuBulletBehavior behavior = newBullet.AddComponent<MenuBulletBehavior>();
        behavior.speed = speed;
        
        // Opsiyonel: Rastgele boyut ekleyerek derinlik hissi ver
        float s = Random.Range(0.7f, 1.3f);
        newBullet.transform.localScale = new Vector3(s, s, 1f);
    }
}

// Merminin kendi hareketi için yardımcı class
public class MenuBulletBehavior : MonoBehaviour 
{
    public float speed;

    void Update() 
    {
        // transform.up kullanıyoruz çünkü mermiyi instantiate ederken döndürdük. 
        // Artık mermi kendi "yukarısına" (ki biz onu aşağı çevirdik) doğru gider.
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        // Ekrandan çok uzaklaşınca yok et (Performans için önemli)
        if (transform.position.y < -15f || transform.position.y > 20f || 
            transform.position.x < -20f || transform.position.x > 20f) 
        {
            Destroy(gameObject);
        }
    }
}