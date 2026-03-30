using UnityEngine;
using UnityEngine.UI;

public class SkillTreeLine : MonoBehaviour
{
    [Header("Bağlantı Ayarları")]
    public SkillData targetSkill; // Bu çizgi HANGİ yeteneğe gidiyor?
    
    [Header("Görsel Ayarlar")]
    public Image lineImage;
    public Color lockedColor = new Color(0.2f, 0.2f, 0.2f, 1f); // Sönük, koyu renk
    public Color unlockedColor = new Color(1f, 0.8f, 0f, 1f);   // Parlak altın sarısı (İstediğin rengi seçebilirsin)

    void Start()
    {
        if (lineImage == null) lineImage = GetComponent<Image>();
    }

    void Update()
    {
        // Hedef yetenek açıldıysa çizgiyi parlat!
        if (targetSkill != null && lineImage != null)
        {
            if (targetSkill.isUnlocked)
            {
                lineImage.color = unlockedColor;
            }
            else
            {
                lineImage.color = lockedColor;
            }
        }
    }
}