using UnityEngine;

[CreateAssetMenu(fileName = "NewMagneticEnemyData", menuName = "Game/Enemy/New Magnetic NPC Data")]
public class NewMagneticEnemyData : NewBaseEnemyData
{
    [Header("Manyetik Ayarlar")]
    public float magneticRadius = 8f;
    public float pullForce = 8f;
    public float pushForce = 20f;

    [Header("Mesafe ve Hareket (YENİ)")]
    public float retreatDistance = 4f;     // Oyuncu bu kadar dibine girerse geri geri kaçar

    [Header("Zamanlamalar")]
    public float abilityDuration = 2f;
    public float abilityCooldown = 1.5f;   // (Bunu Inspector'dan düşürerek daha sık saldırtabilirsin)

    [Header("Yörünge ve Ekstralar")]
    public float orbitSpeed = 3f;
    public float orbitRadius = 1.5f;
    public GameObject bulletPrefab;
    public float bulletSpawnRate = 0.15f;  // (YENİ) Mermi atma sıklığı. Küçülttükçe daha sık atar!

    [Header("Görsel Ayarlar")]
    public Color pullColor = Color.blue;
    public Color pushColor = Color.red;
    [Header("Yetenek Sesleri")]
    public AudioClip rocketPunchSound;
    public AudioClip blackHoleSound;
    public AudioClip clapSound;
    public AudioClip shootSound; // 🔴 YENİ: Mermi fırlatma sesi
}