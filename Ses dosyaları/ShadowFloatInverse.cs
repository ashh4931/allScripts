using UnityEngine;

public partial class ShadowFloatVisual : MonoBehaviour
{/*
    public float amplitude = 0.25f;
    public float frequency = 1f;

    [Header("Scale Ayarları")]
    public float minScale = 0.8f;   // Yukarıdayken (Küçük)
    public float maxScale = 1.1f;   // Aşağıdayken (Büyük)

    [Header("Alpha Ayarları")]
    public float minAlpha = 0.3f;   // Aşağıdayken (Şeffaf)
    public float maxAlpha = 0.9f;   // Yukarıdayken (Mat)

    private Vector3 startPos;
    private Vector3 startScale;
    private SpriteRenderer sr;

    void Start()
    {
        startPos = transform.position;
        startScale = transform.localScale;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (GetComponentInParent<weapon>().isEquipped)
            return;
        // Sinüs dalgası: -1 (yukarı) ile +1 (aşağı) arası çalışır
        float sin = Mathf.Sin(Time.time * frequency);

        // Hareket: Sinüs pozitifse yOffset negatif olur (Aşağı iner)
        float yOffset = -sin * amplitude;
        transform.position = startPos + Vector3.up * yOffset;

        // t değerini normalize ediyoruz (0 ile 1 arası)
        // sin = -1 (en üst) ise t = 0
        // sin = 1 (en alt) ise t = 1
        float t = Mathf.InverseLerp(-1f, 1f, sin);

        // SCALE: Aşağıdayken (t=1) maxScale olur.
        float scale = Mathf.Lerp(minScale, maxScale, t);
        transform.localScale = startScale * scale;

        // ALPHA: Aşağıdayken (t=1) minAlpha (şeffaf) olmasını istiyorsun.
        // Bu yüzden t yerine (1 - t) kullanarak alpha'yı tersine çeviriyoruz.
        Color c = sr.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, 1 - t); 
        sr.color = c;
    }*/
}