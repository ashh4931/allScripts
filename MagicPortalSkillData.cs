using UnityEngine;

[CreateAssetMenu(fileName = "New Magic Portal Skill", menuName = "Skills/Magic Portal Skill")]
public class MagicPortalSkillData : SkillData
{
    public override void Cast(GameObject player)
    {
        // Oyuncunun üzerindeki MagicPortal scriptini bul ve çalıştır
        MagicPortal skillComponent = player.GetComponentInChildren<MagicPortal>();
        
        if (skillComponent != null)
        {
            skillComponent.Use();
        }
        else
        {
            Debug.LogWarning("MagicPortal scripti bulunamadı! Player -> skills objesine eklendiğinden emin olun.");
        }
    }
}