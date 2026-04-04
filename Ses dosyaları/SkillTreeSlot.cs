using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // FARE HAREKETLERİNİ DİNLEMEK İÇİN GEREKLİ

// IPointerEnterHandler ve IPointerExitHandler fare üstüne gelince ve çıkınca tetiklenir
public class SkillTreeSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler 
{
    public SkillData skillData; 
    public Image iconImage;     
    
    [Header("Görsel Ayarlar")]
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f); 
    public Color unlockedColor = Color.white;                   

    void Start() 
    {
        UpdateUI();
    }

    public void UpdateUI() 
    {
        if(skillData != null && iconImage != null)
        {
            iconImage.sprite = skillData.icon;
            
            if (skillData.isUnlocked) iconImage.color = unlockedColor;
            else iconImage.color = lockedColor;
        }
    }

    public void OnSkillClicked()
    {
        if (skillData == null) return;

        if (skillData.isUnlocked)
        {
            FindObjectOfType<HotbarManager>().PrepareToAssign(skillData);
            return;
        }

        TryUnlockSkill();
    }

    private void TryUnlockSkill()
    {
        if (skillData.prerequisiteSkills != null && skillData.prerequisiteSkills.Length > 0)
        {
            foreach (SkillData prereq in skillData.prerequisiteSkills)
            {
                if (prereq != null && !prereq.isUnlocked)
                {
                    Debug.Log($"Önce {prereq.skillName} yeteneğini açmalısın!");
                    return; 
                }
            }
        }

        PlayerStats stats = FindObjectOfType<PlayerStats>();
        if (stats != null && stats.money >= skillData.unlockCost)
        {
            stats.money -= skillData.unlockCost;
            skillData.isUnlocked = true;
            UpdateUI();
        }
    }

 

    // --- BU KISIMLARI SKILLTREESLOT SCRIPTININ EN ALTINA EKLE ---

    // Fare butonun ÜSTÜNE GELDİĞİNDE çalışır
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (skillData != null && SkillTooltip.instance != null)
        {
            SkillTooltip.instance.ShowTooltip(skillData);
        }
    }

    // Fare butonun ÜSTÜNDEN ÇIKTIĞINDA çalışır
    public void OnPointerExit(PointerEventData eventData)
    {
        if (SkillTooltip.instance != null)
        {
            SkillTooltip.instance.HideTooltip();
        }
    }
}