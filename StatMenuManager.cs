using UnityEngine;
using TMPro;

public class StatMenuManager : MonoBehaviour
{
    [Header("UI Referansları (Paneller ve Yazılar)")]
    public GameObject statMenuPanel;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI maxStaminaText;
    public TextMeshProUGUI staminaRegenText;
    // 🔴 YENİ EKLENEN YAZI REFERANSLARI
    public TextMeshProUGUI maxManaText;
    public TextMeshProUGUI healthRegenText;
    public TextMeshProUGUI manaRegenText;
    public TextMeshProUGUI speedText;

    public TextMeshProUGUI moneyText;

    [Header("Geliştirme Bedeli (Para)")]
    public float upgradeCost = 50f;

    [Header("Geliştirme Miktarları")]
    public float healthIncrease = 50f;
    public float attackIncrease = 5f;
    public float defenseIncrease = 3.5f;

    // 🔴 YENİ EKLENEN ARTIRMA MİKTARLARI
    public float maxManaIncrease = 20f;
    public float healthRegenIncrease = 0.5f;
    public float manaRegenIncrease = 1f;
    public float speedIncrease = 0.25f;
    public float maxStaminaIncrease = 15f;

    public float staminaRegenIncrease = 0.75f;
    private PlayerStats playerStats;
    private bool isMenuOpen = false;
    GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
        }

        if (statMenuPanel != null) statMenuPanel.SetActive(false);
    }

    void Update()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
        }
        // Mevcut özel giriş kontrolün (Örn: "I" veya "C" tuşu gibi)
        if (InputManager.GetKeyDown("OpenStatMenu"))
        {
            ToggleMenu();
        }
        if (playerStats.money > 70)
            HintManager.Instance.ShowHint("hint_first_Stat", false, 5f);
        if (playerStats.money > 180)
            HintManager.Instance.ShowHint("hint_first_Skill", false, 5f);
        if (playerStats.stamina > 50)
            HintManager.Instance.ShowHint("hint_shield", false, 5f);

        // --- 🔴 ESC TUŞU KONTROLÜ ---
        // Eğer menü açıksa ve Esc'ye basılırsa menüyü kapat
        if (isMenuOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }

        if (isMenuOpen && playerStats != null)
        {
            UpdateUI();
        }
    }

    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        statMenuPanel.SetActive(isMenuOpen);

        // Menü açıldığı anda değerleri bir kez hemen güncelle
        if (isMenuOpen)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        // UI güncellenmeden hemen önce playerStats hala null mı diye bak
        if (playerStats == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerStats = player.GetComponent<PlayerStats>();

            if (playerStats == null) return; // Hala bulunamadıysa (intro bitmediyse) çık
        }
        healthText.text = $"Maks. Can: {playerStats.maxHealth}";
        attackText.text = $"Saldırı Gücü: {playerStats.attackPower}";
        defenseText.text = $"Savunma: {playerStats.defense}";
        maxManaText.text = $"Maks. Mana: {playerStats.maxMana}";
        healthRegenText.text = $"Can Yenileme: {playerStats.healthRegenRate}/sn";
        manaRegenText.text = $"Mana Yenileme: {playerStats.manaRegenRate}/sn";
        speedText.text = $"Hareket Hızı: {playerStats.movSpeed}";

        // 🟢 STAMINA UI GÜNCELLEME
        maxStaminaText.text = $"Maks. Enerji: {playerStats.maxStamina}";
        staminaRegenText.text = $"Enerji Yenileme: {playerStats.staminaRegenRate}/sn";

        moneyText.text = $"Para: {playerStats.money} (Geliştirme Bedeli: {upgradeCost})";
    }

    // --- TEMEL GELİŞTİRMELER ---
    public void UpgradeHealth()
    {
        if (playerStats != null && playerStats.money >= upgradeCost)
        {
            playerStats.money -= upgradeCost;
            playerStats.maxHealth += healthIncrease;
            playerStats.currentHealth += healthIncrease; // Max can artınca mevcut can da o kadar dolsun
        }
    }

    public void UpgradeAttack()
    {
        if (playerStats != null && playerStats.money >= upgradeCost)
        {
            playerStats.money -= upgradeCost;
            playerStats.attackPower += attackIncrease;
        }
    }

    public void UpgradeDefense()
    {
        if (playerStats != null && playerStats.money >= upgradeCost)
        {
            playerStats.money -= upgradeCost;
            playerStats.defense += defenseIncrease;
        }
    }

    // --- 🔴 YENİ GELİŞTİRME FONKSİYONLARI ---

    public void UpgradeMaxMana()
    {
        if (playerStats != null && playerStats.money >= upgradeCost)
        {
            playerStats.money -= upgradeCost;
            playerStats.maxMana += maxManaIncrease;
            playerStats.mana += maxManaIncrease; // Max mana artınca mevcut mana da dolsun
        }
    }

    public void UpgradeHealthRegen()
    {
        if (playerStats != null && playerStats.money >= upgradeCost)
        {
            playerStats.money -= upgradeCost;
            playerStats.healthRegenRate += healthRegenIncrease;
        }
    }

    public void UpgradeManaRegen()
    {
        if (playerStats != null && playerStats.money >= upgradeCost)
        {
            playerStats.money -= upgradeCost;
            playerStats.manaRegenRate += manaRegenIncrease;
        }
    }

    public void UpgradeSpeed()
    {
        if (playerStats != null && playerStats.money >= upgradeCost)
        {
            playerStats.money -= upgradeCost;
            playerStats.movSpeed += speedIncrease;
        }
    }

    public void UpgradeMaxStamina()
    {
        if (playerStats != null && playerStats.money >= upgradeCost)
        {
            playerStats.money -= upgradeCost;
            playerStats.maxStamina += maxStaminaIncrease;
            playerStats.stamina += maxStaminaIncrease; // Kapasite artınca mevcut enerjiyi de artır
        }
    }

    public void UpgradeStaminaRegen()
    {
        if (playerStats != null && playerStats.money >= upgradeCost)
        {
            playerStats.money -= upgradeCost;
            playerStats.staminaRegenRate += staminaRegenIncrease;
        }
    }
}