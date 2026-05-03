using UnityEngine;
using System.Collections;

public class MagicPortal : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Büyü çemberinin merkez alacağı oyuncu üzerindeki pivot")]
    public Transform pivotPoint;
    public GameObject portalPrefab;
    public GameObject projectilePrefab;

    [Header("Büyü Çemberi Ayarları")]
    [Tooltip("Çember, pivottan ne kadar uzağa açılacak?")]
    public float spawnDistance = 2.5f;
    [Tooltip("Çember açıldıktan sonra ateş etmeye başlamadan önce geçecek süre")]
    public float delayBeforeFire = 1f;
    [Tooltip("3D Partikülün eksen kaymasını düzeltmek için Z açısı ofseti")]
    public float zRotationOffset = -90f;

    [Header("Atış Ayarları")]
    [Tooltip("Çemberden çıkacak mermi sayısı")]
    public int projectileCount = 5;
    [Tooltip("Mermilerin art arda çıkış süresi. Hepsini aynı anda atmak için 0 yapın.")]
    public float timeBetweenShots = 0.15f;

    [Header("Ses Ayarları")]
    public AudioClip portalOpenSound;
    public AudioClip shootSound;

    public void Use()
    {
        Transform origin = pivotPoint != null ? pivotPoint : transform;

        // 1. Fare yönünü bul
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3 direction = (mousePos - origin.position).normalized;

        // 2. Çemberin açılacağı konumu hesapla
        Vector3 portalPos = origin.position + direction * spawnDistance;

        // 3. Çemberi Yarat (Önce parentsız yaratıyoruz ki prefabın kendi boyutu gelsin)
        GameObject portal = Instantiate(portalPrefab, portalPos, Quaternion.identity);
        
        // --- BOYUT KORUMA VE CHİLD YAPMA ---
        // 'true' parametresi sayesinde kendi orijinal scale değerini korur
        portal.transform.SetParent(origin, true);
        
        // --- ÖZEL ROTASYON ---
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float finalAngle = baseAngle + zRotationOffset;
        Vector3 currentRot = portal.transform.eulerAngles;
        portal.transform.rotation = Quaternion.Euler(currentRot.x, currentRot.y, finalAngle);
        // ------------------------------------------------------------

        // 4. Portal açılma sesini çal
        if (portalOpenSound != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFXAtPosition(portalOpenSound, portalPos);
        }

        // 5. Mermileri fırlatma sürecini başlat
        StartCoroutine(FireRoutine(portal, direction));
    }

    private IEnumerator FireRoutine(GameObject portal, Vector3 direction)
    {
        yield return new WaitForSeconds(delayBeforeFire);

        for (int i = 0; i < projectileCount; i++)
        {
            // Eğer portal herhangi bir sebeple yok olduysa işlemi durdur
            if (portal == null) break;

            // Mermiyi portalın GÜNCEL pozisyonunda yarat
            GameObject bullet = Instantiate(projectilePrefab, portal.transform.position, Quaternion.identity);
            
            bullet.transform.right = direction; 

            if (shootSound != null && AudioManager.instance != null)
            {
                // Sesi portalın güncel pozisyonunda çal
                AudioManager.instance.PlaySFXAtPosition(shootSound, portal.transform.position);
            }

            if (timeBetweenShots > 0)
            {
                yield return new WaitForSeconds(timeBetweenShots);
            }
        }

        if (portal != null) Destroy(portal, 0.5f); 
    }
}