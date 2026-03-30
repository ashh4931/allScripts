using UnityEngine;

public class MouseTrailFollow : MonoBehaviour
{
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        // 1. Fare pozisyonunu piksellerle al
        Vector3 mousePos = Input.mousePosition;

        // 2. Kamera ile objen arasındaki mesafeyi ayarla (Z ekseni)
        // 2D oyunlarda kamera genellikle -10'dadır, objeler 0'dadır. 
        // Bu yüzden mesafeye 10 veriyoruz.
        mousePos.z = 9f; 

        // 3. Pikseli Dünya Koordinatına (Unity birimlerine) çevir
        Vector3 worldPos = mainCam.ScreenToWorldPoint(mousePos);

        // 4. Objenin pozisyonunu güncelle
        transform.position = worldPos;
    }
}