using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float movSpeed;
    public float speedX, speedY;
    Vector2 movement;
    public bool isDashing;
    public KeyCode movementBoost = KeyCode.LeftShift;
    public float sprintMultiplier = 1.2f;
    [Header("Sprint Ayarları")]
    public float sprintStaminaCost = 15f; // Saniyede ne kadar mana harcayacak?
    Rigidbody2D rb;
    public Animator bodyAnimator;
    public Animator handAnimator;

    // Yeni hali
    public PlayerStats stats;

    [Header("Footstep Audio")]
    public AudioSource audioSource;
    public AudioClip[] footstepClips;
    public float baseStepInterval = 0.5f;

    float footstepTimer;
    private bool isKnockedBack = false;//Knockback(geri tepme) yapmak için eklendi.
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();


        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        speedX = Input.GetAxisRaw("Horizontal");
        speedY = Input.GetAxisRaw("Vertical");
        movement = new Vector2(speedX, speedY).normalized;
        if (Input.GetKeyDown(KeyCode.H))
        {
            stats.currentHealth += 500;
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            stats.money += 1000;
        }
        movSpeed = stats.movSpeed;

        // ✅ Sprint ve Mana hesabı
        // Şartlar: Koşma tuşuna basılıyor MU? + Karakter hareket ediyor MU? + Manası var MI?
        if (Input.GetKey(movementBoost) && movement.sqrMagnitude > 0 && stats.stamina > 0)
        {
            movSpeed *= sprintMultiplier; // Hızı artır

            // Zamanla manayı azalt (saniyede sprintManaCost kadar)
            stats.stamina -= sprintStaminaCost * Time.deltaTime;

            // Mananın eksi değerlere düşmesini engelle
            if (stats.mana < 0)
            {
                stats.mana = 0;
            }
        }

        UpdateAnimations();
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

        if (isMoving)
        {
            // Hareket ediyorsa, basılan tuşlara göre yön belirle
            bodyAnimator.SetFloat("moveX", speedX);
            bodyAnimator.SetFloat("moveY", speedY);
            handAnimator.SetFloat("moveX", speedX);
            handAnimator.SetFloat("moveY", speedY);
        }
        else
        {
            // Hareket etmiyorsa (Idle), fareye doğru bak
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 lookDirection = (mousePos - transform.position).normalized;

            // Animator'daki moveX ve moveY değerlerini farenin yönüne göre güncelle
            bodyAnimator.SetFloat("moveX", lookDirection.x);
            bodyAnimator.SetFloat("moveY", lookDirection.y);

            // El animasyonu da fareye uyum sağlasın istiyorsan:
            handAnimator.SetFloat("moveX", lookDirection.x);
            handAnimator.SetFloat("moveY", lookDirection.y);
        }
    }

    void HandleFootsteps()
    {
        if (movement.sqrMagnitude <= 0)
        {
            footstepTimer = 0f;
            return;
        }

        float stepInterval = baseStepInterval / movSpeed;
        footstepTimer += Time.deltaTime;

        if (footstepTimer >= stepInterval)
        {
            PlayFootstep();
            footstepTimer = 0f;
        }
    }

    void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(
            footstepClips[Random.Range(0, footstepClips.Length)]
        );
    }
    // Knockback(geri tepme) yapmak için eklendi. BombEnemyData.cs dosyasına knockbackForce adında bir değişken ekledik. 
    // BombNPC.cs dosyasında ise ApplyKnockback() fonksiyonunu oluşturduk. 
    // Bu fonksiyon, düşmanın oyuncuya uygulayacağı itme kuvvetini hesaplar ve Rigidbody2D bileşenine uygular.
    // Ayrıca, knockback etkisi sırasında hareket kontrolünü devre dışı bırakmak için isKnockedBack adlı bir bool değişkeni kullanıyoruz.
    public void ApplyKnockback(Vector2 force)
    {
        isKnockedBack = true;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);
        Invoke("ResetKnockback", 0.2f); // 0.2 saniye sonra kontrolü geri ver
    }
    private void ResetKnockback() => isKnockedBack = false;
}
