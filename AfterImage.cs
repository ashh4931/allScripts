using UnityEngine;

public class AfterImage : MonoBehaviour
{
    private SpriteRenderer sr;
    private float fadeSpeed; // Artık dışarıdan geliyor
    private float alpha = 0.8f;

    // Init fonksiyonuna 'lifeTimeSpeed' parametresini ekledik
    public void Init(Sprite sprite, Vector3 pos, Quaternion rot, Vector3 scale, int sortingOrder, float multiplier, float lifeTimeSpeed)
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        transform.position = pos;
        transform.rotation = rot;
        transform.localScale = scale * multiplier;
        sr.sortingOrder = sortingOrder - 1;
        sr.color = new Color(0.5f, 0.8f, 1f, alpha);

        // Silinme hızını kafa ne derse o yaptık
        this.fadeSpeed = lifeTimeSpeed;
    }

    void Update()
    {
        alpha -= Time.deltaTime * fadeSpeed;
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);

        // Küçülme/Büyüme dengesi (Boyuta göre yavaşlatılmış)
        transform.localScale += Vector3.one * Time.deltaTime * 0.1f;

        if (alpha <= 0) Destroy(gameObject);
    }
}  //public float fadeSpeed = 0.2f; // 4f yerine 1f yaparsan 4 kat daha uzun süre kalır.
   // private float alpha = 0.6f;  // Başlangıç şeffaflığını biraz artır ki daha belirgin olsun.
