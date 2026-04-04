using UnityEngine;

public class ShadowFollower : MonoBehaviour
{
    [Header("Takip Ayarları")]
    public Transform target; // Takip edeceği el (Örn: NorthHand)
    public Vector3 offset = new Vector3(0, -0.5f, 0); // Elden ne kadar aşağıda duracak?

    [Header("Görsel Ayarlar")]
    public bool matchTargetScale = true; // El büyürse gölge de büyüsün mü?

    // Update yerine LateUpdate kullanıyoruz ki, el hareketini bitirdikten SONRA gölge onu takip etsin.
    // Bu sayede titreme (jitter) olmaz.
    void LateUpdate()
    {
        if (target != null)
        {
            // 1. Pozisyonu takip et (Belirlediğimiz mesafe kadar aşağıdan)
            transform.position = target.position + offset;

            // 2. Rotasyonu KİLİTLE! (El fırıldak gibi dönse bile gölge hep düz durur)
            transform.rotation = Quaternion.identity;

            // 3. Elin büyüklüğüne göre gölgeyi de ayarla (İsteğe bağlı)
            if (matchTargetScale)
            {
                // Elin boyutunu alıp biraz daha yassı (basık) bir gölge yapıyoruz
                transform.localScale = new Vector3(target.localScale.x, target.localScale.y * 0.5f, 1f);
            }
        }
    }
}