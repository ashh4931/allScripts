using UnityEngine;

// Yetenek ağacında açılması için gereken seviye veya puan gibi verileri de buraya ekleyebilirsin
public abstract class SkillData : ScriptableObject
{


    [Header("Ön Koşullar (Prerequisites)")]
    [Tooltip("Bu yeteneği açmak için gereken TÜM yetenekleri buraya ekle")]
    public SkillData[] prerequisiteSkills; // 🔴 YENİ: Artık burası bir dizi (liste)!


    [Header("Temel Bilgiler")]
    public string skillName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Gereksinimler")]
    public float cooldown;
    public float manaCost;
    public float staminaCost;
    [Header("Yetenek Ağacı Ayarları")]
    public bool isUnlocked = false;       // Yetenek şu an açık mı? (Başlangıç skilleri için Inspector'dan true yap)
    public int unlockCost = 1;            // Açmak için kaç yetenek puanı (Skill Point) lazım?
    public SkillData prerequisiteSkill;   // (Opsiyonel) Bunu açmak için önce hangi skill açılmış olmalı?

    // Her yetenek bu fonksiyonu kendi mantığına göre DOLDURMAK ZORUNDA
    public abstract void Cast(GameObject player);
}