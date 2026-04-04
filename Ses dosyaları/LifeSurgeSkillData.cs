using UnityEngine;

[CreateAssetMenu(fileName = "New Life Surge Skill", menuName = "Skills/Life Surge Skill")]
public class LifeSurgeSkillData : SkillData
{
    [Header("Life Surge Ayarları")]
    public float healAmount = 250f;

    public override void Cast(GameObject player)
    {
        // Oyuncunun üzerindeki LifeSurge componentini bul
        LifeSurge lifeSurgeComponent = player.GetComponentInChildren<LifeSurge>();
        
        if (lifeSurgeComponent != null)
        {
            // Can miktarını ScriptableObject üzerinden gönderiyoruz!
            lifeSurgeComponent.Use(healAmount); 
        }
        else
        {
            Debug.LogWarning("Oyuncunun içinde LifeSurge scripti bulunamadı!");
        }
    }
}