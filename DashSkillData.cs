using UnityEngine;

[CreateAssetMenu(fileName = "New Dash Skill", menuName = "Skills/Dash Skill")]
public class DashSkillData : SkillData
{
    public override void Cast(GameObject player)
    {
        // Oyuncunun üzerindeki eski Dash.cs scriptini bul ve çalıştır!
        Dash dashComponent = player.GetComponentInChildren<Dash>();
        if (dashComponent != null)
        {
            dashComponent.use();
        }
    }
}