using UnityEngine;

public class HudController : MonoBehaviour
{
    private PlayerSkillManager skillManager;

    void Awake()
    {
        skillManager = GetComponent<PlayerSkillManager>();
    }

    void Update()
    {
        // Space tuşu her zaman "Dash" slotunu tetikler
        if (Input.GetKeyDown(KeyCode.Space))
        {
            skillManager.UseSkillInSlot(skillManager.dashSlot);
        }

        // Q tuşu 1. Slotu tetikler (İçinde ne varsa o çalışır!)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            skillManager.UseSkillInSlot(skillManager.slot1);
        }

        // E tuşu 2. Slotu tetikler
        if (Input.GetKeyDown(KeyCode.E))
        {
            skillManager.UseSkillInSlot(skillManager.slot2);
        }
        
        // Diğer tuşlarını da bu şekilde slotlara bağlayabilirsin...*/
    }
}