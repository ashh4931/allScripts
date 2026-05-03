using UnityEngine;

[CreateAssetMenu(fileName = "New Spirit Blades", menuName = "Skills/Spirit Blades")]
public class SpiritBladesData : SkillData
{
    [Header("Ruhani Kılıç Ayarları")]
    [Tooltip("Çağrılacak kılıç sayısı")]
    public int bladeCount = 3;
    
    [Tooltip("Kılıçlar çağrıldıktan sonra kaç saniye boyunca kalacak?")]
    public float activeDuration = 15f;

    public override void Cast(GameObject player)
    {
        // Oyuncunun (Player -> skills) altındaki fırlatıcı scripti bul
        SpiritBladesSkill skillComponent = player.GetComponentInChildren<SpiritBladesSkill>();
        
        if (skillComponent != null)
        {
            // Kılıçları Data'dan gelen sayıyla ve süreyle yarat
            skillComponent.Use(bladeCount, activeDuration);
        }
        else
        {
            Debug.LogWarning("SpiritBladesSkill scripti bulunamadı! Player -> skills objesine eklediğinden emin ol.");
        }
    }
}