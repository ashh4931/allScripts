using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FloorSpawner : MonoBehaviour
{
    private BoxCollider2D spawnArea;

    void Awake()
    {
        spawnArea = GetComponent<BoxCollider2D>();
        spawnArea.isTrigger = true; // Oyuncu çarpıp takılmasın
    }

    // WaveManager bu fonksiyonu çağırıp eline bir prefab tutuşturacak
    // FloorSpawner.cs içinde olmalı!
    public GameObject SpawnSpecificEnemy(GameObject prefab)
    {
        // 1. Önce hazırladığın fonksiyonu kullanarak collider içinden rastgele bir nokta al
        Vector2 randomSpawnPosition = GetRandomPointInBounds(spawnArea.bounds);

        // 2. Instantiate kısmında transform.position yerine bu yeni noktayı kullan
        GameObject spawnedObject = Instantiate(prefab, randomSpawnPosition, Quaternion.identity);

        // Doğurulan objeyi geri döndür
        return spawnedObject;
    }

    private Vector2 GetRandomPointInBounds(Bounds bounds)
    {
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = Random.Range(bounds.min.y, bounds.max.y);
        return new Vector2(randomX, randomY);
    }

    private void OnDrawGizmos()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}