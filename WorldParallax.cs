using UnityEngine;

public class WorldParallax : MonoBehaviour
{
    private Vector3 startPos;

    [Header("Hareket Ayarları")]
    // Dünya koordinatlarında değerler çok küçük olmalı!
    // Arkaplan için: 0.2f | Örümcek ağı için: 0.5f civarı dene.
    public float moveAmount = 0.2f; 
    public float smoothness = 5f;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // 1. Fare pozisyonunu ekranın merkezine göre normalize et (-1 ile 1 arası)
        Vector3 mousePos = Input.mousePosition;
        float xFactor = (mousePos.x - (Screen.width / 2)) / (Screen.width / 2);
        float yFactor = (mousePos.y - (Screen.height / 2)) / (Screen.height / 2);

        // 2. Hedef dünya pozisyonu
        Vector3 targetPos = startPos - new Vector3(xFactor * moveAmount, yFactor * moveAmount, 0);

        // 3. Yumuşak geçiş
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothness);
    }
}