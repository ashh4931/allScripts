using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StormVerdictSkill : MonoBehaviour
{
    public GameObject circlePrefab;
    
    [Header("Ayarlar")]
    public float spawnRadius = 20f;
    public float minDistanceBetweenCircles = 3f;
    public int maxCircles = 6;
    public float spawnInterval = 0.5f;
    public float lightningHeight = 25f;

    private List<VerdictCircle> activeCircles = new List<VerdictCircle>();

    public void Use(float damage, float circleRadius)
    {
        StartCoroutine(SpawnRoutine(damage, circleRadius));
    }

    private IEnumerator SpawnRoutine(float damage, float circleRadius)
    {
        activeCircles.Clear();
        List<Vector2> spawnPoints = new List<Vector2>();

        for (int i = 0; i < maxCircles; i++)
        {
            Vector2 spawnPos = CalculateValidSpawnPoint(spawnPoints);
            spawnPoints.Add(spawnPos);

            GameObject circleObj = Instantiate(circlePrefab, spawnPos, Quaternion.identity);
            VerdictCircle script = circleObj.GetComponent<VerdictCircle>();
            
            if (script != null)
            {
                script.Setup(damage, circleRadius, lightningHeight);
                activeCircles.Add(script);
            }

            yield return new WaitForSeconds(spawnInterval);
        }

        // Hepsi tamamlanınca yıldırımları indir!
        yield return new WaitForSeconds(0.2f);
        foreach (var circle in activeCircles)
        {
            if (circle != null) circle.Strike();
        }
    }

    private Vector2 CalculateValidSpawnPoint(List<Vector2> existingPoints)
    {
        Vector2 center = transform.position;
        Vector2 finalPos = center;

        for (int attempt = 0; attempt < 50; attempt++) // 50 deneme sınırı
        {
            Vector2 randomPoint = center + Random.insideUnitCircle * spawnRadius;
            bool tooClose = false;

            foreach (Vector2 p in existingPoints)
            {
                if (Vector2.Distance(randomPoint, p) < minDistanceBetweenCircles)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                finalPos = randomPoint;
                break;
            }
        }
        return finalPos;
    }
}