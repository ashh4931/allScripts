using UnityEngine;

[CreateAssetMenu(fileName = "New Storm Verdict", menuName = "Skills/Storm Verdict")]
public class StormVerdictSkillData : SkillData
{
    public float damage = 50f;
    public float circleEffectRadius = 2.5f;

    public override void Cast(GameObject player)
    {
        StormVerdictSkill skillComponent = player.GetComponentInChildren<StormVerdictSkill>();
        if (skillComponent != null)
        {
            skillComponent.Use(damage, circleEffectRadius);
        }
    }
}