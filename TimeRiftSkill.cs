using UnityEngine;

public class TimeRiftSkill : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Üzerinde TimeRiftAura scripti olan prefab")]
    public GameObject timeRiftPrefab;
    
    [Header("Ayarlar")]
    [Tooltip("Yırtık oyuncunun tam üstünde mi açılsın, yoksa farenin olduğu yerde mi?")]
    public bool spawnAtMouse = false;

    public void Use()
    {
        if (timeRiftPrefab == null)
        {
            Debug.LogWarning("Time Rift Prefab'ı atanmamış!");
            return;
        }

        Vector3 spawnPos = transform.position; // Varsayılan olarak oyuncunun pozisyonu

        // Eğer istersen yırtığı farenin olduğu yere de atabilirsin
        if (spawnAtMouse)
        {
            spawnPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            spawnPos.z = 0f;
        }

        // Yırtığı yarat (Zaman yavaşlatma işini prefabın içindeki TimeRiftAura halledecek)
        Instantiate(timeRiftPrefab, spawnPos, Quaternion.identity);
    }
}