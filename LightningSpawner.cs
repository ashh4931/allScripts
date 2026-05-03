using UnityEngine;
using System.Collections;

public class LightningSpawner : MonoBehaviour
{
    // Yıldırım prefabı
    public GameObject lightningPrefab;

    // Spawn alanı
    public Vector2 minPos;
    public Vector2 maxPos;

    // Spawn aralıkları (min ve max saniye)
    public float minInterval = 1f;
    public float maxInterval = 3f;

    // Yıldırım sesi
    public AudioClip lightningSound;

    void Start()
    {
        // Spawn döngüsünü başlat
        StartCoroutine(SpawnLightningRoutine());
    }

    IEnumerator SpawnLightningRoutine()
    {
        while (true)
        {
            // Rastgele süre bekle
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            // 🔴 SES BURADAN AUDIOMANAGER'A GÖNDERİLİYOR
            if (lightningSound != null)
            {
                AudioManager.instance.PlayAmbienceAtPosition(lightningSound, Camera.main.transform.position, 1f);
            }

            // Sesten 1 saniye sonra yıldırımı oluştur
            yield return new WaitForSeconds(1f);

            // Rastgele pozisyon belirle
            Vector2 randomPos = new Vector2(
                Random.Range(minPos.x, maxPos.x),
                Random.Range(minPos.y, maxPos.y)
            );

            // Yıldırımı oluştur
            GameObject lightning = Instantiate(lightningPrefab, randomPos, Quaternion.identity);
            
            if(lightning.GetComponent<LightningController>() != null)
                lightning.GetComponent<LightningController>().PlayLightning(1f);

            // 1.2 saniye sonra yok et
            Destroy(lightning, 1.2f);
        }
    }
}