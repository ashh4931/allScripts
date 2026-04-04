using System;
using UnityEngine;

public class DoubleDash : MonoBehaviour
{
    public float dashSpeed = 20f;
    public float dashTime = 0.2f;
    public AudioClip dashSound;
    
    private AudioSource audioSource;
    private Rigidbody2D rb;
    private bool isDashing = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponentInParent<Rigidbody2D>();
    }

    public void use()
    {
        if (!isDashing)
        {
            if (dashSound != null && audioSource != null)
            {
                // Üst üste atılırken seslerin aynı çıkmaması için pitch'i rastgele değiştir
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.15f);
                audioSource.PlayOneShot(dashSound);
            }

            PlayerController pc = GetComponentInParent<PlayerController>();
            float speedX = pc.speedX;
            float speedY = pc.speedY;

            Vector2 dashDirection = Vector2.zero;

            // Yön belirleme
            if (speedX > 0 && speedY > 0) dashDirection = new Vector2(1, 1);
            else if (speedX > 0 && speedY < 0) dashDirection = new Vector2(1, -1);
            else if (speedX < 0 && speedY > 0) dashDirection = new Vector2(-1, 1);
            else if (speedX < 0 && speedY < 0) dashDirection = new Vector2(-1, -1);
            else if (speedX > 0) dashDirection = Vector2.right;
            else if (speedX < 0) dashDirection = Vector2.left;
            else if (speedY > 0) dashDirection = Vector2.up;
            else if (speedY < 0) dashDirection = Vector2.down;
            
            // Eğer hiçbir tuşa basmıyorsa sağa (veya baktığı yöne) gitsin
            if (dashDirection == Vector2.zero) dashDirection = Vector2.right; 

            dashDirection = dashDirection.normalized;

            StartCoroutine(DashCoroutine(dashDirection));
        }
    }

    private System.Collections.IEnumerator DashCoroutine(Vector2 direction)
    {
        isDashing = true;

        PlayerController pc = GetComponentInParent<PlayerController>();
        if (pc != null) pc.isDashing = true;

        float startTime = Time.time;
        while (Time.time < startTime + dashTime)
        {
            rb.linearVelocity = direction * dashSpeed;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        if (pc != null) pc.isDashing = false;
        isDashing = false;
    }
}