using UnityEngine;

[CreateAssetMenu(fileName = "New LifeOverflow Skill", menuName = "Skills/LifeOverflow Skill")]
public class LifeOverflowSkillData : SkillData
{
    public override void Cast(GameObject player)
    {
        // Oyuncunun üzerindeki LifeOverflow scriptini bul ve çalıştır
        LifeOverflow skillComponent = player.GetComponentInChildren<LifeOverflow>();
        if (skillComponent != null)
        {
            skillComponent.use(); // Senin scriptinde baş harfi küçük
        }
        else
        {
            Debug.LogWarning("LifeOverflow scripti bulunamadı!");
        }
    }
}