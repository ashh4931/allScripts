using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    private StatController statController;

    [Header("Tüm Yetenek Veritabanı")]
    public List<SkillData> allSkills; // Projedeki TÜM ScriptableObject'leri buraya sürükle

    [Header("Kuşanılmış Yetenek Slotları")]
    public SkillData slot1;
    public SkillData slot2;
    public SkillData dashSlot;

    private Dictionary<SkillData, float> skillCooldowns = new Dictionary<SkillData, float>();

    void Start()
    {
        // OYUN BAŞLARKEN TÜM SKILLERİ SIFIRLAR (Save sorunu çözülür)
        foreach (var skill in allSkills)
        {
            if (skill != null) skill.ResetSkill();
        }
    }

    void Awake() => statController = GetComponent<StatController>();

    public void UseSkillInSlot(SkillData skillToUse)
    {
        if (skillToUse == null) return;

        if (skillCooldowns.ContainsKey(skillToUse))
        {
            if (Time.time < skillCooldowns[skillToUse])
            {
                // HATA DÜZELTME: skillName -> localizedName.GetLocalizedString()
                Debug.Log($"{skillToUse.localizedName.GetLocalizedString()} bekleme süresinde!");
                return;
            }
        }

        // ... (Mana/Stamina kontrolleri aynı kalacak)
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats.mana < skillToUse.manaCost || stats.stamina < skillToUse.staminaCost) return;

        statController.UseMana(skillToUse.manaCost);
        statController.UseStamina(skillToUse.staminaCost);
        skillToUse.Cast(gameObject);
        skillCooldowns[skillToUse] = Time.time + skillToUse.cooldown;
    }
}