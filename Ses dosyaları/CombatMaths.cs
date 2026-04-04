using UnityEngine;

public static class CombatMaths
{
    
    
  
     public static float CalculateFireRate(float baseFireRate, float attackSpeed)
    {
        return  baseFireRate / Mathf.Sqrt(attackSpeed);

    }

}
