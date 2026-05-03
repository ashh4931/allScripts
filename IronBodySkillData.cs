using UnityEngine;

[CreateAssetMenu(fileName = "New IronBody Skill", menuName = "Skills/IronBody Skill")]
public class IronBodySkillData : SkillData
{
    public override void Cast(GameObject player)
    {
        // Oyuncunun üzerindeki IronBody scriptini bul ve çalıştır
        IronBody skillComponent = player.GetComponentInChildren<IronBody>();
        if (skillComponent != null)
        {
            skillComponent.use();
        }
        else
        {
            Debug.LogWarning("IronBody scripti bulunamadı!");
        }
    }
}