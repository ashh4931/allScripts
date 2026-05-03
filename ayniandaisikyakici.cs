using UnityEngine;
using UnityEngine.Rendering.Universal; // Light2D kullanmak için bu kütüphane şart!

public class ayniandaisikyakici : MonoBehaviour
{
    [Header("Işık Listesi")]
    [Tooltip("Nefes almasını istediğin tüm ışıkları buraya sürükle.")]
    public Light2D[] targetLights;

    [Header("Nefes Ayarları")]
    public float minIntensity = 0.4f;   // En sönük hali
    public float maxIntensity = 1.2f;   // En parlak hali
    public float speed = 2f;            // Nefes alma hızı

    void Update()
    {
        if (targetLights == null || targetLights.Length == 0) return;

        // Sinüs dalgası kullanarak 0 ile 1 arasında bir değer üretiyoruz
        // Formül: (sin(zaman * hız) + 1) / 2
        float lerpFactor = (Mathf.Sin(Time.time * speed) + 1f) / 2f;

        // Belirlediğimiz min ve max arasında yumuşak geçiş yapıyoruz
        float currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, lerpFactor);

        // Listedeki her bir ışığa bu yoğunluğu basıyoruz
        foreach (Light2D light in targetLights)
        {
            if (light != null)
            {
                light.intensity = currentIntensity;
            }
        }
    }
}
 