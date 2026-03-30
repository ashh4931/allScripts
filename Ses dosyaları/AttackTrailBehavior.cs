using UnityEngine;

public class AttackTrailBehavior : StateMachineBehaviour
{
    // Animasyon başladığında çalışır
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var meleeController = animator.GetComponent<MeleeHandController>();
        if (meleeController != null && meleeController.currentWeapon != null)
        {
            var sword = meleeController.currentWeapon.GetComponent<BaseSword>();
            if (sword != null) sword.SetTrail(true);
        }
    }

    // Animasyon bittiğinde veya başka bir animasyona geçildiğinde çalışır
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var meleeController = animator.GetComponent<MeleeHandController>();
        if (meleeController != null && meleeController.currentWeapon != null)
        {
            var sword = meleeController.currentWeapon.GetComponent<BaseSword>();
            if (sword != null) sword.SetTrail(false);
        }
    }
}