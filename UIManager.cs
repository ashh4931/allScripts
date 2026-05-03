using UnityEngine;
using UnityEngine;
using UnityEngine.UI; // 🔴 Hatanın çözümü için bu satır şart!
using TMPro; // Eğer TextMeshPro kullanıyorsan bu da kalsın
using System.Collections;
public class UIManager : MonoBehaviour
{
    public GameObject skillTreeCanvas;
    public GameObject inventoryCanvas;
[Header("Demo Uyarı Ayarları")]
    public GameObject warningPanel; // Müfettişten (Inspector) paneli buraya sürükle
    public float warningDuration = 5f;
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


    public void ShowDemoWarning()
    {
        if (warningPanel != null)
        {
            StartCoroutine(FadeAndBreathRoutine());
        }
    }

    private IEnumerator FadeAndBreathRoutine()
    {
        CanvasGroup cg = warningPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = warningPanel.AddComponent<CanvasGroup>();

        warningPanel.SetActive(true);
        float elapsed = 0f;

        // --- GİRİŞ VE NEFES ALMA (BREATH) ---
        while (elapsed < warningDuration)
        {
            elapsed += Time.deltaTime;
            
            // Matematiksel Nefes Efekti: Sinüs dalgası kullanarak 0.4 ile 1.0 arası alpha değişimi
            // 3f hızı temsil eder, artırırsan daha hızlı nefes alır.
            cg.alpha = 0.7f + Mathf.Sin(Time.time * 4f) * 0.3f;
            
            yield return null;
        }

        // --- ÇIKIŞ (ŞEFFAFLAŞARAK KAPANMA) ---
        float fadeTime = 1f;
        float startAlpha = cg.alpha;
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            cg.alpha = Mathf.Lerp(startAlpha, 0, t / fadeTime);
            yield return null;
        }

        warningPanel.SetActive(false);
    }
}