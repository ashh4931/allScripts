using UnityEngine;

[CreateAssetMenu(fileName = "NewRocketEnemyData", menuName = "Game/Enemy/New Rocket NPC Data")]
public class NewRocketEnemyData : NewBaseEnemyData
{
    [Header("Atış ve Hedefleme Ayarları")]
    [Range(1, 10)] public int rocketsPerVolley = 4; // Bir saldırıda kaç roket atılacak?
    public float timeBetweenShots = 0.3f; // Roketler arası bekleme süresi
    public float attackCooldown = 3f; // Batarya kalktıktan sonraki bekleme süresi
    public float predictiveLeadTime = 1.5f; // Oyuncunun önüne ne kadar mesafe atılsın?

    [Header("Menzil ve Mesafe (Kiting)")]
    public float artilleryRange = 12f; // Saldırıya başlama menzili
    public float retreatDistance = 6f; // Oyuncu çok yaklaşırsa kaçma mesafesi

    [Header("Düşüş ve Gecikme Ayarları")]
    public float indicatorDelay = 1.5f; // Ateşlendikten kaç saniye sonra yerdeki kırmızı alan çıksın?
    public float fallDelay = 1f; // Kırmızı alan çıktıktan kaç saniye sonra roket düşüp patlasın?

    [Header("Patlama ve Hasar")]
    public float explosionRadius = 3f;
    public float minDamage = 5f;
    public float knockbackForce = 8f;

    [Header("Formasyon Çarpanları")]
    public float circleScatterRadius = 2.5f; // Çember formasyonunda roketlerin dağılma alanı
    public float lineSpacing = 2f; // Çizgi formasyonunda roketler arası mesafe

    [Header("Görsel Prefablar")]
    public GameObject upwardRocketPrefab; // Ekrandan yukarı çıkıp kaybolan görsel
    public GameObject strikeManagerPrefab; // Hedefte bekleyip patlamayı yönetecek obje
    public GameObject explosionVFX; // Patlama anında çıkacak efekt

    [Header("Sesler")]
    public AudioClip shootSound;
    public AudioClip indicatorSound; // Kırmızı alan çıkınca çalacak uyarı sesi
    public AudioClip explosionSound;
}