using UnityEngine;

[CreateAssetMenu(fileName = "New Teleport Skill", menuName = "Skills/Teleport Skill")]
public class TeleportSkillData : SkillData
{
    public override void Cast(GameObject player)
    {
        // Oyuncunun üzerindeki Teleport scriptini bul ve çalıştır
        Teleport skillComponent = player.GetComponentInChildren<Teleport>();
        if (skillComponent != null)
        {
            skillComponent.Use();
        }
        else
        {
            Debug.LogWarning("Teleport scripti bulunamadı!");
        }
    }
}