using UnityEngine;

[CreateAssetMenu(fileName = "New Circular Shield Skill", menuName = "Skills/Circular Shield")]
public class CircularShieldSkillData : SkillData
{
    [Header("Dairesel Kalkan Ayarları")]
    public float shieldHealth = 100f; // Kalkanın canı
    public float duration = 5f;       // Hiç hasar almazsa ne kadar süre açık kalacak?

    public override void Cast(GameObject player)
    {
        CircularShield skillComponent = player.GetComponentInChildren<CircularShield>(true);
        if (skillComponent != null)
        {
            skillComponent.ActivateShield(shieldHealth, duration);
        }
    }
}