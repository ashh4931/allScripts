using UnityEngine;

public class GhostRun : MonoBehaviour
{
    bool isGhostRunning = false;
    float ghostrunDuration = 5f;
    float ghostrunTimer = 0f;
    float ghostrunSpeedMultiplier;
    public AudioClip ghostRunSound;

    private PlayerStats stats;
    private SpriteRenderer mainRenderer;
private Rigidbody2D playerRb;private void Awake()
{
    
    playerRb = GetComponentInParent<Rigidbody2D>();
}void Start()
{
    stats = GetComponentInParent<PlayerStats>();
    ghostrunSpeedMultiplier = 2f * stats.movSpeed;

    // 🔥 Visual child’ından SpriteRenderer
    Transform visual = transform.root.Find("visual");
    mainRenderer = visual.GetComponent<SpriteRenderer>();
}

    void Update()
    {
        if (isGhostRunning)
        {
            // Zamanı azalt
            ghostrunTimer -= Time.deltaTime;

            // Şeffaflık efekti (Nefes alma tarzı)
            if (mainRenderer != null)
            {
                float alpha = Mathf.PingPong(Time.time * 5f, 0.5f) + 0.3f; // 0.3 ile 0.8 arası gider gelir
                Color c = mainRenderer.color;
                c.a = alpha;
                mainRenderer.color = c;
            }

            // Süre bitti mi?
            if (ghostrunTimer <= 0)
            {
                StopGhostRun();
            }
        }
    }

    public void Use() // 'use' küçük harf yerine 'Use' daha standarttır
    {
        if (isGhostRunning) return; // Zaten aktifse tekrar başlatma
        playsoundeffect();
        isGhostRunning = true;
        ghostrunTimer = ghostrunDuration;
        stats.movSpeed += ghostrunSpeedMultiplier;
    }

    void StopGhostRun()
    { 
        isGhostRunning = false;
        stats.movSpeed -= ghostrunSpeedMultiplier;

        // Şeffaflığı normale döndür
        if (mainRenderer != null)
        {
            Color c = mainRenderer.color;
            c.a = 1f;
            mainRenderer.color = c;
        }
    }
    void playsoundeffect()
    {
        AudioSource.PlayClipAtPoint(ghostRunSound, transform.position);
    }
}