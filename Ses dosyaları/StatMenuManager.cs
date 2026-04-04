using UnityEngine;
using TMPro; 

public class StatMenuManager : MonoBehaviour
{
    [Header("UI Referansları (Paneller ve Yazılar)")]
    public GameObject statMenuPanel; 
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;
    
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
    public float defenseIncrease = 0.3f;
    
    // 🔴 YENİ EKLENEN ARTIRMA MİKTARLARI
    public float maxManaIncrease = 20f;
    public float healthRegenIncrease = 0.5f;
    public float manaRegenIncrease = 1f;
    public float speedIncrease = 0.25f;

    private PlayerStats playerStats;
    private bool isMenuOpen = false;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
        }
        
        if (statMenuPanel != null) statMenuPanel.SetActive(false); 
    }

   void Update()
{
    // Mevcut özel giriş kontrolün (Örn: "I" veya "C" tuşu gibi)
    if (InputManager.GetKeyDown("OpenStatMenu"))
    {
        ToggleMenu();
    }

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
    }

    private void UpdateUI()
    {
        healthText.text = $"Maks. Can: {playerStats.maxHealth}";
        attackText.text = $"Saldırı Gücü: {playerStats.attackPower}";
        defenseText.text = $"Savunma: {playerStats.defense}";
        
        // 🔴 YENİ EKLENEN YAZILARIN GÜNCELLENMESİ
        maxManaText.text = $"Maks. Mana: {playerStats.maxMana}";
        healthRegenText.text = $"Can Yenileme: {playerStats.healthRegenRate}/sn";
        manaRegenText.text = $"Mana Yenileme: {playerStats.manaRegenRate}/sn";
        speedText.text = $"Hareket Hızı: {playerStats.movSpeed}";

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
}