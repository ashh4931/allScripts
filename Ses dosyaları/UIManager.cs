using UnityEngine;
using UnityEngine;
using UnityEngine.UI; // 🔴 Hatanın çözümü için bu satır şart!
using TMPro; // Eğer TextMeshPro kullanıyorsan bu da kalsın
public class UIManager : MonoBehaviour
{
    public GameObject skillTreeCanvas;
    public GameObject inventoryCanvas;

    void Update()
    {
        // Yetenek Ağacı Kontrolü
        if (InputManager.GetKeyDown("SkillTree"))
        {
            ToggleMenu(skillTreeCanvas);
        }

        // Envanter Kontrolü
        if (InputManager.GetKeyDown("Inventory"))
        {
            ToggleMenu(inventoryCanvas);
        }

        // --- 🔴 ESC TUŞU İLE KAPATMA SİSTEMİ ---
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
    }

    private void HandleEscapeKey()
    {
        // Eğer Skill Tree açıksa onu kapat
        if (skillTreeCanvas != null && skillTreeCanvas.activeSelf)
        {
            ToggleMenu(skillTreeCanvas);
        }
        // Eğer Inventory açıksa onu kapat
        else if (inventoryCanvas != null && inventoryCanvas.activeSelf)
        {
            ToggleMenu(inventoryCanvas);
        }
    }

    void ToggleMenu(GameObject menu)
    {
        if (menu == null) return;

        bool isActive = !menu.activeSelf; // Yeni durum (aktif mi olacak?)
        menu.SetActive(isActive);
        if (isActive)
        {
            // Menü içindeki ScrollRect bileşenini bul ve en üste (1) taşı
            ScrollRect scroll = menu.GetComponentInChildren<ScrollRect>();
            if (scroll != null)
            {
                scroll.verticalNormalizedPosition = 1f;
            }
        }
        // Eğer en az bir menü hala açıksa (örneğin birini kapattın ama diğeri açık kaldıysa)
        bool anyMenuOpen = (skillTreeCanvas != null && skillTreeCanvas.activeSelf) ||
                           (inventoryCanvas != null && inventoryCanvas.activeSelf);

        if (anyMenuOpen)
        {
            //Cursor.visible = true;
            Time.timeScale = 0;
            // Cursor.lockState = CursorLockMode.None; // İstersen açabilirsin
        }
        else
        {
            //Cursor.visible = false;
            Time.timeScale = 1;
            // Cursor.lockState = CursorLockMode.Locked; // İstersen açabilirsin
        }
    }
}