using UnityEngine;

[CreateAssetMenu(fileName = "New EarthQuake Skill", menuName = "Skills/EarthQuake Skill")]
public class EarthQuakeSkillData : SkillData
{
    public override void Cast(GameObject player)
    {
        EarthQuake eqComponent = player.GetComponentInChildren<EarthQuake>();
        if (eqComponent != null) eqComponent.use();
    }
}