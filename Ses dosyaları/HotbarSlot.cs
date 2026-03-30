using UnityEngine;
using UnityEngine.UI;

public class HotbarSlot : MonoBehaviour
{
    public SkillData assignedSkill; 
    public Image iconImage;         
    
    [Header("Bekleme Süresi (Cooldown) UI")]
    public Image cooldownOverlay; // 🔴 YENİ: Inspector'dan yarattığın o yarı saydam siyah Image'i buraya sürükle!

    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    private float cooldownDuration = 0f;

    public void AssignSkill(SkillData newSkill)
    {
        assignedSkill = newSkill;
        
        if (assignedSkill != null)
        {
            iconImage.sprite = assignedSkill.icon; 
            iconImage.enabled = true; 
            
            // Atama yapıldığında overlay'i sıfırla (gizle)
            if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0f;
        }
        else
        {
            iconImage.enabled = false; 
            if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0f;
        }
    }

    // 🔴 YENİ: Dışarıdan tetiklenecek Cooldown Başlatma fonksiyonu
   public void StartCooldown(float duration)
    {
        // Bakalım kod buraya kadar geliyor mu ve süre kaç saniye geliyor?
        Debug.Log($"{assignedSkill.skillName} yeteneğinin UI animasyonu başladı! Süre: {duration}"); 
        
        isOnCooldown = true;
        cooldownDuration = duration;
        cooldownTimer = duration; 
    }
    void Update()
    {
        // Eğer yetenek beklemedeyse ve overlay'i bağladıysan çalışır
        if (isOnCooldown && cooldownOverlay != null)
        {
            cooldownTimer -= Time.deltaTime;
            
            // FillAmount 0 ile 1 arası değer alır. Kalan süreyi toplam süreye bölerek yüzdesini buluyoruz.
            cooldownOverlay.fillAmount = cooldownTimer / cooldownDuration;

            // Süre bittiyse sıfırla
            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                cooldownOverlay.fillAmount = 0f; 
            }
        }
    }
}