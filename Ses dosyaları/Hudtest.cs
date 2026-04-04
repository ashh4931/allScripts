using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Hudtest : MonoBehaviour
{
    public Color staminaNormalColor = new Color(1f, 0.7f, 0f); // Kendi stamina rengin
    private bool wasStaminaFull = true;
    private Vector3 staminaBarOrigin;
    private float staminaShakeTimer = 0f;
    public PlayerStats playerStats;
    [Header("Level Settings")]
    public TextMeshProUGUI levelText; // 🔴 YENİ: UI'daki Level metnini buraya sürükle
    public string levelPrefix = "LVL ";
    [Header("Bar Images")]
    public Image healthBar;      // HealthLVL objesini buraya at
    public Image healthGhostBar; // HealthGhost objesini buraya at
    public Image manaBar;
    public Image staminaBar;
    public Image xpBar;

    [Header("Settings")]
    public float lerpSpeed = 0.05f;
    // RENKLERİ BURADAN VEYA INSPECTOR'DAN MUTLAKA SEÇ!
    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;

    private Vector3 healthBarOrigin;

    void Start()
    {
        // Başlangıç yerini kaydediyoruz
        if (healthBar != null) healthBarOrigin = healthBar.transform.localPosition;
        if (staminaBar != null) staminaBarOrigin = staminaBar.transform.localPosition;
    }


    void UpdateXP()
    {
        if (xpBar == null || playerStats == null) return;

        // StatController'daki formülü burada da kullanıyoruz
        float requiredXP = playerStats.level * 100f;
        xpBar.fillAmount = playerStats.experience / requiredXP;
    }
    void UpdateLevel()
    {
        if (levelText == null || playerStats == null) return;

        // Seviye bilgisini PlayerStats'tan alıp metne yazdırıyoruz
        levelText.text = levelPrefix + playerStats.level.ToString();
    }
    void UpdateStaminaShake()
    {
        if (staminaShakeTimer > 0)
        {
            staminaShakeTimer -= Time.deltaTime;
            float shakeX = Random.Range(-5f, 5f); // Titreme şiddeti
            staminaBar.transform.localPosition = staminaBarOrigin + new Vector3(shakeX, 0, 0);

            // Barın rengini geçici olarak kırmızı/gri yapabiliriz
            staminaBar.color = Color.red;
        }
        else
        {
            staminaBar.transform.localPosition = staminaBarOrigin;
            staminaBar.color = Color.white; // Veya orijinal rengi
        }
    }
    public void ShakeStaminaBar()
    {
        staminaShakeTimer = 0.3f; // 0.3 saniye boyunca titreyecek
    }
    void Update()
    {
        UpdateHealth();
        UpdateMana();
        updateStamina();
        UpdateStaminaShake();
        UpdateXP();
        UpdateLevel();
    }

    void UpdateHealth()
    {
        if (healthBar == null || playerStats == null) return;

        float healthRatio = (float)playerStats.currentHealth / playerStats.maxHealth;
        healthBar.fillAmount = healthRatio;

        // Ghost Bar takibi
        if (healthGhostBar != null)
        {
            healthGhostBar.fillAmount = Mathf.Lerp(healthGhostBar.fillAmount, healthRatio, lerpSpeed);
        }

        // RENK KONTROLÜ: Siyahlığı önlemek için
        healthBar.color = Color.Lerp(lowHealthColor, fullHealthColor, healthRatio);

        // Efektler
        HandleHealthEffects(healthRatio);
    }

    void HandleHealthEffects(float ratio)
    {
        if (ratio < 0.3f) // Can %30 altındaysa
        {
            float pulse = 1f + Mathf.Sin(Time.time * 15f) * 0.05f;
            healthBar.transform.localScale = new Vector3(pulse, pulse, 1f);

            // Titreme (Shake)
            float shakeX = Random.Range(-1f, 1f);
            healthBar.transform.localPosition = healthBarOrigin + new Vector3(shakeX, 0, 0);
        }
        else
        {
            // Normale dön
            healthBar.transform.localScale = Vector3.one;
            healthBar.transform.localPosition = healthBarOrigin;
        }
    }

    void UpdateMana()
    {
        if (manaBar != null)
            manaBar.fillAmount = (float)playerStats.mana / playerStats.maxMana;
    }

    void updateStamina()
    {
        float staminaRatio = (float)playerStats.stamina / playerStats.maxStamina;
        staminaBar.fillAmount = staminaRatio;

        // 1. TAM DOLMA ANI: Az önce dolu değildi ama şimdi dolduysa
        if (staminaRatio >= 1f && !wasStaminaFull)
        {
            // Barı anlık olarak BEYAZ yap (Parlama efekti)
            staminaBar.color = Color.white;
            wasStaminaFull = true;
        }
        else if (staminaRatio < 1f)
        {
            wasStaminaFull = false;
        }

        // 2. RENK GERİ DÖNÜŞÜ: Eğer bar beyazsa, yavaşça orijinal rengine dönsün
        if (staminaBar.color != staminaNormalColor)
        {
            staminaBar.color = Color.Lerp(staminaBar.color, staminaNormalColor, Time.deltaTime * 5f);
        }
    }
    void TriggerStaminaPing()
    {
        // Barı anlık olarak beyazlatıp eski rengine döndürebiliriz
        // Veya basit bir "scaling" (büyüyüp küçülme) yapabiliriz
        StopCoroutine("PingEffect");
        StartCoroutine("PingEffect");
    }

    System.Collections.IEnumerator PingEffect()
    {
        staminaBar.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        yield return new WaitForSeconds(0.1f);
        staminaBar.transform.localScale = Vector3.one;
    }
}