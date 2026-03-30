using UnityEngine;

[CreateAssetMenu(fileName = "NewGhostEnemyData", menuName = "Game/Enemy/Ghost Data")]
public class NewGhostEnemyData : NewBaseEnemyData
{
    [Header("Ses Ayarları")]
    public AudioClip dashSound; // Atılma (saldırı) sesi
    [Header("Hayalet Saldırı Ayarları")]
    public float dashSpeed = 20f;          // İçinden geçerkenki atılma hızı
    public float dashOvershoot = 30f;       // Oyuncunun ne kadar arkasına kadar uçacak?
    public float retreatDistance = 30f;     // Saldırdıktan sonra oyuncudan ne kadar uzağa kaçacak?

    [Header("Öfke (Enrage) Mekaniği")]
    public float normalCooldown = 3f;      // Normal bekleme süresi
    public float enragedCooldown = 0.5f;   // Öfkelendiğindeki bekleme süresi (Nefes aldırmaz!)
    public float enrageTimeThreshold = 15f;// Kaç saniye hayatta kalırsa öfkelenecek?

    [Header("Görsel Ayarlar")]
    public float fogWobbleSpeed = 2f;      // Sis parçalarının dalgalanma hızı
    public float fogWobbleAmount = 0.3f;   // Sis parçalarının ne kadar geniş dalgalanacağı
}