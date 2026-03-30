using UnityEngine;

public class CoinSpin : MonoBehaviour
{
    [Header("Ayarlar")]
    public float donmeHizi = 200f; // Dönüş hızı

    void Update()
    {
        // Parayı kendi Y ekseni etrafında döndürür
        // 2D projelerde Y ekseninde döndürmek "kart çevirme" efekti yaratır
        transform.Rotate(Vector3.up * donmeHizi * Time.deltaTime);
    }
}