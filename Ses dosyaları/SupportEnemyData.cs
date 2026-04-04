using UnityEngine;

[CreateAssetMenu(fileName = "NewSupportData", menuName = "Game/Enemy/Support Enemy Data")]
public class SupportEnemyData : NewBaseEnemyData
{
    [Header("Heal (Destek) Ayarları")]
    public float healRange = 8f;          // Heal yapabileceği alan yarıçapı
    public float healAmount = 50f;        // Her atışta ne kadar heal verecek?
    public float healCooldown = 5f;       // Kaç saniyede bir heal atacak?

    [Header("Kaçma Ayarları")]
    public float fleeRange = 10f;         // Oyuncu bu mesafeye gelince kaçmaya başla
}