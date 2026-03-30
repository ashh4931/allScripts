using UnityEngine;

[CreateAssetMenu(fileName = "NewSplitterData", menuName = "Game/Enemy/Splitter")]
 
public class SplitterEnemyData : NewBaseEnemyData
{
    [Header("Bölünme (Split) Ayarları")]
    public GameObject nextStagePrefab; 
    public int spawnCount = 2;         

    [Header("Yakın Dövüş (Melee) Ayarları")]
    public float meleeDamage = 15f;     
    public float attackCooldown = 1f;   
    
    // 🔴 YENİ: Yakın dövüş (Çarpma/Isırma) sesi
    public AudioClip meleeSound; 
}