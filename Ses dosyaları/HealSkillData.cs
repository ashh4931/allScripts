using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Skill", menuName = "Skills/Heal Skill")]
public class HealSkillData : SkillData
{
    public float healAmount = 50f;

    public override void Cast(GameObject player)
    {
        // Oyuncunun üzerindeki Heal componentini bul ve çalıştır
        Heal healComponent = player.GetComponentInChildren<Heal>();
        if (healComponent != null)
        {
            healComponent.use(); // İçinde zaten ses ve VFX oynatma kodların var
        }
    }
}