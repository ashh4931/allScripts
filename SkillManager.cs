using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [Header("Tüm Yetenek Listesi")]
    [Tooltip("Projedeki tüm SkillData dosyalarını (Dash, DoubleDash vb.) buraya sürükle.")]
    public SkillData[] allSkills;

    private void Awake()
    {
        ResetAllSkills();
    }

    /// <summary>
    /// Oyun başladığında tüm yetenekleri başlangıç ayarlarına (unlockedByDefault) döndürür.
    /// </summary>
    public void ResetAllSkills()
    {
        if (allSkills == null || allSkills.Length == 0)
        {
           // Debug.LogWarning("<color=orange>[SkillManager]</color> Hiç yetenek atanmadı! Lütfen Inspector üzerinden SkillData'ları listeye ekleyin.");
            return;
        }

        foreach (SkillData skill in allSkills)
        {
            if (skill != null)
            {
                // SkillData.cs içindeki ResetSkill metodunu çağırır
                skill.ResetSkill(); 
               // Debug.Log($"<color=green>[SkillManager]</color> {skill.name} sıfırlandı. (Kilit Açık mı: {skill.isUnlocked})");
            }
        }
    }
}