using UnityEngine;
using UnityEngine.Localization; // ÖNEMLİ: Bunu eklemezsen LocalizedString çalışmaz!

// Nadirlik seviyeleri için enum yapısı
public enum Rarity { Common, Uncommon, Rare, Epic, Legendary, Mythic }

public enum WeaponType { Gun, Melee, Wand }

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "ScriptableObjects/WeaponData")]
public class New_WeaponData : ScriptableObject
{
    [Header("General Settings (Localized)")]
    // ESKİ HALİ: public string weaponName;
    public LocalizedString weaponName; // Artık bir "Key" seçeceksin
    
    // ESKİ HALİ: public string description;
    public LocalizedString description; // Artık bir "Key" seçeceksin

    [Header("General Settings")]
    public Rarity rarity; 
    public WeaponType weaponType;
    
    [Header("Stats")]
    public int damage;
    public float attackSpeed;
    public Sprite weaponIcon;
    
    [Header("Melee Settings")]
    public float swingAngle = 120f;
    public float swingRadius = 1.5f;

    [Header("Visual Effects")]
    public GameObject rarityParticlePrefab;
}