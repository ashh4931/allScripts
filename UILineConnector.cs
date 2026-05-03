using UnityEngine;

public class UILineConnector : MonoBehaviour
{
    public RectTransform startSkill; // Başlangıç yeteneği
    public RectTransform endSkill;   // Hedef yeteneği
    public float thickness = 5f;     // Çizgi kalınlığı

    private RectTransform lineRect;

    void Start()
    {
        // Çizgi olarak kullanılacak Image objesini hazırla
        lineRect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (startSkill == null || endSkill == null) return;

        // İki skill arasındaki yönü ve mesafeyi hesapla
        Vector2 direction = endSkill.anchoredPosition - startSkill.anchoredPosition;
        float distance = direction.magnitude;

        // Çizgiyi konumlandır
        lineRect.anchoredPosition = startSkill.anchoredPosition + direction / 2;

        // Çizgiyi döndür
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        lineRect.rotation = Quaternion.Euler(0, 0, angle);

        // Çizginin boyunu ve kalınlığını ayarla
        lineRect.sizeDelta = new Vector2(distance, thickness);
    }
}