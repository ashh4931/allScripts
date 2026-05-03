using UnityEngine;
using UnityEngine.Localization; // Localization kütüphanesi şart

public abstract class SkillData : ScriptableObject
{
    [Header("Demo & Sistem Ayarları")]
    public bool isAvailableInDemo = true; // Demoda açık mı?
    public bool isUnlocked = false;       // Mevcut durum (Editor'de değişebilir)
    [Tooltip("Oyun her başladığında 'isUnlocked' bu değere döner.")]
    public bool unlockedByDefault = false; // Başlangıçta açık mı?

    [Header("Ön Koşullar")]
    public SkillData[] prerequisiteSkills;

    [Header("Temel Bilgiler (Yerelleştirilmiş)")]
    public LocalizedString localizedName;        // Name -> localizedName oldu
    public LocalizedString localizedDescription; // description -> localizedDescription oldu
    public Sprite icon;

    [Header("Gereksinimler")]
    public float cooldown;
    public float manaCost;
    public float staminaCost;
    public int unlockCost = 1;

    // Oyun başlarken veya dil değişince veriyi sıfırlamak için
    
    public void ResetSkill()
    {
        isUnlocked = unlockedByDefault;
    }

    public abstract void Cast(GameObject player);
    public virtual void Cast(GameObject player, string keyName)
    {
        // Eğer alt sınıfta bu metod ezilmezse, varsayılan olarak eskisini çalıştır.
        // Böylece IronBody vs. bozulmaz!
        Cast(player); 
    }
}