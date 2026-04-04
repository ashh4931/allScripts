using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool IntroFinished = false;
    // PlayerController içinde değişkenlerin olduğu yere ekle:
    [Header("Ana Görsel Animator")]
    public Animator visualAnimator; // Inspector'dan 'visual' objesini buraya sürükle
    public float movSpeed;
    public float speedX, speedY;
    Vector2 movement;
    public bool isDashing;
    public KeyCode movementBoost = KeyCode.LeftShift;
    public float sprintMultiplier = 1.2f;

    [Header("Sprint Ayarları")]
    public float sprintStaminaCost = 15f;

    Rigidbody2D rb;
    public Animator bodyAnimator;
    public Animator handAnimator;

    [Header("Oyuncu İstatistikleri")]
    public PlayerStats stats;

    [Header("Footstep Audio")]
    public AudioSource audioSource;
    public AudioClip[] footstepClips;
    public float baseStepInterval = 0.5f;

    float footstepTimer;
    private bool isKnockedBack = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        // 1. Intro Kontrolü (Girdi ve animasyon kilidi)
        if (!IntroFinished)
        {
            movement = Vector2.zero;
            bodyAnimator.SetBool("isWalking", false);
            return;
        }

        // 2. Girdileri Al
        speedX = Input.GetAxisRaw("Horizontal");
        speedY = Input.GetAxisRaw("Vertical");
        movement = new Vector2(speedX, speedY).normalized;

        // --- Hile Tuşları (Eksiksiz) ---
        if (Input.GetKeyDown(KeyCode.H)) stats.currentHealth += 500;
        if (Input.GetKeyDown(KeyCode.L)) stats.money += 1000;

        movSpeed = stats.movSpeed;

        // --- Sprint ve Stamina (Eksiksiz) ---
        if (Input.GetKey(movementBoost) && movement.sqrMagnitude > 0 && stats.stamina > 0)
        {
            movSpeed *= sprintMultiplier;
            stats.stamina -= sprintStaminaCost * Time.deltaTime;
            if (stats.stamina < 0) stats.stamina = 0;
        }

        // 3. Fonksiyonları Çağır
        UpdateAnimations();
        HandleFootsteps();
    }
    void FixedUpdate()
    {

        if (isKnockedBack) return; // İtilme varken hareket kodunu çalıştırma!

        if (isDashing) return; // ❗ Dash sırasında hareket yazma


        rb.linearVelocity = movement * movSpeed;

    }

    void UpdateAnimations()
    {
        bool isMoving = movement.sqrMagnitude > 0;
        bodyAnimator.SetBool("isWalking", isMoving);

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDirection = ((Vector2)mousePos - (Vector2)transform.position).normalized;

        // Hem hareket hem idle durumunu tek seferde yöneten temiz yapı
        float targetX = isMoving ? speedX : lookDirection.x;
        float targetY = isMoving ? speedY : lookDirection.y;

        bodyAnimator.SetFloat("moveX", targetX);
        bodyAnimator.SetFloat("moveY", targetY);

        if (handAnimator != null)
        {
            handAnimator.SetFloat("moveX", targetX);
            handAnimator.SetFloat("moveY", targetY);
        }
    }

    // --- Ayak Sesi Sistemi (Eksiksiz) ---
    void HandleFootsteps()
    {
        if (movement.sqrMagnitude <= 0) { footstepTimer = 0f; return; }
        float stepInterval = baseStepInterval / movSpeed;
        footstepTimer += Time.deltaTime;
        if (footstepTimer >= stepInterval) { PlayFootstep(); footstepTimer = 0f; }
    }

    void PlayFootstep()
    {
        if (footstepClips.Length == 0 || audioSource == null) return;
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(footstepClips[Random.Range(0, footstepClips.Length)]);
    }

    // --- Knockback Sistemi (Eksiksiz) ---
    public void ApplyKnockback(Vector2 force)
    {
        isKnockedBack = true;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);
        Invoke("ResetKnockback", 0.2f);
    }
    private void ResetKnockback() => isKnockedBack = false;
}