using UnityEngine;

// 🔴 LOOT MANTIĞI İÇİN YENİ BİR STRUCT (YAPI)
[System.Serializable]
public class EnemyLoot
{
    public GameObject itemPrefab; // Düşecek obje (Altın, Can iksiri vs.)
    [Range(0f, 100f)] public float dropChance; // Düşme ihtimali (%0 - %100)
}

[CreateAssetMenu(fileName = "NewBaseEnemyData", menuName = "Game/Enemy/Base Data")]
public class NewBaseEnemyData : ScriptableObject
{
    [Header("Temel İstatistikler")]
    public float maxHealth = 20f;
    public float moveSpeed = 3f;
    public float baseDamage = 10f;

    [Header("Algılama ve Saldırı")]
    public float detectionRadius = 10f; 
    public float attackRange = 1.5f;    
    
    [Header("Durumlar")]
    public bool canMove = true;
    public bool startFrozen = false;

    // 🔴 YENİ EKLENEN ORTAK SES VE LOOTLAR
    [Header("Ortak Sesler")]
    public AudioClip hurtSound;   // Hasar yeme sesi
    public AudioClip deathSound;  // Ölüm sesi

    [Header("Ganimet (Loot) Sistemi")]
    public EnemyLoot[] lootTable; // Bu düşman ölünce neler düşürebilir?
    public float expValue = 20f;
}