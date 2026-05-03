using UnityEngine;

[CreateAssetMenu(fileName = "NewBombEnemyData", menuName = "Game/Enemy/New Bomb NPC Data")]
public class NewBombEnemyData : NewBaseEnemyData 
{
    [Header("Explosion Zamanlama Ayarları")]
    public float heatUpTime = 2.0f;        // Kırmızıya dönme süresi
    public float detonationDelay = 0.5f;   // Kırmızı olduktan sonra patlama beklemesi

    [Header("Explosion Etki Ayarları")]
    public float explosionRadius = 4f;     // Hasar vereceği alanın çapı
    public float knockbackForce = 10f;     // İtme gücü miktarı
    public float minDamage = 10f;          // Merkezden uzaklaştıkça düşecek minimum hasar

    [Header("Explosion Görsel ve Ses")]
    public Color warningColor = Color.red; 
    public GameObject explosionEffect;     
    public AudioClip explosionSound;       
    public AudioClip knockbackSound;
}