using System.Data.SqlTypes;
using UnityEngine;

public class PlayerStats : MonoBehaviour 
{

    [Header("Max Stats")]
    public float
    maxHealth = 100f,
    maxMana = 100f,
    maxStamina = 75f, 
    deffensePower = 5f; 


    [Header("Current Stats")]
    public float
    currentHealth = 100f,
    mana = 500f,
    stamina = 75f,
    money=0f,
    armor=10f;
 
   
    [Header("Combat")]
    public float
    attackPower = 20f,
    defense = 1f,
    critChance = 5f,
    critDamage = 150f,
    attackSpeed = 25f;


    [Header("Regen Rates")]
    public float
    healthRegenRate = 0.5f,
    manaRegenRate = 5f,
    staminaRegenRate = 2f;


    [Header("Progression")]
    public int level = 1;
    public float experience = 0f;
    public int slotCount = 0;

    [Header("Movement")]
    public float movSpeed = 5f;

    private void Awake()
    {
        currentHealth = maxHealth;
        mana = maxMana;
        stamina = maxStamina;
    }
   public void increasehealth(float amount, float time)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }



}

