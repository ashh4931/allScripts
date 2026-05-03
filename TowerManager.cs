using UnityEngine;
using System.Collections.Generic;

public class TowerManager : MonoBehaviour
{
    [Header("Kule Ayarları")]
    public GameObject towerPrefab;
    public BoxCollider2D spawnArea; // Kulelerin çıkabileceği alan
    public LayerMask towerLayer;   // Kulelerin birbirini algılaması için layer
    public int maxActiveTowers = 5;
    public float minDistanceBetweenTowers = 2.5f;

    [Header("Durum İzleyici")]
    public List<GameObject> activeTowers = new List<GameObject>();

    private void OnEnable()
    {
        // Dalga bittiğinde (dinlenme başladığında) kuleleri dik
        WaveEvents.OnWaveFinished += HandleWaveFinished;
        Debug.Log("<color=magenta>[TowerManager] Dalga bitişlerini dinlemek için abone olundu.</color>");
    }

    private void OnDisable()
    {
        WaveEvents.OnWaveFinished -= HandleWaveFinished;
    }

    private void HandleWaveFinished()
    {
        // Önce yok edilmiş kuleleri listeden temizle
        activeTowers.RemoveAll(t => t == null);

        if (activeTowers.Count < maxActiveTowers)
        {
            Debug.Log("<color=magenta>[TowerManager] Dalga bitti, yeni kuleler aranıyor...</color>");
            SpawnTowers();
        }
        else
        {
            Debug.Log("<color=white>[TowerManager] Maksimum kule sayısına ulaşıldı, yeni kule dikilmiyor.</color>");
        }
    }

    public void SpawnTowers()
    {
        int remainingSlot = maxActiveTowers - activeTowers.Count;
        // Her dalga sonunda rastgele 1 ile 3 arası kule çıksın (isteğe göre ayarlanabilir)
        int countToSpawn = Mathf.Min(Random.Range(1, 4), remainingSlot);

        int successCount = 0;

        for (int i = 0; i < countToSpawn; i++)
        {
            Vector2 randomPos = Vector2.zero;
            bool foundValidPos = false;
            int maxAttempts = 30; // Uygun yer bulmak için deneme sınırı
            int currentAttempt = 0;

            while (!foundValidPos && currentAttempt < maxAttempts)
            {
                randomPos = new Vector2(
                    Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x),
                    Random.Range(spawnArea.bounds.min.y, spawnArea.bounds.max.y)
                );

                // Belirlenen noktada başka kule var mı kontrol et
                Collider2D hit = Physics2D.OverlapCircle(randomPos, minDistanceBetweenTowers, towerLayer);

                if (hit == null) foundValidPos = true;
                currentAttempt++;
            }

            if (foundValidPos)
            {
                GameObject towerObj = Instantiate(towerPrefab, randomPos, Quaternion.identity);
                activeTowers.Add(towerObj);
                successCount++;
                
                // Eğer kulelerin kendi kurulum kodu varsa çağır
                // towerObj.GetComponent<TowerController>()?.SetupTower();
            }
        }

        if (successCount > 0)
        {
            Debug.Log($"<color=magenta>[TowerManager] Başarıyla {successCount} yeni kule dikildi!</color>");
            
            // Eğer HintManager varsa burada tetikleyebilirsin
            // HintManager.Instance?.ShowHint("tower_info", "Kadim kuleler ortaya çıktı!", "KULE UYARISI", true, 5f);
        }
    }
}