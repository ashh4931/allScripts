using UnityEngine;
using System.Collections;

public class StatController : MonoBehaviour, IDamageable
{
    [Header("Hasar Yazısı (Popup)")]
    public GameObject damagePopupPrefab; // NPC'lerde kullandığın prefabı buraya atacaksın
    public Vector3 popupOffset = new Vector3(0f, 1.5f, 0f); // Yazının oyuncunun ayaklarında değil, kafasında çıkması için ofset
    private PlayerStats stats;
    [Header("Ses Ayarları")]
    public AudioClip levelUpSound;
    public AudioClip[] hurtSounds;
    [Range(0f, 1f)] public float hurtSoundVolume = 1f;

    // 🔴 YENİ: Özel Zırh Ses Listeleri
    [Header("Özel Zırh Sesleri")]
    public AudioClip[] stoneHurtSounds; // Taş kırılma/çarpma sesleri
    public AudioClip[] ironHurtSounds;  // Metalik çınlama sesleri

    // 🔴 YENİ: Karakterin şu an hangi zırhta olduğunu takip eden gizli bayraklar
    [HideInInspector] public bool isStoneActive = false;
    [HideInInspector] public bool isIronActive = false;
    void OnDisable()
    {
        Debug.Log("StatController deaktif edildi! Kapatan yer burası olabilir.", this);
    }
    private AudioSource sfxSource;
    private AudioSource audioSource;

    // Eski kısık sesli kaynağı kullanmayacağız, kendimize özel SFX kaynağı yaratacağız

    // İstediğin kadar hasar sesi atabilirsin

    [Header("Settings")]
    public float manaCostPerSecondPerSize = 5f;

    void Start() // Awake yerine Start kullan
    {
        stats = GetComponent<PlayerStats>();

        // sfxSource oluşturma kısmını burada yap
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
        }

        // AudioManager'ın hazır olduğundan emin olalım
        if (AudioManager.instance != null)
        {
            sfxSource.outputAudioMixerGroup = AudioManager.instance.sfxGroup;
        }
        else
        {
            Debug.LogWarning("AudioManager hala bulunamadı! Sesler miksere bağlanamadı.");
        }
    }

    void Update()
    {
        RestoreBaseStats();
    }

    private void RestoreBaseStats()
    {
        if (stats == null) return;

        stats.currentHealth = Mathf.MoveTowards(stats.currentHealth, stats.maxHealth, stats.healthRegenRate * Time.deltaTime);
        stats.mana = Mathf.MoveTowards(stats.mana, stats.maxMana, stats.manaRegenRate * Time.deltaTime);
        if (!GetComponent<PlayerShieldController>().isShieldActive)
            stats.stamina = Mathf.MoveTowards(stats.stamina, stats.maxStamina, stats.staminaRegenRate * Time.deltaTime);
    }

    // --- TEMEL AKSİYONLAR ---

    public void TakeDamage(float damage, float minDamage = 0f, float maxDamage = 0f)
    {
        // 1. DEFANS (Kalıcı Zırh/Savunma) HESAPLAMASI
        // Stone Body ve Iron Body'nin verdiği güç burada devreye giriyor!
        // Gelen hasardan, defans gücümüzü çıkarıyoruz.
        float damageAfterDefense = damage - stats.defense;

        // Eğer defansımız o kadar yüksek ki hasarı tamamen emiyorsa, hasarı 0'da tut. 
        // (İstersen burayı "damageAfterDefense = 1;" yapıp her zaman en az 1 hasar almasını sağlayabilirsin)
        if (damageAfterDefense < 0)
        {
            damageAfterDefense = 0;
        }

        // 2. GEÇİCİ KALKAN (Armor) HESAPLAMASI
        // Eğer defansı aşıp gelen bir hasar varsa, önce üstümüzdeki geçici kalkanlardan düşsün
        if (stats.armor > 0 && damageAfterDefense > 0)
        {
            if (stats.armor >= damageAfterDefense)
            {
                stats.armor -= damageAfterDefense;
                damageAfterDefense = 0; // Kalkan tüm kalan hasarı emdi
            }
            else
            {
                damageAfterDefense -= stats.armor; // Kalkan kırıldı, kalan hasar cana gidecek
                stats.armor = 0;
            }
        }

        // 3. CAN HESAPLAMASI (Eğer hala savuşturulamamış net bir hasar varsa)
        if (damageAfterDefense > 0)
        {
            stats.currentHealth -= damageAfterDefense;
            Debug.Log($"Player Stats: Alınan Net Hasar: {damageAfterDefense}. Kalan Can: {stats.currentHealth}");

            // Hasar yazısını oyuncunun kafasında çıkart
            ShowDamagePopup(damageAfterDefense);

            // Savaş seslerini çıkart
            PlayRandomHurtSound();
        }
        else if (damageAfterDefense == 0 && damage > 0)
        {
            // İSTEĞE BAĞLI: Eğer zırhın hasarın tamamını emdiyse ekranda "0" veya "BLOKLANDI" yazsın istersen
            // ShowDamagePopup(0); 
        }

        // 4. ÖLÜM KONTROLÜ
        if (stats.currentHealth <= 0)
        {
            Die();
        }
    }
    private void ShowDamagePopup(float damageAmount)
    {
        if (damagePopupPrefab != null)
        {
            // Yazıyı oyuncunun pozisyonunun biraz yukarısında (kafasında) yarat
            Vector3 spawnPos = transform.position + popupOffset;
            GameObject popup = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);

            // İçindeki TextMeshPro bileşenini bul (Eğer normal TextMesh kullanıyorsan burayı değiştirebiliriz)
            TMPro.TextMeshPro textMesh = popup.GetComponentInChildren<TMPro.TextMeshPro>();

            if (textMesh != null)
            {
                // Hasarı tam sayı olarak yazdır (Örn: 15.4 yerine 15)
                textMesh.text = damageAmount.ToString("F0");
                // Oyuncu hasar yediği için yazının rengini kan kırmızısı yapalım!
                textMesh.color = Color.red;
            }
        }
    }
    // Ses Çalma Fonksiyonu
    private void PlayRandomHurtSound()
    {
        // Başlangıçta varsayılan et seslerini seçiyoruz
        AudioClip[] currentSounds = hurtSounds;

        // 1. KONTROL: Eğer Iron (Demir) Body açıksa, listeyi demir sesleriyle değiştir!
        if (isIronActive && ironHurtSounds != null && ironHurtSounds.Length > 0)
        {
            currentSounds = ironHurtSounds;
        }
        // 2. KONTROL: Eğer Stone (Taş) Body açıksa, listeyi taş sesleriyle değiştir!
        else if (isStoneActive && stoneHurtSounds != null && stoneHurtSounds.Length > 0)
        {
            currentSounds = stoneHurtSounds;
        }

        // Seçilen listeden rastgele bir ses çal
        if (currentSounds != null && currentSounds.Length > 0 && sfxSource != null)
        {
            AudioClip randomClip = currentSounds[Random.Range(0, currentSounds.Length)];

            sfxSource.pitch = Random.Range(1.2f, 1.5f);
            sfxSource.volume = hurtSoundVolume;
            sfxSource.PlayOneShot(randomClip);
        }
    }
    public void Heal(float amount)
    {
        stats.currentHealth = Mathf.Min(stats.currentHealth + amount, stats.maxHealth);
        Debug.Log("Player Stats : Heal : " + stats.currentHealth);

    }

    public void UseMana(float amount)
    {
        stats.mana -= amount;
    }

    public void UseStamina(float amount)
    {
        stats.stamina -= amount;
    }
    public void moneyIncrease(float amount)
    {
        stats.money += amount;

    }
    public float checkMoney()
    {
        return stats.money;
    }
    public void manaIncrease(float amount)
    {

    }
    public void moneyDecrease(float amount)
    {
        float money2 = stats.money;
        money2 -= amount;
        if (money2 < 0)
        {
            stats.money = 0;
        }
        else
        {
            stats.money -= amount;
        }
        Debug.Log("PlayerStats: money : " + stats.money);
    }
    void Die()
    {
        Debug.Log("OYUN BİTTİ! Öldün.");
    }

    public void AddMoney(float amount)
    {
        stats.money += amount;
    }


    public void BoostHealth(float amount, float duration) => StartCoroutine(HealthBoostRoutine(amount, duration));
    public void BoostMana(float amount, float duration) => StartCoroutine(ManaBoostRoutine(amount, duration));
    public void BoostStamina(float amount, float duration) => StartCoroutine(StaminaBoostRoutine(amount, duration));
    public void BoostMovementSpeed(float amount, float duration) => StartCoroutine(StatBoostRoutine("movSpeed", amount, duration));
    public void BoostDefense(float amount, float duration) => StartCoroutine(StatBoostRoutine("defense", amount, duration));
    public void BoostAttackPower(float amount, float duration) => StartCoroutine(StatBoostRoutine("attackPower", amount, duration));
    public void BoostAttackSpeed(float amount, float duration) => StartCoroutine(StatBoostRoutine("attackSpeed", amount, duration));

    private IEnumerator HealthBoostRoutine(float amount, float duration)
    {
        stats.maxHealth += amount;
        stats.currentHealth += amount;
        yield return new WaitForSeconds(duration);
        stats.maxHealth -= amount;
        if (stats.currentHealth > stats.maxHealth) stats.currentHealth = stats.maxHealth;
    }

    private IEnumerator ManaBoostRoutine(float amount, float duration)
    {
        stats.maxMana += amount;
        stats.mana += amount;
        yield return new WaitForSeconds(duration);
        stats.maxMana -= amount;
        if (stats.mana > stats.maxMana) stats.mana = stats.maxMana;
    }

    private IEnumerator StaminaBoostRoutine(float amount, float duration)
    {
        stats.maxStamina += amount;
        stats.stamina += amount;
        yield return new WaitForSeconds(duration);
        stats.maxStamina -= amount;
        if (stats.stamina > stats.maxStamina) stats.stamina = stats.maxStamina;
    }

    private IEnumerator StatBoostRoutine(string statName, float amount, float duration)
    {
        switch (statName)
        {
            case "movSpeed": stats.movSpeed += amount; break;
            case "defense": stats.defense += amount; break;
            case "attackPower": stats.attackPower += amount; break;
            case "attackSpeed": stats.attackSpeed += amount; break;
        }

        yield return new WaitForSeconds(duration);

        switch (statName)
        {
            case "movSpeed": stats.movSpeed -= amount; break;
            case "defense": stats.defense -= amount; break;
            case "attackPower": stats.attackPower -= amount; break;
            case "attackSpeed": stats.attackSpeed -= amount; break;
        }
    }
    // StatController.cs içinde

    // Seviye atlamak için gereken tecrübe puanını hesaplayan bir yardımcı değişken/fonksiyon
    public float GetRequiredExperience()
    {
        // Örnek Formül: Seviye * 100 (1. seviye için 100, 2. için 200...)
        return stats.level * 100f;
    }

    public void AddExperience(float amount)
    {
        stats.experience += amount;
        Debug.Log($"Tecrübe kazanıldı: {amount}. Güncel XP: {stats.experience}/{GetRequiredExperience()}");

        // XP, gereken miktarı aşarsa seviye atla
        while (stats.experience >= GetRequiredExperience())
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        // Fazla kalan XP'yi bir sonraki seviyeye aktar
        stats.experience -= GetRequiredExperience();
        stats.level++;

        // 🔴 STAT ARTIŞLARI (Burayı dilediğin gibi dengele)
        stats.maxHealth += 20f;
        stats.currentHealth = stats.maxHealth; // Seviye atlayınca canı tazele

        stats.maxMana += 10f;
        stats.mana = stats.maxMana;

        stats.attackPower += 5f;
        stats.defense += 1f;

        Debug.Log($"<color=green>TEBRİKLER! Seviye Atladın: {stats.level}</color>");

        // İstersen buraya bir seviye atlama efekti veya sesi ekleyebilirsin
        // sfxSource.PlayOneShot(levelUpSound);
        if (levelUpSound != null)
        {
            // AudioManager'daki SFX kanalını kullanarak sesi çalar
            AudioManager.instance.PlaySFXAtPosition(levelUpSound, transform.position);
        }
    }
}