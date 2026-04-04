using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    private StatController statController;

    [Header("Kuşanılmış Yetenek Slotları")]
    public SkillData slot1; // Örn: Q tuşu
    public SkillData slot2; // Örn: E tuşu
    public SkillData dashSlot; // Örn: Space tuşu

    // Cooldown takibini yapmak için bir sözlük (Dictionary)
    private Dictionary<SkillData, float> skillCooldowns = new Dictionary<SkillData, float>();

    void Awake()
    {
        statController = GetComponent<StatController>();
    }

    // Yeteneği Slot üzerinden tetikleme
    public void UseSkillInSlot(SkillData skillToUse)
    {
        if (skillToUse == null) return; // Slota yetenek atanmamışsa çık

        // 1. Cooldown Kontrolü
        if (skillCooldowns.ContainsKey(skillToUse))
        {
            if (Time.time < skillCooldowns[skillToUse])
            {
                Debug.Log($"{skillToUse.skillName} bekleme süresinde! Kalan zaman: {skillCooldowns[skillToUse] - Time.time:F1}");
                return;
            }
        }

        // 2. Mana ve Stamina Kontrolü
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats.mana < skillToUse.manaCost)
        {
            Debug.Log($"Yeterli Mana yok! Gerekli: {skillToUse.manaCost}");
            return;
        }
        if (stats.stamina < skillToUse.staminaCost)
        {
            Debug.Log($"Yeterli Stamina yok! Gerekli: {skillToUse.staminaCost}");
            return;
        }

        // 3. Bedelleri Öde
        statController.UseMana(skillToUse.manaCost);
        statController.UseStamina(skillToUse.staminaCost);

        // 4. Yeteneği Gerçekleştir (SkillData içindeki Cast çalışır)
        skillToUse.Cast(gameObject);

        // 5. Cooldown'ı Başlat
        skillCooldowns[skillToUse] = Time.time + skillToUse.cooldown;
    }
}