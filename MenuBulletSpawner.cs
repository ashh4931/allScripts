using UnityEngine; // Tüm using'ler en üstte olmalı

public class MenuBulletSpawner : MonoBehaviour
{
    [Header("Mermi Havuzu")]
    public GameObject[] bulletPrefabs; 

    [Header("Göreceli Spawn Aralıkları")]
    public float relativeMinX = -5f;
    public float relativeMaxX = 5f;
    public float relativeMinY = -2f;
    public float relativeMaxY = 2f;
    public float relativeMinZ = -2f;
    public float relativeMaxZ = 5f;

    [Header("Zamanlama ve Hız")]
    public float spawnRate = 0.3f; 
    public float minSpeed = 3f;
    public float maxSpeed = 7f;

    [Header("Derinlik Efekti")]
    public bool useScaleByDepth = true;

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
        if (bulletPrefabs == null || bulletPrefabs.Length == 0) return;

        int randomIndex = Random.Range(0, bulletPrefabs.Length);
        
        float randomZ = Random.Range(relativeMinZ, relativeMaxZ);
        Vector3 spawnPos = new Vector3(
            transform.position.x + Random.Range(relativeMinX, relativeMaxX),
            transform.position.y + Random.Range(relativeMinY, relativeMaxY),
            transform.position.z + randomZ
        );

        GameObject newBullet = Instantiate(bulletPrefabs[randomIndex], spawnPos, transform.rotation);

        if (useScaleByDepth)
        {
            float t = Mathf.InverseLerp(relativeMaxZ, relativeMinZ, randomZ);
            float scale = Mathf.Lerp(0.6f, 1.2f, t);
            newBullet.transform.localScale = Vector3.one * scale;
        }

        float speed = Random.Range(minSpeed, maxSpeed);
        
        // Önemli: MenuBulletBehavior sınıfı aşağıda tanımlı olduğu için hata vermez
        MenuBulletBehavior behavior = newBullet.AddComponent<MenuBulletBehavior>();
        behavior.speed = speed;
    }
}

// DİKKAT: Yeni bir 'using' satırı buraya eklenemez! 
// Sınıf tanımı direkt başlamalı.
public class MenuBulletBehavior : MonoBehaviour 
{
    public float speed;
    public float lifeTime = 12f; 

    void Start() 
    {
        Destroy(gameObject, lifeTime);
    }

    void Update() 
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }
}