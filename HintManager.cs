using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Localization.Settings; // LOCALIZATION KÜTÜPHANESİ

[RequireComponent(typeof(CanvasGroup))]
public class HintManager : MonoBehaviour
{
    public static HintManager Instance;

    [Header("Localization Ayarları")]
    public string stringTableName = "GameTextTable"; // Tablo adımız

    [Header("UI Elemanları")]
    public GameObject hintCard;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI contentText;
    public TextMeshProUGUI continueText;
    public Image cardBackground;
    
    [Header("Ses Ayarları")]
    public AudioClip hintShowSound; // İpucu belirdiğinde çalacak ses
    [Range(0f, 1f)] public float volume = 0.6f;
    
    [Header("Zaman Ayarları")]
    public float slowMotionScale = 0.2f;
    public float bootDuration = 1.5f;
    public AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Görsel Efektler (Breath & Glitch)")]
    public Color[] glitchColors = { Color.cyan, Color.magenta, Color.yellow, Color.red, Color.white };
    public Color warningColor = Color.red; // Nefes alma rengi
    public float breathSpeed = 2f;

    private CanvasGroup canvasGroup;
    private Graphic[] uiElements;
    private Color[] originalColors;
    private Color originalCardColor;
    private HashSet<string> shownHints = new HashSet<string>();
    private Coroutine activeHintCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        canvasGroup = GetComponent<CanvasGroup>();
        uiElements = hintCard.GetComponentsInChildren<Graphic>();

        originalColors = new Color[uiElements.Length];
        for (int i = 0; i < uiElements.Length; i++) originalColors[i] = uiElements[i].color;
        originalCardColor = cardBackground.color;

        hintCard.SetActive(false);
        canvasGroup.alpha = 0;
    }

    // ARTIK SADECE ID ALIYORUZ
    public void ShowHint(string id, bool isWarning = false, float duration = 3f, bool isCritical = false, bool isSlowMo = false)
    {
        if (shownHints.Contains(id)) return;

        if (activeHintCoroutine != null)
        {
            StopCoroutine(activeHintCoroutine);
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f; // Önceki hint yarıda kesilirse fiziği düzelt
        }

        shownHints.Add(id);
        
        // Asenkron çeviri yükleme işlemini başlat
        activeHintCoroutine = StartCoroutine(FetchAndShowHint(id, isWarning, duration, isCritical, isSlowMo));
    }

    IEnumerator FetchAndShowHint(string id, bool isWarning, float duration, bool isCritical, bool isSlowMo)
    {
        // 1. ZAMAN YÖNETİMİ (Metinler yüklenirken bile zaman yavaşlasın/dursun)
        if (isCritical)
        {
            Time.timeScale = 0f;
        }
        else if (isSlowMo)
        {
            Time.timeScale = slowMotionScale;
            // Fiziği de zaman yavaşlamasına uydur:
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }

        // Ortak başlık mantığı
        string titleKey = isWarning ? "hint_header_Warn" : "hint_header_guid";

        // Tablodan verileri çek
        var titleOp = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(stringTableName, titleKey);
        var contentOp = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(stringTableName, id);

        // Güvenli Bekleme: Tablodan veriler gelene kadar bekle
        while (!titleOp.IsDone || !contentOp.IsDone)
        {
            yield return null; 
        }

        string translatedTitle = titleOp.Result;
        string translatedContent = contentOp.Result;

        // Çeviri bulunamazsa varsayılan metinler
        if (string.IsNullOrEmpty(translatedTitle)) translatedTitle = isWarning ? "UYARI" : "REHBER";
        if (string.IsNullOrEmpty(translatedContent)) translatedContent = "Missing translation: " + id;

        // Animasyon ve ekrana basma sekansına geç
        yield return StartCoroutine(HintSequence(translatedContent, translatedTitle, isWarning, duration, isCritical, isSlowMo));
    }

    IEnumerator HintSequence(string content, string title, bool isWarning, float duration, bool isCritical, bool isSlowMo)
    {
        // 2. HAZIRLIK
        titleText.text = title;
        contentText.text = content;
        hintCard.SetActive(true);

        if (AudioManager.instance != null && hintShowSound != null)
        {
            AudioManager.instance.PlaySFXAtPosition(hintShowSound, Vector3.zero, volume);
        }

        if (continueText != null) continueText.gameObject.SetActive(true);

        // 3. BOOT / GLITCH ANIMASYONU
        float elapsed = 0f;
        while (elapsed < bootDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / bootDuration;
            canvasGroup.alpha = alphaCurve.Evaluate(t);

            if (t < 0.85f && Time.frameCount % 4 == 0)
            {
                foreach (var ui in uiElements)
                    ui.color = glitchColors[Random.Range(0, glitchColors.Length)];
            }
            else { ResetColors(); }
            yield return null;
        }

        // 4. BEKLEME DÖNGÜSÜ
        float stayElapsed = 0f;
        while (isCritical || stayElapsed < duration)
        {
            stayElapsed += Time.unscaledDeltaTime;

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                break;

            if (isWarning || isSlowMo)
            {
                float lerp = (Mathf.Sin(Time.unscaledTime * breathSpeed) + 1) / 2f;
                cardBackground.color = Color.Lerp(originalCardColor, warningColor, lerp);
            }

            yield return null;
        }

        // 5. NORMALLEŞME
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f; // Fiziği de normale döndürdük!

        float fadeElapsed = 0f;
        while (fadeElapsed < 0.5f)
        {
            fadeElapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1 - (fadeElapsed / 0.5f);
            yield return null;
        }

        hintCard.SetActive(false);
        cardBackground.color = originalCardColor; 
        activeHintCoroutine = null;
    }

    void ResetColors()
    {
        for (int i = 0; i < uiElements.Length; i++) uiElements[i].color = originalColors[i];
    }
}

/*public void beforeFirstWaveInfo();
//public void firstWeaponAppear();
public void waveInfo();
//public void OnFirstBulletsDrained
public void firstLVLup();
public void suggestTurrets();
public void turretInfo();
public void turretUnEquipInfo();
 
public void staminaInfo();
public void statsInfo();
public void turretUnEquipInfo();

public void shieldInfo();
public void turretOverHeatInfo();

*/

/*Eklenmiş hintler:
 -first_weapon (gameıntro.cs)
 -first shield (TutorialBullet.cs)
 -first reload (gun2_new_script.cs)
  -first turretOVerHeat (autoTurret.cs 85);
  -ilk kule hinti (waveManager)
  -kuleye ilk yaklaşma (towerController)
  -ilk turret
!!!!!!!!!!!!!!!!!!!!!!!!
YAPILACAKLAR
Hint npcleri oyunucya bilgi vermek için 1-2 tane hintnpcsi oluştur:
hint gyro gugnner:
-atteş ettiğnide ooyunucuya kalkan kulalnmayı öğretecek (script sstaminasını fulleyecek ki manası yetersiz gelmes
n)
Wave aralarına kuleler tarzı bir şey: 
-Her wave arasında spawm olurar
-kule başına sonraki wave puanına wavepuanı/4 puan ekleyip oyunu zorlaştırırlar.
-ikinci wave den sonra bunu öğreten bir hint


wave arlarnıda oyuncuya ödül verecek kovolama npc'leri:
-çok basit, oyuncudan kaç ve öldürünce ganimet düşern npcler
-ve buna bir hint
*/