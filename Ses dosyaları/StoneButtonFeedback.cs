using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StoneButtonFeedback : MonoBehaviour, IPointerDownHandler
{
    private RectTransform rect;
    private Vector2 originalPos;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalPos = rect.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Tıklandığında taşın ağır bir şekilde çöktüğünü hissettir.
        // Boyutu hafifçe küçültüp, pozisyonu 1-2 piksel aşağı kaydır.
        StartCoroutine(ClickSequence());
    }

    private System.Collections.IEnumerator ClickSequence()
    {
        // DARBE ANI (Impact): Boyutu ve pozisyonu değiştir.
        transform.localScale = Vector3.one * 0.95f; // %5 küçült
        rect.anchoredPosition = originalPos + new Vector2(0, -3f); // 3 pixel aşağı kaydır.

        yield return new WaitForSeconds(0.1f); // Çok kısa bekle.

        // ESKİ HALİNE DÖNME (Recovery): Yumuşak bir şekilde.
        // Not: Bu kısım önceki Parallax scriptiyle çakışmaması için tasarlanmıştır.
    }

}