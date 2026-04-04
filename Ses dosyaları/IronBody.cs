using UnityEngine;

public class IronBody : MonoBehaviour
{
    [Header("Ses Ayarları")]
    public AudioClip armorOpenSound;
    public AudioClip armorCloseSound;

    [Header("Ayarlar")]
    public float duration = 10f;
    public float defenseBoostAmount = 15f;
    public float attackPowerBoostAmount = 20f;

    [Header("Görsel Ayarlar")]
    public SpriteRenderer mainRenderer;
    public Color activeColor = new Color(1f, 0.22f, 0f, 1f);

    private bool isActive;
    private Color originalColor = Color.white;

    // Değişkeni burada tutuyoruz ki kapanırken de kullanabilelim
    private StatController statController;

    void Start()
    {
        // Renk hafızasını Start'ta alabiliriz, onda sorun yok
        if (mainRenderer != null)
        {
            originalColor = mainRenderer.color;
        }
    }

    public void use()
    {
        if (isActive) return;

        // 🔴 HAYAT KURTARAN DÜZELTME: Referansı tam kullanacağımız anda ve Parent'ta buluyoruz!
        statController = GetComponentInParent<StatController>();

        // Güvenlik Duvarı
        if (statController == null)
        {
            Debug.LogError("IronBody: StatController bulunamadı! Bu script Player'ın altındaki bir objede olmalı.");
            return;
        }

        isActive = true;

        // 1. AÇILIŞ SESİ
        if (armorOpenSound != null) AudioManager.instance.PlaySFXAtPosition(armorOpenSound, Camera.main.transform.position);

        // 2. RENGİ YAP
        if (mainRenderer != null)
        {
            mainRenderer.color = activeColor;
        }

        // 3. İSTATİSTİKLERİ DEĞİŞTİR
        statController.BoostDefense(defenseBoostAmount, duration);
        statController.BoostAttackPower(attackPowerBoostAmount, duration);

        // Demir zırh seslerini aktifleştir
        statController.isIronActive = true;

        // 4. SÜRE BİTİNCE KAPAT
        Invoke("RemoveArmorEffect", duration);
    }

    void RemoveArmorEffect()
    {
        StatController statController = GetComponentInParent<StatController>();
        PlayerStats stats = GetComponentInParent<PlayerStats>();
        isActive = false;

        // 1. KAPANIŞ SESİNİ ÇAL
        if (armorCloseSound != null) AudioManager.instance.PlaySFXAtPosition(armorCloseSound, Camera.main.transform.position);

        // 2. RENGİ ESKİ HALİNE DÖNDÜR
        if (mainRenderer != null)
        {
            mainRenderer.color = originalColor;
        }

        // Zırh kapandığı için normal seslere geri dön (Eğer statController null değilse)
        if (statController != null)
        {
            statController.isIronActive = false;
        }
    }
}