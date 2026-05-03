using UnityEngine;

public class UIMenuParallax : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector2 startAnchoredPos;

    [Header("Hareket Ayarları")]
    // Arkaplan için: 20-30 arası bir değer
    // Örümcek ağı için: 40-60 arası bir değer (Daha yakın hissi için)
    public float moveAmount = 30f; 
    public float smoothness = 5f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startAnchoredPos = rectTransform.anchoredPosition;
    }

    void Update()
    {
        // 1. Fare pozisyonunu ekranın merkezine göre normalize et (-1 ile 1 arası)
        Vector2 mousePos = Input.mousePosition;
        float xFactor = (mousePos.x - (Screen.width / 2)) / (Screen.width / 2);
        float yFactor = (mousePos.y - (Screen.height / 2)) / (Screen.height / 2);

        // 2. Hedef pozisyonu hesapla (Farenin tersine hareket)
        Vector2 targetPos = startAnchoredPos - new Vector2(xFactor * moveAmount, yFactor * moveAmount);

        // 3. Yumuşak geçişle uygula
        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPos, Time.deltaTime * smoothness);
    }
}