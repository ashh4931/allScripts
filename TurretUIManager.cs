using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TurretUIManager : MonoBehaviour
{
    [Header("Bağlantılar")]
    public PlayerTurretController playerController; 
    public GameObject menuPanel; 

    [Header("Sağ Taraf: Ekipman Yuvaları (Dropdowns)")]
    public Dropdown[] slotDropdowns; 

    private List<PlayerTurretData> uniqueOwnedTurrets = new List<PlayerTurretData>();

    void Update()
    {
       if (InputManager.GetKeyDown("TurretMenu") || (menuPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape)))
    {
        ToggleMenu();
    }
    }

    public void ToggleMenu()
    {
        if (menuPanel != null)
        {
            bool isActive = menuPanel.activeSelf;
            menuPanel.SetActive(!isActive);

            if (!isActive) 
            {
                RefreshDropdowns();
            }
        }
    }

    public void UI_BuyTurret(PlayerTurretData turretData)
    {
        if (playerController != null)
        {
            bool success = playerController.BuyTurret(turretData);
            if (success)
            {
                RefreshDropdowns(); 
            }
        }
    }

    public void RefreshDropdowns()
    {
        if (playerController == null || slotDropdowns.Length != 4) return;

        uniqueOwnedTurrets.Clear();
        foreach (var t in playerController.ownedTurrets)
        {
            if (!uniqueOwnedTurrets.Contains(t)) uniqueOwnedTurrets.Add(t);
        }

        for (int i = 0; i < 4; i++)
        {
            Dropdown dropdown = slotDropdowns[i]; 
            
            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.ClearOptions();

            List<string> options = new List<string> { "Yok (Yuva Bos)" }; 

            foreach (var t in uniqueOwnedTurrets)
            {
                int totalOwned = playerController.ownedTurrets.FindAll(x => x == t).Count;
                int currentlyEquipped = 0;
                
                for (int j = 0; j < 4; j++)
                {
                    if (playerController.equippedData[j] == t) currentlyEquipped++;
                }

                // 🔴 YENİ DÜZENLEME: Stok ve Kullanım Durumu Matematiği
                int availableStock = totalOwned - currentlyEquipped;
                bool isEquippedInThisSlot = (playerController.equippedData[i] == t);

                if (isEquippedInThisSlot) availableStock++; // Bu yuvadaki taret kendi stoğunu yiyor sayılmaz

                // Yazıyı oluşturmaya başlıyoruz
                string optionText = t.turretName;

                if (availableStock > 0)
                {
                    optionText += " (Stok: " + availableStock + ")";
                }
                else
                {
                    optionText += " (Stok: 0)";
                }

                // Eğer BU YUVA HARİCİNDE başka bir yuvada takılıysa yanına uyarı ekle!
                int usedInOtherSlots = currentlyEquipped - (isEquippedInThisSlot ? 1 : 0);
                if (usedInOtherSlots > 0)
                {
                    optionText += " - [Kullanimda]";
                }

                options.Add(optionText);
            }

            dropdown.AddOptions(options);

            if (playerController.equippedData[i] == null)
            {
                dropdown.value = 0; 
            }
            else
            {
                int index = uniqueOwnedTurrets.IndexOf(playerController.equippedData[i]);
                dropdown.value = index + 1; 
            }

            int slotIndex = i; 
            dropdown.onValueChanged.AddListener((int selectedValue) => OnDropdownSelectionChanged(slotIndex, selectedValue));
        }
    }

    private void OnDropdownSelectionChanged(int slotIndex, int selectedDropdownValue)
    {
        if (selectedDropdownValue == 0)
        {
            playerController.EquipTurret(null, slotIndex);
        }
        else
        {
            PlayerTurretData selectedData = uniqueOwnedTurrets[selectedDropdownValue - 1];

            int totalOwned = playerController.ownedTurrets.FindAll(x => x == selectedData).Count;
            int currentlyEquipped = 0;
            for (int j = 0; j < 4; j++) if (playerController.equippedData[j] == selectedData) currentlyEquipped++;
            
            bool isEquippedInThisSlot = (playerController.equippedData[slotIndex] == selectedData);
            int actuallyAvailable = totalOwned - currentlyEquipped + (isEquippedInThisSlot ? 1 : 0);

            if (actuallyAvailable > 0)
            {
                playerController.EquipTurret(selectedData, slotIndex);
            }
            else
            {
                // 🔴 YENİ: Eğer stokta olmayan bir şeyi seçmeye çalışırsa konsola yaz ve işlemi iptal et
                Debug.LogWarning("Bu taret baska bir yuvada kullanimda ve bostaki stogu bitti!");
            }
        }

        // 🔴 YENİ: Hata yapsa da yapmasa da menüyü yenile ki, yanlışlıkla (Stok: 0) seçtiyse kutu eski haline geri dönsün!
        RefreshDropdowns();
    }
}