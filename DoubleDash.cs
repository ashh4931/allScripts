using UnityEngine;

public class DoubleDash : MonoBehaviour
{
    public float dashSpeed = 20f;
    public float dashTime = 0.2f;
    public AudioClip dashSound;
    
    private AudioSource audioSource;
    private Rigidbody2D mainRb;
    private PlayerController pc;
    private bool isDashing = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
        pc = GetComponentInParent<PlayerController>();
        if (pc != null)
        {
            mainRb = pc.GetComponent<Rigidbody2D>();
        }
    }

    public void use()
    {
        if (!isDashing && pc != null && mainRb != null)
        {
            if (dashSound != null && audioSource != null)
            {
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.15f);
                audioSource.PlayOneShot(dashSound);
            }

            float speedX = pc.speedX;
            float speedY = pc.speedY;

            Vector2 dashDirection = Vector2.zero;

            if (speedX > 0 && speedY > 0) dashDirection = new Vector2(1, 1);
            else if (speedX > 0 && speedY < 0) dashDirection = new Vector2(1, -1);
            else if (speedX < 0 && speedY > 0) dashDirection = new Vector2(-1, 1);
            else if (speedX < 0 && speedY < 0) dashDirection = new Vector2(-1, -1);
            else if (speedX > 0) dashDirection = Vector2.right;
            else if (speedX < 0) dashDirection = Vector2.left;
            else if (speedY > 0) dashDirection = Vector2.up;
            else if (speedY < 0) dashDirection = Vector2.down;
            
            if (dashDirection == Vector2.zero) dashDirection = Vector2.right; 

            dashDirection = dashDirection.normalized;

            StartCoroutine(DashCoroutine(dashDirection));
        }
    }

    private System.Collections.IEnumerator DashCoroutine(Vector2 direction)
    {
        isDashing = true;
        pc.isDashing = true;

        float startTime = Time.time;
        while (Time.time < startTime + dashTime)
        {
            mainRb.linearVelocity = direction * dashSpeed;
            yield return null;
        }

        mainRb.linearVelocity = Vector2.zero;
        pc.isDashing = false;
        isDashing = false;
    }
}