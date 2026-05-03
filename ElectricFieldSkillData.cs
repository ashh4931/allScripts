using UnityEngine;

[CreateAssetMenu(fileName = "New Electric Field Skill", menuName = "Skills/Electric Field")]
public class ElectricFieldSkillData : SkillData
{
    // ZORUNLU ESKİ METOD (Kullanmayacağız ama abstract olduğu için bulunmalı)
    public override void Cast(GameObject player)
    {
        // Boş bırakabilirsin, çünkü bu yetenek artık tuş bilgisi olmadan çalışamaz.
    }

    // YENİ EZDİĞİMİZ METOD: Tuş bilgisini alıp şarja gönderiyoruz
    public override void Cast(GameObject player, string keyName)
    {
        ElectricField skillComponent = player.GetComponentInChildren<ElectricField>();

        if (skillComponent != null)
        {
            if (skillComponent.IsActive)
            {
                skillComponent.StopSkill();
            }
            else
            {
                // ARTIK SABİT DEĞİL: Hangi tuştan geldiyse o tuşu şarja gönderiyoruz!
                skillComponent.BeginCharge(keyName); 
            }
        }
    }
}