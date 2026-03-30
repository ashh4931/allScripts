using UnityEngine;

[CreateAssetMenu(fileName = "New StoneBody Skill", menuName = "Skills/StoneBody Skill")]
public class StoneBodySkillData : SkillData
{
    public override void Cast(GameObject player)
    {
        // Oyuncunun üzerindeki StoneBody scriptini bul ve çalıştır
        StoneBody skillComponent = player.GetComponentInChildren<StoneBody>();
        if (skillComponent != null)
        {
            skillComponent.use();
        }
        else
        {
            Debug.LogWarning("StoneBody scripti bulunamadı!");
        }
    }
}