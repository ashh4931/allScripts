using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillTreeZoom : MonoBehaviour
{
    [Header("Zoom Ayarları")]
    public RectTransform content; 
    public float zoomSpeed = 0.1f;
    public float minZoom = 0.5f; // En uzak görünüm
    public float maxZoom = 2.0f; // En yakın görünüm

    [Header("Merkezleme Ayarları")]
    public ScrollRect scrollRect; 

    void OnEnable()
    {
        ResetView();
    }

    void Update()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0)
        {
            Zoom(scrollInput);
        }
    }

    void Zoom(float delta)
    {
        Vector3 newScale = content.localScale + Vector3.one * delta * zoomSpeed;

        newScale.x = Mathf.Clamp(newScale.x, minZoom, maxZoom);
        newScale.y = Mathf.Clamp(newScale.y, minZoom, maxZoom);
        newScale.z = 1;

        content.localScale = newScale;
    }

    public void ResetView()
    {
        // 1. Zoom seviyesini EN UZAK (minZoom) seviyesine çek
        if (content != null)
        {
            // X ve Y eksenlerini minZoom değerine eşitliyoruz
            content.localScale = new Vector3(minZoom, minZoom, 1f); 
        }

        // 2. Scroll'u tam merkeze (0.5) al
        if (scrollRect != null)
        {
            scrollRect.horizontalNormalizedPosition = 0.5f;
            scrollRect.verticalNormalizedPosition = 0.5f;
        }
    }
}