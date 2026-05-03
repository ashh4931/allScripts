using UnityEngine;

[CreateAssetMenu(fileName = "NewTurretData", menuName = "Player/Turret Data")]
public class PlayerTurretData : ScriptableObject
{
    [Header("Temel Bilgiler")]
    public string turretName = "Temel Taret";
    public int price = 100;

    [Header("Savaş Ayarları")]
    public GameObject turretPrefab; // Görsel ve namlunun olduğu obje
    public GameObject bulletPrefab;
    public float fireRate = 0.5f;   // Kaç saniyede bir ateş eder?
    public float range = 8f;        // Düşmanı görme menzili

    [Header("Sesler")]
    public AudioClip fireSound;
}