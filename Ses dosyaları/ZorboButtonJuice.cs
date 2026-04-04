using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ZorboButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rect;
    private Vector2 originalPos;
    private Vector3 originalScale;

    [Header("Mıknatıs ve Büyüme")]
    public float scaleMultiplier = 1.15f; 
    public float moveStrength = 40f;    // Bunu 50-60 yaparak etkini artırabilirsin
    public float lerpSpeed = 12f;

    private bool isHovering = false;
    private Vector2 targetPos;
    private Vector3 targetScale;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalPos = rect.anchoredPosition;
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (isHovering)
        {
            targetScale = originalScale * scaleMultiplier;

            // 1. Butonun dünya (ekran) merkezini bul
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector2 buttonCenter = (corners[0] + corners[2]) / 2f;

            // 2. Fare ile merkez arasındaki farkı bul
            Vector2 mousePos = Input.mousePosition;
            Vector2 dir = (mousePos - buttonCenter);

            // 3. Bu farkı moveStrength ile sınırlayarak orijinal pozisyona ekle
            // Clamp kullanarak butonun çok uzaklara kaçmasını engelliyoruz
            float dist = Mathf.Clamp(dir.magnitude, 0, moveStrength);
            targetPos = originalPos + (dir.normalized * dist);
        }
        else
        {
            targetPos = originalPos;
            targetScale = originalScale;
        }

        // Yumuşak geçiş (Smooth transition)
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPos, Time.deltaTime * lerpSpeed);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * lerpSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData) => isHovering = true;
    public void OnPointerExit(PointerEventData eventData) => isHovering = false;

    public void OnPointerDown(PointerEventData eventData) 
    {
        // Tıklandığında anlık küçülme (Feedback)
        transform.localScale = originalScale * 0.95f;
    }

    public void OnPointerUp(PointerEventData eventData) { }
}