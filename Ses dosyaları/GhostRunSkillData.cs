using UnityEngine;

[CreateAssetMenu(fileName = "New GhostRun Skill", menuName = "Skills/GhostRun Skill")]
public class GhostRunSkillData : SkillData
{
    public override void Cast(GameObject player)
    {
        // Oyuncunun üzerindeki GhostRun scriptini bul ve çalıştır
        GhostRun skillComponent = player.GetComponentInChildren<GhostRun>();
        if (skillComponent != null)
        {
            skillComponent.Use();
        }
        else
        {
            Debug.LogWarning("GhostRun scripti bulunamadı!");
        }
    }
}