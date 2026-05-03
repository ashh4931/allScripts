using UnityEngine;

[CreateAssetMenu(fileName = "New Double Teleport Skill", menuName = "Skills/Double Teleport Skill")]
public class DoubleTeleportSkillData : SkillData
{
    [Header("Double Teleport Özel Ayarları")]
    [Tooltip("İkinci ışınlanmayı da attıktan sonra başlayacak olan asıl bekleme süresi")]
    public float realCooldown = 8f; 
    
    [Tooltip("İlk ışınlanmadan sonra, ikincisini atmak için oyuncunun kaç saniyesi var?")]
    public float comboWindow = 2f; 

    private int teleportCount = 0;
    private float lastTeleportTime = -10f;

    public override void Cast(GameObject player)
    {
        // 1. KOMBO KONTROLÜ: Süre geçtiyse komboyu sıfırla
        if (Time.time > lastTeleportTime + comboWindow)
        {
            teleportCount = 0;
        }

        teleportCount++;
        lastTeleportTime = Time.time;

        // 2. AKSİYON: Oyuncudaki DoubleTeleport scriptini bul ve çalıştır
        DoubleTeleport skillComponent = player.GetComponentInChildren<DoubleTeleport>();
        if (skillComponent != null) 
        {
            skillComponent.Use();
        }
        else 
        {
            Debug.LogWarning("Oyuncuda DoubleTeleport scripti bulunamadı!");
        }

        // 3. COOLDOWN AYARLAMA MANTIĞI
        if (teleportCount == 1)
        {
            // İlk kullanım: Oyuncu hemen 2. kez atabilsin diye bekleme süresi çok kısa
            this.cooldown = 0.1f; 
        }
        else
        {
            // İkinci kullanım: 2 hak bitti, asıl uzun bekleme süresini başlat
            this.cooldown = realCooldown;
            teleportCount = 0;
        }
    }

    // Güvenlik: Oyun başladığında veya script yeniden yüklendiğinde resetle
    private void OnEnable()
    {
        this.cooldown = realCooldown;
        teleportCount = 0;
    }
}   