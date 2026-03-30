using UnityEngine;

[CreateAssetMenu(fileName = "NewRangedEnemyData", menuName = "Game/Enemy/New Ranged NPC Data")]
public class NewRangedEnemyData : NewBaseEnemyData
{
    [Header("Menzilli Hareket Ayarları")]
    public float preferredDistance = 6f; // Oyuncuya bu mesafeden fazla yaklaşmayacak
    public float retreatDistance = 4f;   // Oyuncu çok dibine girerse geri kaçma mesafesi
    public float orbitSpeed = 2f;        // Etrafında dönerkenki hızı (Opsiyonel)

    [Header("Silah Ayarları")]
    public GameObject[] weaponPrefabs;   // Oyundaki silah prefablarını buraya atacaksın
    public float fireRate = 1.5f;        // Saniyede kaç mermi atacak?
   [Header("Silah Sesleri")]
    public AudioClip fireSound; 
    
    // 🔴 YENİ EKLENEN: Ateş sesini kısmak için özel ayar
    [Range(0f, 1f)] public float fireSoundVolume = 0.4f;

    
}