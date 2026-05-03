using UnityEngine;

public class NPCEyeTracker : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Gözün etrafında döneceği merkez nokta (Örn: Çarkın tam ortası)")]
    public Transform centerPoint; 
    private Transform player;

    [Header("Yörünge (Orbit) Ayarları")]
    [Tooltip("Göz merkezden ne kadar uzakta duracak?")]
    public float radius = 0.5f; 
    [Tooltip("Gözün oyuncuyu takip etme hızı (Yumuşaklık)")]
    public float followSpeed = 10f;

    [Header("Rotasyon Ayarları")]
    [Tooltip("Gözün kendi ekseni etrafında oyuncuya dönmesini istiyor musun?")]
    public bool lookAtPlayer = true;

    void Start()
    {
        // Oyuncuyu tag üzerinden buluyoruz
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Eğer merkez nokta atanmamışsa, gözün bağlı olduğu ana objeyi (parent) merkez kabul et
        if (centerPoint == null && transform.parent != null)
        {
            centerPoint = transform.parent;
        }
    }

    void Update()
    {
        if (player == null || centerPoint == null) return;

        // 1. Oyuncunun merkeze olan yönünü hesapla
        Vector2 directionToPlayer = (player.position - centerPoint.position).normalized;

        // 2. Gözün gitmesi gereken hedef pozisyonu (Çemberin üzeri) hesapla
        Vector3 targetPosition = centerPoint.position + (Vector3)directionToPlayer * radius;

        // 3. Gözü o noktaya yumuşak bir şekilde kaydır (Lerp)
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

        // 4. (Opsiyonel) Göz bebeğinin/görselinin de oyuncuya doğru dönmesini sağla
        if (lookAtPlayer)
        {
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            // 2D'de sağa (X eksenine) bakan bir sprite için standart döndürme
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    
    // Editörde gözün dönebileceği çemberi (yörüngeyi) yeşil bir çizgiyle gösterir
    private void OnDrawGizmosSelected()
    {
        if (centerPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(centerPoint.position, radius);
        }
    }
}