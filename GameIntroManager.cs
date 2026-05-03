using UnityEngine;
using TMPro;
using System.Collections;

public class GameIntroManager : MonoBehaviour
{
    [Header("Sistem Bağlantıları")]
    public WaveManager waveManager;
    [Header("Dünya Obje Yöneticisi")]
    public WorldObjectManager worldObjectManager;

    [Header("Debug Ayarları")]
    public bool isDebugMode = false;

    [Header("Zamanlama Ayarları (Saniye)")]
    public float terminalBaslangicGecikmesi = 0.5f;
    public float yaziHizi = 0.02f;
    public float yuklemeCubuguHizi = 0.15f;
    public float taramaHizi = 0.02f;
    public float terminalSonrasiBekleme = 1.5f;
    public float glitchSuresi = 1.0f;
    public float spawnAnimSuresi = 2.0f;

    [Header("Ses Ayarları")]
    public AudioSource ambientSesSource;
    public AudioSource klavyeSesSource;

    [Header("Animator Değişimi")]
    public RuntimeAnimatorController mainHandController;

    [Header("UI Yöneticisi")]
    public HUDBootGlitch hudBootScript;

    [Header("UI Elemanları")]
    public GameObject terminalCanvas;
    public TextMeshProUGUI terminalText;

    [Header("Oyun Objeleri")]
    public GameObject zorboPlayer;
    public GameObject glitchLine;

    [Header("Silah Paketleri")]
    public GameObject magicPacketPrefab;
    public GameObject firearmPacketPrefab;
    public Transform dropPointLeft;
    public Transform dropPointRight;

    private SpriteRenderer playerSprite;

    void Start()
    {
        InitializeIntroState();
        StartCoroutine(PlayTerminalIntro());
    }

    // 1. ADIM: Başlangıç durumunu hazırla
    private void InitializeIntroState()
    {
        if (zorboPlayer != null)
        {
            playerSprite = zorboPlayer.GetComponent<SpriteRenderer>() ?? zorboPlayer.GetComponentInChildren<SpriteRenderer>();

            if (playerSprite != null)
            {
                Color c = playerSprite.color;
                c.a = 0f;
                playerSprite.color = c;
            }
        }

        zorboPlayer.SetActive(false);
        glitchLine.SetActive(false);
        terminalCanvas.SetActive(true);
        terminalText.text = "";

        if (ambientSesSource != null) ambientSesSource.Play();
    }

    // ANA AKIŞ YÖNETİCİSİ (Master Coroutine)
    IEnumerator PlayTerminalIntro()
    {
        // 1. Terminal Yazı Dizisi
        yield return StartCoroutine(ExecuteTerminalSequence());

        // 2. Glitch Geçişi
        yield return StartCoroutine(ExecuteGlitchTransition());

        // 3. Karakterin Sisteme Girişi (Spawn)
        yield return StartCoroutine(ExecutePlayerSpawn());

        // 4. Silahların ve İpuçlarının Verilmesi
        yield return StartCoroutine(ExecuteInitialLootDrop());

        // 5. Oyun Başlatma
        if (waveManager != null) waveManager.BeginWaveSystem();
    }

    // --- ALT FONKSİYONLAR ---

    private IEnumerator ExecuteTerminalSequence()
    {
        string fullTerminalText =
            "<color=#888888>[SYS_TIME: 00:00:00.000]</color>\n" +
            "> [ BOOT_SEQ_v9.2 ] ... <color=#ff3333>KERNEL PANIC.</color>\n" +
            "> MEM_CHK: <color=#ff3333>|##########| 11%</color> (CORRUPTED)\n" +
            "> <color=#ff3333>/// WARNING: UNAUTHORIZED OVERWRITE ///</color>\n" +
            "-------------------------------------------------\n" +
            "> INITIATING PROTOCOL: LAST_CORE_RESERVE.\n" +
            "> BYPASSING INFECTED DRIVES... <color=#33ff33>|##########| SUCCESS</color>\n" +
            "> [SCANNING: <color=#00ffff>%SCAN%</color>]\n" +
            "> FOUND: C:\\SYSTEM\\ROOT\\LEGACY\\ZORBO.EXE\n" +
            "> <color=#ff3333>SYSTEM PURGE IN 3... 2... 1...</color>\n" +
            "> <color=#00ffff>EXECUTE ZORBO.█</color>";

        if (isDebugMode)
        {
            terminalText.text = fullTerminalText.Replace("%SCAN%", "0x7F21 (COMPLETE)");
            terminalText.maxVisibleCharacters = terminalText.textInfo.characterCount;
            yield break; // Debug modundaysa hemen bitir
        }

        terminalText.text = fullTerminalText;
        terminalText.maxVisibleCharacters = 0;
        terminalText.ForceMeshUpdate();
        int totalVisibleCharacters = terminalText.textInfo.characterCount;

        yield return new WaitForSeconds(terminalBaslangicGecikmesi);

        for (int i = 0; i <= totalVisibleCharacters; i++)
        {
            terminalText.maxVisibleCharacters = i;
            string parsedText = terminalText.GetParsedText();

            // SCANNING Logic (Hex Tarama Animasyonu)
            if (i >= 6 && parsedText.Length >= i && parsedText.Substring(0, i).EndsWith("%SCAN%"))
            {
                yield return StartCoroutine(HandleHexScanningEffect(fullTerminalText, (finalText) =>
                {
                    fullTerminalText = finalText;
                    terminalText.text = fullTerminalText;
                    terminalText.ForceMeshUpdate();
                    totalVisibleCharacters = terminalText.textInfo.characterCount;
                }));
                // Scan animasyonundan sonra i'yi güncellemek için basit bir ofset ekleyebiliriz veya 
                // metin güncellendiği için i'yi yeni pozisyona çekebiliriz (mevcut kodun mantığını koruyorum)
            }

            HandleTypingSoundAndDelay(i, totalVisibleCharacters, out float currentDelay);
            yield return new WaitForSeconds(currentDelay);
        }

        if (klavyeSesSource != null) klavyeSesSource.Stop();
        yield return new WaitForSeconds(terminalSonrasiBekleme);
    }

    private IEnumerator HandleHexScanningEffect(string originalText, System.Action<string> onComplete)
    {
        if (klavyeSesSource != null) klavyeSesSource.Stop();

        for (int scanFrame = 0; scanFrame < 25; scanFrame++)
        {
            string hexAddr = "0x" + Random.Range(0x1000, 0xFFFF).ToString("X4");
            terminalText.text = originalText.Replace("%SCAN%", hexAddr);
            yield return new WaitForSeconds(taramaHizi);
        }

        string finalReplacement = "0x7F21 (COMPLETE)";
        onComplete?.Invoke(originalText.Replace("%SCAN%", finalReplacement));
    }

    private void HandleTypingSoundAndDelay(int charIndex, int totalChars, out float delay)
    {
        delay = yaziHizi;
        if (charIndex <= 0 || charIndex > totalChars) return;

        var charInfo = terminalText.textInfo.characterInfo[charIndex - 1];
        char lastChar = charInfo.character;
        Color32 charColor = charInfo.color;

        // 1. GECİKME MANTIĞI
        if (lastChar == '█' || lastChar == '░') delay = yuklemeCubuguHizi;
        else if (lastChar == '.' || lastChar == ':' || lastChar == '!') delay = yaziHizi * 7.5f;

        // 2. SES KONTROL MANTIĞI (Öneri: Filtreleme)
        bool isRed = charColor.r > 200 && charColor.g < 100;
        bool isGray = charColor.r == charColor.g && charColor.r < 160;

        // Yükleme çubuklarını ve sembolik blokları sessize alıyoruz
        bool isProgressBar = (lastChar == '█' || lastChar == '░');

        // Boşluklar, alt satırlar ve tireler zaten sessizdi
        bool isWhitespace = (lastChar == ' ' || lastChar == '\n' || lastChar == '-');

        // Nerede ses çalmasın?
        bool sesCalsinMi = !isRed && !isGray && !isProgressBar && !isWhitespace;

        if (klavyeSesSource != null)
        {
            if (sesCalsinMi)
            {
                // --- KÜÇÜK BİR DOKUNUŞ: PITCH RANDOMİZASYONU ---
                // Her tık sesinin aynı olmaması için pitch ile hafifçe oynuyoruz.
                // Bu, "makineleşmiş" hissi kırıp daha organik bir klavye sesi verir.
                klavyeSesSource.pitch = Random.Range(0.95f, 1.05f);

                if (!klavyeSesSource.isPlaying) klavyeSesSource.Play();
            }
            else
            {
                klavyeSesSource.Stop();
            }
        }
    }
    private IEnumerator ExecuteGlitchTransition()
    {
        if (ambientSesSource != null) ambientSesSource.Stop();
        terminalCanvas.SetActive(false);
        glitchLine.SetActive(true);

        if (!isDebugMode) yield return new WaitForSeconds(glitchSuresi);

        var glitchAnim = glitchLine.GetComponent<AdvancedGlitchAnim>();
        if (glitchAnim != null)
            glitchAnim.StopGlitchSmoothly(isDebugMode ? 0f : 0.8f);
        else
            glitchLine.SetActive(false);

        if (!isDebugMode) yield return new WaitForSeconds(0.8f);
    }

    private IEnumerator ExecutePlayerSpawn()
    {
        zorboPlayer.SetActive(true);
        PlayerController pControl = zorboPlayer.GetComponentInParent<PlayerController>();

        if (pControl != null && pControl.bodyAnimator != null)
        {
            pControl.bodyAnimator.SetTrigger("spawn");
            pControl.bodyAnimator.Update(0f);
        }

        // Glitch ve HUD sistemlerini aç
        if (zorboPlayer.GetComponent<SpriteBootGlitch>() != null)
            zorboPlayer.GetComponent<SpriteBootGlitch>().SystemOnline();

        if (hudBootScript != null) hudBootScript.SystemOnline();

        if (!isDebugMode) yield return new WaitForSeconds(spawnAnimSuresi);

        // Görünür yap
        if (playerSprite != null)
        {
            Color c = playerSprite.color;
            c.a = 1f;
            playerSprite.color = c;
        }

        // Player'ı kontrol edilebilir yap
        if (pControl != null)
        {
            if (pControl.visualAnimator != null && mainHandController != null)
                pControl.visualAnimator.runtimeAnimatorController = mainHandController;

            pControl.IntroFinished = true;
        }

        if (worldObjectManager != null) worldObjectManager.ActivateSystem();
    }

    private IEnumerator ExecuteInitialLootDrop()
    {
        if (!isDebugMode) yield return new WaitForSeconds(3.0f);

        if (magicPacketPrefab != null && dropPointLeft != null)
            Instantiate(magicPacketPrefab, dropPointLeft.position, Quaternion.identity);

        if (firearmPacketPrefab != null && dropPointRight != null)
            Instantiate(firearmPacketPrefab, dropPointRight.position, Quaternion.identity);

        if (!isDebugMode) yield return new WaitForSeconds(2.0f);

        HintManager.Instance.ShowHint("hint_firstweapon", false, 4f);
        if (!isDebugMode) yield return new WaitForSeconds(3.0f);
    }
}