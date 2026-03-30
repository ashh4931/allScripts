using System;
using UnityEngine;

public class Dash : MonoBehaviour
{
    public float dashSpeed = 20f;      // Dash hızı
    public float dashTime = 0.2f;      // Dash süresi
    private AudioSource audioSource;
    private Rigidbody2D rb;
    private bool isDashing = false;
    public AudioClip dashSound;
    void Start()
    {
        rb = GetComponentInParent<Rigidbody2D>();
    }

    public void use()
    {
        if (!isDashing)
        {
            audioSource.PlayOneShot(dashSound);
            Debug.Log("Dash skill used.");

            // PlayerController'daki hız değerlerini al
            PlayerController pc = GetComponentInParent<PlayerController>();
            float speedX = pc.speedX;
            float speedY = pc.speedY;

            Vector2 dashDirection = Vector2.zero;

            // Basit if-else ile yön belirleme (çaprazlar dahil)
            if (speedX > 0 && speedY > 0)
                dashDirection = new Vector2(1, 1);
            else if (speedX > 0 && speedY < 0)
                dashDirection = new Vector2(1, -1);
            else if (speedX < 0 && speedY > 0)
                dashDirection = new Vector2(-1, 1);
            else if (speedX < 0 && speedY < 0)
                dashDirection = new Vector2(-1, -1);
            else if (speedX > 0)
                dashDirection = Vector2.right;
            else if (speedX < 0)
                dashDirection = Vector2.left;
            else if (speedY > 0)
                dashDirection = Vector2.up;
            else if (speedY < 0)
                dashDirection = Vector2.down;

            Debug.Log("Dash direction before normalization: " + dashDirection);
            dashDirection = dashDirection.normalized; // Yönü normalize et

            StartCoroutine(DashCoroutine(dashDirection));

        }
    }

    private System.Collections.IEnumerator DashCoroutine(Vector2 direction)
    {
        isDashing = true;

        PlayerController pc = GetComponentInParent<PlayerController>();
        pc.isDashing = true;

        float startTime = Time.time;
        while (Time.time < startTime + dashTime)
        {
            rb.linearVelocity = direction * dashSpeed;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        pc.isDashing = false;
        isDashing = false;
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }



}
