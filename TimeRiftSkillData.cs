using UnityEngine;

[CreateAssetMenu(fileName = "New Time Rift Skill", menuName = "Skills/Time Rift Skill")]
public class TimeRiftSkillData : SkillData
{
    public override void Cast(GameObject player)
    {
        // Oyuncunun (Player -> skills) altındaki fırlatıcı scripti bul
        TimeRiftSkill skillComponent = player.GetComponentInChildren<TimeRiftSkill>();
        
        if (skillComponent != null)
        {
            skillComponent.Use();
        }
        else
        {
            Debug.LogWarning("TimeRiftSkill scripti bulunamadı! Player -> skills objesine eklediğinden emin ol.");
        }
    }
}