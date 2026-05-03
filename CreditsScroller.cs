using UnityEngine;

public class CreditsScroller : MonoBehaviour
{
    public float scrollSpeed = 50f; // Kayma hızı
    public float startYPosition = -600f; // Yazının başlayacağı alt nokta
    public float resetYPosition = 1500f; // Yazının duracağı/sıfırlanacağı üst nokta
    
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Credits paneli her açıldığında yazıyı başlangıç pozisyonuna getirir
    void OnEnable()
    {
        Vector2 pos = rectTransform.anchoredPosition;
        pos.y = startYPosition;
        rectTransform.anchoredPosition = pos;
    }

    void Update()
    {
        // Yazıyı her frame'de yukarı kaydır
        Vector2 pos = rectTransform.anchoredPosition;
        pos.y += scrollSpeed * Time.deltaTime;
        rectTransform.anchoredPosition = pos;

        // Belirli bir yüksekliğe ulaşınca (isteğe bağlı) başa dönebilir veya durabilir
        /*
        if (pos.y > resetYPosition) {
            pos.y = startYPosition;
            rectTransform.anchoredPosition = pos;
        }
        */
    }
}