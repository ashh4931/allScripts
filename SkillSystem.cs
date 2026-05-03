using System;
using Unity.VisualScripting;
using UnityEngine;

public class SkillSystem : MonoBehaviour
{
    Skill
    dash,
    electricField,
    teleport,
    heal,
    lifeoverflow,
        ghostRun;

    public class Skill
    {
        public string skillName;
        public float cooldown;
        public float staminaCost;
        public float manaCost;
        private float lastUsedTime = -Mathf.Infinity;
        public bool IsReady => Time.time >= lastUsedTime + cooldown;
        public float CooldownRemaining => Mathf.Max(0f, (lastUsedTime + cooldown) - Time.time);
        private Rigidbody2D playerRb;
        public bool TryUse()
        {
            if (!IsReady) return false;

            lastUsedTime = Time.time;
            return true;
        }
        public Skill(string name, float cd, float mana, float stamina)
        {
            skillName = name;
            cooldown = cd;
            staminaCost = stamina;
            manaCost = mana;
        }
    }



    private void createSkills()
    {
        dash = new Skill("Dash", 3f, 20f, 30f);
        teleport = new Skill("Teleport", 5f, 60f, 0f);
        electricField = new Skill("ElectricField", 60f, 50f, 0f);
        ghostRun = new Skill("GhostRun", 20f, 40f, 20f);
        heal = new Skill("Heal", 10f, 30f, 0f);
        lifeoverflow = new Skill("LifeOverflow", 30f, 100f, 0f);
    }

    void Start()
    {
        createSkills();
    }

    public void useSkill(String skillName, float size = 1f)
    {
        switch (skillName)
        {
            case "Dash":
                {
                    if (dash.TryUse())
                    {
                        if (!CheckMana(dash.manaCost))
                        {
                            Debug.Log("Not enough mana to use Dash.");
                            return;
                        }
                        if (!CheckStamina(dash.staminaCost))
                        {
                            Debug.Log("Not enough stamina to use Dash.");
                            return;
                        }
                        GetComponent<Dash>().use();
                        GetComponentInParent<StatController>().UseMana(dash.manaCost);
                        GetComponentInParent<StatController>().UseStamina(dash.staminaCost);
                    }
                    else
                    {
                        Debug.Log("Dash skill is on cooldown. Time remaining: " + dash.CooldownRemaining + " seconds.");
                    }
                    break;
                }
            case "LifeOverflow":
                {
                    if (lifeoverflow.TryUse())
                    {
                        if (!CheckMana(lifeoverflow.manaCost))
                        {
                            Debug.Log("Not enough mana to use Life Overflow.");
                            return;
                        }
                        GetComponent<LifeOverflow>().use();
                        GetComponent<StatController>().UseMana(lifeoverflow.manaCost);
                    }
                    else
                    {
                        Debug.Log("Life Overflow skill is on cooldown. Time remaining: " + lifeoverflow.CooldownRemaining + " seconds.");
                    }
                    break;
                }
            case "Teleport":
                {
                    if (teleport.TryUse())
                    {
                        if (!CheckMana(teleport.manaCost))
                        {
                            Debug.Log("Not enough mana to use Teleport.");
                            return;
                        }
                        GetComponent<Teleport>().Use();
                        GetComponentInParent<StatController>().UseMana(teleport.manaCost);
                    }
                    else
                    {
                        Debug.Log("Teleport skill is on cooldown. Time remaining: " + teleport.CooldownRemaining + " seconds.");
                    }
                    break;
                }
            case "ElectricField":
                {
                    if (electricField.TryUse())
                    {
                        if (!CheckMana(electricField.manaCost))
                        {
                            Debug.Log("Not enough mana to use Electric Field.");
                            return;
                        }
                      //  GetComponent<ElectricField>().use(size);
                       // GetComponentInParent<StatController>().UseMana(electricField.manaCost);
                    }
                    else
                    {
                        GetComponent<ElectricField>().StopSkill();
                        Debug.Log("Electric Field skill is on cooldown. Time remaining: " + electricField.CooldownRemaining + " seconds.");
                    }
                    break;
                }
            case "Heal":
                {
                    if (heal.TryUse())
                    {
                        if (!CheckMana(heal.manaCost))
                        {
                            Debug.Log("Not enough mana to use Heal.");
                            return;
                        }
                        GetComponent<Heal>().use();
                        GetComponent<StatController>().UseMana(heal.manaCost);
                    }
                    else
                    {
                        Debug.Log("Heal skill is on cooldown. Time remaining: " + heal.CooldownRemaining + " seconds.");
                    }
                    break;
                }
            case "GhostRun":
                {
                    if (ghostRun.TryUse())
                    {
                        if (!CheckMana(ghostRun.manaCost))
                        {
                            Debug.Log("Not enough mana to use Ghost Run.");
                            return;
                        }
                        if (!CheckStamina(ghostRun.staminaCost))
                        {
                            Debug.Log("Not enough stamina to use Ghost Run.");
                            return;
                        }
                        GetComponent<GhostRun>().Use();
                        GetComponentInParent<StatController>().UseMana(ghostRun.manaCost);
                        GetComponentInParent<StatController>().UseStamina(ghostRun.staminaCost);
                    }
                    else
                    {
                        Debug.Log("Ghost Run skill is on cooldown. Time remaining: " + ghostRun.CooldownRemaining + " seconds.");
                    }
                    break;
                }
            default:
                {
                    Debug.Log("Skill not found.");
                    break;
                }
        }
    }
    private bool CheckMana(float manaCost)
    {
        PlayerStats stats = GetComponentInParent<PlayerStats>();
        if (stats == null) return false;

        return stats.mana >= manaCost;
    }

    private bool CheckStamina(float staminaCost)
    {
        PlayerStats stats = GetComponentInParent<PlayerStats>();
        if (stats == null) return false;

        return stats.stamina >= staminaCost;
    }

}













