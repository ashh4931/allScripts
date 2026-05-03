using UnityEngine;
using UnityEngine.UI;

public class SkillTreeLine : MonoBehaviour
{
    [Header("Bağlantı Ayarları")]
    [Tooltip("Bu yolu açan ana yetenek (Ebeveyn)")]
    public SkillData sourceSkill; 
    
    [Tooltip("Bu hattın ulaştığı tüm yetenekler (Birden fazla olabilir)")]
    public SkillData[] targetSkills; 

    [Header("Görsel Ayarlar")]
    public Image lineImage;
    public Color lockedColor = new Color(0.15f, 0.15f, 0.15f, 1f); // Tamamen kapalı
    public Color pathOpenColor = new Color(0.4f, 0.4f, 0.4f, 1f);   // Yol açık (Ebeveyn açıldı ama yetenek alınmadı)
    public Color activeColor = new Color(1f, 0.8f, 0f, 1f);       // Yol aktif (Hedeflerden en az biri açıldı)

    void Start()
    {
        if (lineImage == null) lineImage = GetComponent<Image>();
    }

    void Update()
    {
        if (lineImage == null) return;

        // 1. Durum: Hedef yeteneklerden herhangi biri açıldıysa hat tam parlar (Aktif)
        if (IsAnyTargetUnlocked())
        {
            lineImage.color = activeColor;
        }
        // 2. Durum: Ebeveyn yetenek açıldıysa yol 'erişilebilir' görünür (Yol Açık)
        else if (sourceSkill != null && sourceSkill.isUnlocked)
        {
            lineImage.color = pathOpenColor;
        }
        // 3. Durum: Ebeveyn bile açılmadıysa hat sönüktür (Kilitli)
        else
        {
            lineImage.color = lockedColor;
        }
    }

    private bool IsAnyTargetUnlocked()
    {
        if (targetSkills == null || targetSkills.Length == 0) return false;

        foreach (var skill in targetSkills)
        {
            if (skill != null && skill.isUnlocked) return true;
        }
        return false;
    }
}