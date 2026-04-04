using UnityEngine;
using TMPro;
using UnityEngine.Localization; // Localization kütüphanesi
using UnityEngine.Localization.Components; // Otomatik güncelleme bileşenleri için

public class weapon : MonoBehaviour
{
    [Header("Sistem Ayarları")]
    public New_WeaponData weaponData;
    public bool isEquipped = false;
    public Transform gripPoint;

    [Header("Efekt Ayarları")]
    public GameObject visualEffect;

    [Header("UI Elemanları")]
    public GameObject interactionCanvas;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI descriptionText;

    void Awake() // Start yerine Awake kullanarak daha erken kapanmasını sağla
    {
        // Oyun başında kartı kapat, efekti (yerdeki parlama) aç
        if (interactionCanvas != null) interactionCanvas.SetActive(false);
        if (visualEffect != null) visualEffect.SetActive(!isEquipped);
    }
    void Start()
    {
        // Başlangıçta UI'ı hazırla
        UpdateCardUI();
        if (interactionCanvas != null) interactionCanvas.SetActive(false);

        // Efekti başlangıç durumuna göre ayarla
        RefreshEffect();
    }

    // UPDATE metodu içindeki her saniye çalışan kontrolü sildik. 
    // Performans için sadece durum değiştiğinde (SetEquipped) çağırmak en iyisidir.

    public void SetEquipped(bool state)
    {
        isEquipped = state;

        // Silah eldeyse ne kart ne efekt gözükmeli
        if (isEquipped)
        {
            if (interactionCanvas != null) interactionCanvas.SetActive(false);
            if (visualEffect != null) visualEffect.SetActive(false);
        }
        else
        {
            // Silah yere bırakıldıysa efekti geri aç ama KARTI açma (oyuncu triggerda değilse)
            if (visualEffect != null) visualEffect.SetActive(true);
        }
    }

    public void RefreshEffect()
    {
        if (visualEffect != null)
        {
            // Yerdeyken aktif, eldeyken deaktif
            visualEffect.SetActive(!isEquipped);
        }
    }
    public void UpdateCardUI()
    {
        if (weaponData == null) return;

        // İsmi ve açıklamayı güncelle
        nameText.text = weaponData.weaponName.GetLocalizedString();
        descriptionText.text = weaponData.description.GetLocalizedString();

        // Nadirliğe göre renk belirle
        Color rarityColor = GetRarityColor(weaponData.rarity);
        nameText.color = rarityColor; // Silah isminin rengini değiştir

        // Eğer CardBackground objen varsa onun rengini de hafifçe değiştirebilirsin
        // GetComponentInChildren<Image>().color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, 0.5f);
    }

    private Color GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return Color.white;
            case Rarity.Uncommon: return Color.green;
            case Rarity.Rare: return new Color(0.1f, 0.5f, 1f); // Mavi
            case Rarity.Epic: return new Color(0.6f, 0.1f, 1f); // Mor
            case Rarity.Legendary: return Color.orange;
            case Rarity.Mythic: return Color.red;
            default: return Color.white;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Sadece Player gelirse VE silah yerdeyse kartı aç
        if (other.CompareTag("Player") && !isEquipped)
        {
            UpdateCardUI();
            if (interactionCanvas != null) interactionCanvas.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (interactionCanvas != null) interactionCanvas.SetActive(false);
        }
    }
}