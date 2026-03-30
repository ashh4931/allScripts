using UnityEngine;

public class RotateToAlpha : MonoBehaviour
{
    [Header("Referans")]
    public Transform hand; 

    [Header("Alpha Ayarları")]
    [Range(0f, 1f)] public float maxAlpha = 1f;    // En yukarıdayken (90 derece)
    [Range(0f, 1f)] public float minAlpha = 0.15f; // En aşağıdayken (-90 derece)

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (hand == null)
            hand = transform.parent;
    }

    void Update()
    {
        // Silah kuşanılmamışsa işlem yapma
        var weaponComponent = GetComponentInParent<weapon>();
        if (weaponComponent != null && weaponComponent.isEquipped == false)
            return;

        // 1️⃣ Elin dünya koordinatlarındaki 'yukarı' yönelimini al
        // hand.right bazen flip yediği için yanıltıcı olabilir. 
        // Bunun yerine elin merkezinden mouse'a giden gerçek yönü kullanmak daha garantidir.
        
        Vector3 forwardDir = hand.right; 
        
        // 2️⃣ Y eksenindeki sapmayı bul (-1 ile 1 arası)
        // Eğer el tam yukarı bakıyorsa 1, tam aşağı bakıyorsa -1, yatayda 0 döner.
        // Normalize edilmiş vektörde y değeri bize Sinüs'ü verir.
        float yComponent = forwardDir.y; 

        // 3️⃣ -1 (aşağı) ve 1 (yukarı) aralığını 0 ile 1 arasına çek
        // minAlpha (aşağı) için 0, maxAlpha (yukarı) için 1.
        float t = Mathf.InverseLerp(-1f, 1f, yComponent);

        // 4️⃣ Alpha hesapla
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        // 5️⃣ Uygula
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }
}