using UnityEngine;
using TMPro; // 🔴 YENİ EKLENDİ: TextMeshPro kütüphanesi

public class SkillTooltip : MonoBehaviour
{
    public static SkillTooltip instance;

    [Header("UI Referansları")]
    // 🔴 DEĞİŞTİ: Bütün 'Text' kelimeleri 'TextMeshProUGUI' oldu!
    public TextMeshProUGUI nameText;           
    public TextMeshProUGUI descriptionText;    
    public TextMeshProUGUI statusText;         
    public TextMeshProUGUI costText;           
    public TextMeshProUGUI statsText;          

    void Awake() 
    {
        instance = this;
        gameObject.SetActive(false); 
    }

    public void ShowTooltip(SkillData skill)
    {
        gameObject.SetActive(true); 
        
        if(nameText != null) nameText.text = skill.skillName;
        if(descriptionText != null) descriptionText.text = skill.description;
        
        if(statusText != null)
        {
            if(skill.isUnlocked)
            {
                statusText.text = "Öğrenildi";
                statusText.color = Color.green; 
            }
            else
            {
                statusText.text = "Kilitli";
                statusText.color = Color.red;   
            }
        }

        if(costText != null)
        {
            if(skill.isUnlocked) costText.text = "";
            else costText.text = $"{skill.unlockCost} $";
        }

        if(statsText != null)
        {
            statsText.text = $"Mana: {skill.manaCost}  |  Bekleme: {skill.cooldown} sn";
        }
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false); 
    }

    void Update() 
    {
        // Fareyi takip et
        transform.position = Input.mousePosition + new Vector3(20f, -20f, 0f);
    }
}