using UnityEngine;

[CreateAssetMenu(fileName = "NewSnakeEnemyData", menuName = "Game/Enemy/Snake Data")]
public class NewSnakeEnemyData : NewBaseEnemyData
{
    [Header("Yılan Parçaları")]
    public GameObject[] bodyPrefabs;
    public GameObject tailPrefab;

    [Header("Mekanik Ayarlar")]
    public int segmentCount = 8;
    public float segmentGap = 0.6f;        // Parçalar arası mesafe (Görsele göre ayarla)
    public float enrageSpeedMultiplier = 1.1f; // Her parça ölünce hız artışı
    public float segmentHealthMultiplier = 0.5f;

    [Header("Saldırı ve Manevra")]
    public float strikeOvershoot = 12f;     // Oyuncunun ne kadar arkasına uçsun?
    public float retreatDistance = 15f;     // Kaçarken ne kadar uzağa gitsin?
    public float idleWaitTime = 2f;         // Tekrar saldırmadan önce bekleme
    public float turnSpeed = 150f;          // Dönüş çevikliği (Düşük = Daha geniş kavis)

    [Header("Görsel Organik Efektler")]
    public float pulseSpeed = 5f;           // Nefes alma hızı
    public float pulseAmount = 0.15f;       // Şişme miktarı (%15)

    [Header("Görsel ve Ses")]
    public GameObject explosionEffect;
    public AudioClip explosionSound;
}