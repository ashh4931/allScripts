using UnityEngine;

[CreateAssetMenu(fileName = "New Double Dash Skill", menuName = "Skills/Double Dash Skill")]
public class DoubleDashSkillData : SkillData
{
    [Header("Double Dash Özel Ayarları")]
    [Tooltip("İkinci dash'i de attıktan sonra başlayacak olan asıl bekleme süresi")]
    public float realCooldown = 5f; 
    
    [Tooltip("İlk atıldıktan sonra, ikincisini atmak için oyuncunun kaç saniyesi var?")]
    public float comboWindow = 2f; 

    private int dashCount = 0;
    private float lastDashTime = -10f;

    public override void Cast(GameObject player)
    {
        // 1. KOMBO KONTROLÜ: Eğer ilk atıştan sonra çok fazla zaman (comboWindow) geçtiyse, komboyu sıfırla
        if (Time.time > lastDashTime + comboWindow)
        {
            dashCount = 0;
        }

        dashCount++;
        lastDashTime = Time.time;

        // 2. AKSİYON: Fiziksel dash'i attır
        DoubleDash skillComponent = player.GetComponentInChildren<DoubleDash>();
        if (skillComponent != null) skillComponent.use();

        // 3. SİHİRLİ KISIM (Cooldown Ayarlama)
        if (dashCount == 1)
        {
            // İLK ATIŞ: Hotbar'ın bizi engellememesi için cooldown'ı anlık olarak 0.1 saniye yapıyoruz.
            // Böylece oyuncu hemen 2. kere basabiliyor.
            this.cooldown = 0.1f; 
        }
        else
        {
            // İKİNCİ ATIŞ: Artık 2 hakkı da bitti, asıl uzun cooldown'ı başlat ve komboyu sıfırla!
            this.cooldown = realCooldown;
            dashCount = 0;
        }
    }

    // Güvenlik: Oyun başladığında bekleme süresi her ihtimale karşı normale dönsün
    private void OnEnable()
    {
        this.cooldown = realCooldown;
        dashCount = 0;
    }
}