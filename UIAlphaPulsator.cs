using UnityEngine;
using UnityEngine.UI; // Image bileşeni için gerekli
using System.Collections.Generic;

public class UIAlphaPulsator : MonoBehaviour
{
    [Header("UI Elemanları")]
    public List<Image> uiImages = new List<Image>();

    [Header("Şeffaflık Sınırları (0 ile 1 arası)")]
    [Range(0f, 1f)] public float minAlpha = 0.2f;
    [Range(0f, 1f)] public float maxAlpha = 1.0f;

    [Header("Hız Ayarları")]
    public float minSpeed = 0.5f;
    public float maxSpeed = 2.0f;

    private List<float> speeds = new List<float>();
    private List<float> offsets = new List<float>();

    void Start()
    {
        // Her görsel için rastgele hız ve başlangıç zamanı belirliyoruz
        foreach (var img in uiImages)
        {
            speeds.Add(Random.Range(minSpeed, maxSpeed));
            offsets.Add(Random.Range(0f, 100f));
        }
    }

    void Update()
    {
        for (int i = 0; i < uiImages.Count; i++)
        {
            if (uiImages[i] != null)
            {
                // PingPong ile 0-1 arası yumuşak gidiş geliş sağlıyoruz
                float lerpVal = Mathf.PingPong((Time.time + offsets[i]) * speeds[i], 1f);
                
                // Mevcut rengi alıp sadece alpha (a) değerini değiştiriyoruz
                Color tempColor = uiImages[i].color;
                tempColor.a = Mathf.Lerp(minAlpha, maxAlpha, lerpVal);
                uiImages[i].color = tempColor;
            }
        }
    }
}