using UnityEngine;

// Unity'de sağ tıklayıp rahatça üretebilmen için menüye ekliyoruz
[CreateAssetMenu(fileName = "NewTriShieldData", menuName = "Game/Enemy/TriShieldData")]
public class TriShieldEnemyData : NewBaseEnemyData
{
    [Header("Saldırı (Ateş) Ayarları")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;

    [Header("Zamanlamalar (Pattern)")]
    public float defenseTime = 5f;     // Aşama A süresi
    public float prepTime = 1.5f;      // Aşama B süresi
    public float attackTime = 1f;      // Aşama C süresi
    public float cooldownTime = 1f;    // Aşama D süresi

    [Header("Görsel Ayarlar")]
    public float fastRotationSpeed = 150f; 
    public Color defenseColor = Color.cyan;
    public Color warningColor = new Color(1f, 0.5f, 0f); // Turuncu
}