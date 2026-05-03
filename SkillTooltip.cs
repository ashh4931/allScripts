using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings; // 🔴 YENİ: Tablodan manuel veri çekmek için gerekli

public class SkillTooltip : MonoBehaviour
{
    public static SkillTooltip instance;

    [Header("Ayarlar")]
    public string tableName = "AnaDilTablosu"; // 🔴 Localization tablonun adını buraya tam yaz!

    [Header("UI Referansları")]
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

        // Zaten ScriptableObject üzerinden gelen metinler (Doğru kullanım)
        if (nameText != null) nameText.text = skill.localizedName.GetLocalizedString();
        if (descriptionText != null) descriptionText.text = skill.localizedDescription.GetLocalizedString();

        // 🔴 YENİ: Status (Kilitli/Açık) kısmını tablodan çekme
        if (statusText != null)
        {
            if (skill.isUnlocked)
            {
                // Tablodaki "skill_Unlocked" anahtarını bulur
                statusText.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "skill_Unlocked");
                statusText.color = Color.green;
            }
            else
            {
                // Tablodaki "skill_locked" anahtarını bulur
                statusText.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "skill_locked");
                statusText.color = Color.red;
            }
        }

        if (costText != null)
        {
            if (skill.isUnlocked) costText.text = "";
            else costText.text = $"{skill.unlockCost} $";
        }

        // 🔴 YENİ: Stats (Mana/Bekleme) kısmını tablodan çekme
        if (statsText != null)
        {
            string manaLabel = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "Mana");
            string cooldownLabel = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "coolDown");

            statsText.text = $"{manaLabel}: {skill.manaCost}  |  {cooldownLabel}: {skill.cooldown} sn";
        }
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        transform.position = Input.mousePosition + new Vector3(20f, -20f, 0f);
    }
}