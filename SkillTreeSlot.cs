using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillTreeSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public SkillData skillData;
    public Image iconImage;

    [Header("Görsel Ayarlar")]
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    public Color unlockedColor = Color.white;

    // YENİ EKLENEN KISIM: Ses efekti değişkeni
    [Header("Ses Efektleri")]
    public AudioClip unlockSoundEffect;

    void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (skillData != null && !skillData.isAvailableInDemo)
        {
            gameObject.SetActive(false);
            return;
        }

        if (skillData != null && iconImage != null)
        {
            iconImage.sprite = skillData.icon;
            iconImage.color = skillData.isUnlocked ? unlockedColor : lockedColor;
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
        Debug.Log("Tıklanıyor");

        TryUnlockSkill();
    }

    private void TryUnlockSkill()
    {
        if (skillData == null)
        {
            Debug.LogError("<color=red>[SkillTreeSlot]</color> Kritik Hata: Bu slotun SkillData'sı atanmamış!");
            return;
        }

        string skillDisplayName = (skillData.localizedName != null && !skillData.localizedName.IsEmpty)
            ? skillData.localizedName.GetLocalizedString()
            : skillData.name;

        Debug.Log($"<color=white><b>[{skillDisplayName}]</b></color> için kilit açma süreci başladı...");

        if (!skillData.isAvailableInDemo)
        {
            Debug.LogWarning($"<color=yellow>[Demo Kısıtlaması]</color> {skillDisplayName} bu sürümde kilitli!");
            return;
        }

        if (skillData.prerequisiteSkills != null && skillData.prerequisiteSkills.Length > 0)
        {
            foreach (SkillData prereq in skillData.prerequisiteSkills)
            {
                if (prereq != null && !prereq.isUnlocked)
                {
                    string prereqName = (prereq.localizedName != null && !prereq.localizedName.IsEmpty)
                        ? prereq.localizedName.GetLocalizedString() : prereq.name;

                    Debug.LogWarning($"<color=red>[Ön Koşul Hatası]</color> {skillDisplayName} açmak için önce {prereqName} açılmalı!");
                    return; 
                }
            }
            Debug.Log("<color=green>[Ön Koşullar]</color> Tamamlandı.");
        }

        PlayerStats stats = FindObjectOfType<PlayerStats>();
        if (stats == null)
        {
            Debug.LogError("<color=red>[Hata]</color> Sahnede PlayerStats scripti bulunamadı!");
            return;
        }

        Debug.Log($"<color=cyan>[Cüzdan Kontrolü]</color> Gereken: {skillData.unlockCost}, Mevcut: {stats.money}");

        if (stats.money >= skillData.unlockCost)
        {
            // SATIN ALMA İŞLEMİ
            stats.money -= skillData.unlockCost;
            skillData.isUnlocked = true;

            Debug.Log($"<color=green><b>[BAŞARILI]</b></color> {skillDisplayName} açıldı! Kalan Para: {stats.money}");

            // YENİ EKLENEN KISIM: AudioManager ile sesi çal
            if (unlockSoundEffect != null && AudioManager.instance != null)
            {
                // UI sesi olduğu için transform.position veriyoruz, zaten AudioManager içindeki
                // spatialBlend = 0f ayarı bunu ekrana eşit dağıtılan 2D sese çevirecektir.
                AudioManager.instance.PlaySFXAtPosition(unlockSoundEffect, transform.position);
            }

            UpdateUI();
        }
        else
        {
            Debug.LogWarning($"<color=orange>[Yetersiz Para]</color> {skillDisplayName} için {(skillData.unlockCost - stats.money)} birim daha lazım.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (skillData != null && SkillTooltip.instance != null)
        {
            SkillTooltip.instance.ShowTooltip(skillData);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (SkillTooltip.instance != null)
        {
            SkillTooltip.instance.HideTooltip();
        }
    }
}