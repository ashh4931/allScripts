using UnityEngine;

public class PlayerControllerWithAnimationTest : MonoBehaviour
{
    public Animator animator;
    public Transform visual; 
    public float baseScale = 8f;

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // Hareket kontrolü
        if (x != 0 || y != 0)
        {
            UpdateAnimation(x, y);
            FlipCharacter(x);
        }
        else
        {
            SetAllBoolsFalse();
            animator.SetBool("idle", true);
        }
    }

    void UpdateAnimation(float x, float y)
    {
        SetAllBoolsFalse();

        // 1. Çaprazlar (Önce çaprazları kontrol etmeliyiz)
        if (y < 0 && x != 0) 
            animator.SetBool("moving_front_diagonal", true);
        else if (y > 0 && x != 0) 
            animator.SetBool("moving_behind_diagonal", true); 

        // 2. Düz Yönler
        else if (y < 0) 
            animator.SetBool("moving_front", true);
        else if (y > 0) 
            animator.SetBool("moving_behind", true);
        else if (x != 0) 
            animator.SetBool("moving_side", true);
    }

    void FlipCharacter(float x)
    {
        // Sprite'ların SOL (left) olduğu için:
        // x negatifse (sola gidiyorsa) düz tut (8)
        // x pozitifse (sağa gidiyorsa) aynala (-8)
        if (x < 0)
            visual.localScale = new Vector3(baseScale, baseScale, baseScale);
        else if (x > 0)
            visual.localScale = new Vector3(-baseScale, baseScale, baseScale);
    }

    void SetAllBoolsFalse()
    {
        animator.SetBool("idle", false);
        animator.SetBool("moving_front", false);
        animator.SetBool("moving_behind", false);
        animator.SetBool("moving_side", false);
        animator.SetBool("moving_front_diagonal", false);
        animator.SetBool("moving_behind_diagonal", false);
    }
}