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
    public void SpawnSpecificEnemy(GameObject enemyPrefab)
    {
        if (enemyPrefab == null) return;

        Vector2 randomPoint = GetRandomPointInBounds(spawnArea.bounds);
        Instantiate(enemyPrefab, randomPoint, Quaternion.identity);
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