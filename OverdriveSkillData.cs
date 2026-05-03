using UnityEngine;

[CreateAssetMenu(fileName = "New Overdrive Skill", menuName = "Skills/Overdrive Skill")]
public class OverdriveSkillData : SkillData
{
    [Header("Overdrive Özel Ayarları")]
    [Tooltip("Yetenek kaç saniye boyunca aktif kalacak?")]
    public float activeDuration = 5f;

    public override void Cast(GameObject player)
    {
        // Oyuncunun üzerindeki Overdrive componentini bul
        Overdrive overdriveComponent = player.GetComponentInChildren<Overdrive>();
        
        if (overdriveComponent != null)
        {
            // Belirlediğimiz süreyi (activeDuration) scriptin içine yolla ve çalıştır
            overdriveComponent.Use(activeDuration); 
        }
        else
        {
            Debug.LogWarning("Oyuncunun içinde Overdrive scripti bulunamadı!");
        }
    }
}