using UnityEngine;
using TMPro;
using System.Collections;

public class GameIntroManager : MonoBehaviour
{
    [Header("UI Yöneticisi")]
    public HUDBootGlitch hudBootScript; // Oyun UI Canvas'ını buraya sürükle

    [Header("UI Elemanları")]
    public GameObject terminalCanvas;
    public TextMeshProUGUI terminalText;

    [Header("Oyun Objeleri")]
    public GameObject zorboPlayer;
    public GameObject glitchLine;

    [Header("Silah Paketleri")]
    public GameObject magicPacketPrefab;   // Asa paketi
    public GameObject firearmPacketPrefab; // Silah paketi
    public Transform dropPointLeft;        // Zorbo'nun sol üstü
    public Transform dropPointRight;       // Zorbo'nun sağ üstü

    void Start()
    {
        // Başlangıçta her şey kapalı, terminal açık
        zorboPlayer.SetActive(false);
        glitchLine.SetActive(false);
        terminalCanvas.SetActive(true);
        terminalText.text = "";

        StartCoroutine(PlayTerminalIntro());
    }

    IEnumerator PlayTerminalIntro()
    {
        // 1. DÜZELTİLMİŞ TERMİNAL METNİ (0x fazlalığı silindi)
        string fullTerminalText =
      "<color=#888888>[SYS_TIME: 00:00:00.000]</color>\n" +
      "> [ BOOT_SEQ_v9.2 ] ... <color=#ff3333>KERNEL PANIC.</color>\n" +
      "> MEM_CHK: <color=#ff3333>[██░░░░░░░░] 11%</color> (CORRUPTED)\n" +
      "> <color=#ff3333>/// WARNING: UNAUTHORIZED OVERWRITE ///</color>\n" +
      "> ALL PERIMETER DEFENSES: OFFLINE.\n" +
      "-------------------------------------------------\n" +
      "<color=#888888>[SYS_TIME: 00:00:01.442]</color>\n" +
      "> INITIATING PROTOCOL: LAST_CORE_RESERVE.\n" +
      "> BYPASSING INFECTED DRIVES... <color=#33ff33>[██████████] SUCCESS</color>\n" +
      "> SEARCHING DEEP_ARCHIVE PATHS...\n" +
      "> [SCANNING: <color=#00ffff>%SCAN%</color>]\n" +
      "> FOUND: C:\\SYSTEM\\ROOT\\LEGACY\\ZORBO.EXE\n" +
      "<color=#888888>[SYS_TIME: 00:00:02.911]</color>\n" +
      "> WARNING: DEBUGGER_UNIT IS HIGHLY UNSTABLE.\n" +
      "> WE HAVE NO OTHER CHOICE.\n" +
      "> <color=#ff3333>SYSTEM PURGE IN 3... 2... 1...</color>\n" +
      "> <color=#00ffff>EXECUTE ZORBO.█</color>";

        terminalText.text = fullTerminalText;
        terminalText.maxVisibleCharacters = 0;
        terminalText.ForceMeshUpdate();

        int totalVisibleCharacters = terminalText.textInfo.characterCount;

        yield return new WaitForSeconds(0.5f);

        // 2. GÜNCELLENMİŞ DAKTİLO DÖNGÜSÜ
        for (int i = 0; i <= totalVisibleCharacters; i++)
        {
            terminalText.maxVisibleCharacters = i;

            // TMP'nin HTML kodlarından arındırılmış saf metnini alıyoruz
            string parsedText = terminalText.GetParsedText();

            // Eğer daktilo tam olarak "%SCAN%" kelimesini bitirdiyse hileyi çalıştır
            if (i >= 6 && parsedText.Length >= i && parsedText.Substring(0, i).EndsWith("%SCAN%"))
            {
                // Sayıları akıt
                for (int scanFrame = 0; scanFrame < 25; scanFrame++)
                {
                    string hexAddr = "0x" + Random.Range(0x1000, 0xFFFF).ToString("X4");
                    terminalText.text = fullTerminalText.Replace("%SCAN%", hexAddr);
                    yield return new WaitForSeconds(0.02f); // Hızlı tarama
                }

                // Taramayı bitir ve kalıcı sonuca çevir
                string finalReplacement = "0x7F21 (COMPLETE)";
                fullTerminalText = fullTerminalText.Replace("%SCAN%", finalReplacement);
                terminalText.text = fullTerminalText;

                // --- HAYAT KURTARAN GÜNCELLEME BURADA ---
                // Metin uzadığı için TextMeshPro'ya "harfleri yeniden say" diyoruz
                terminalText.ForceMeshUpdate();
                totalVisibleCharacters = terminalText.textInfo.characterCount;

                // Döngü sayacını (i), aradaki harf farkı kadar ileri sarıyoruz ki yazmaya kaldığı yerden devam etsin
                i += (finalReplacement.Length - "%SCAN%".Length);
            }

            // Harf bazlı hız ayarı
            float delay = 0.02f;
            if (i > 0 && i < totalVisibleCharacters)
            {
                char lastChar = terminalText.textInfo.characterInfo[i - 1].character;
                if (lastChar == '.' || lastChar == ':' || lastChar == '!') delay = 0.15f;
                else if (lastChar == '\n') delay = 0.25f;
            }

            yield return new WaitForSeconds(delay);
        }

        // 3. FIRTINA ÖNCESİ SESSİZLİK (2.5 saniye)
        yield return new WaitForSeconds(2.5f);

        // 4. GLITCH VE ZORBO GİRİŞİ
        terminalCanvas.SetActive(false);
        glitchLine.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        zorboPlayer.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        if (glitchLine.GetComponent<AdvancedGlitchAnim>() != null)
            glitchLine.GetComponent<AdvancedGlitchAnim>().StopGlitchSmoothly(0.8f);
        else
            glitchLine.SetActive(false);

        yield return new WaitForSeconds(0.8f);
        yield return new WaitForSeconds(0.5f);

        // 5. HUD VE SİLAHLAR
        if (hudBootScript != null) hudBootScript.SystemOnline();

        if (magicPacketPrefab != null && dropPointLeft != null)
            Instantiate(magicPacketPrefab, dropPointLeft.position, Quaternion.identity);

        if (firearmPacketPrefab != null && dropPointRight != null)
            Instantiate(firearmPacketPrefab, dropPointRight.position, Quaternion.identity);
    }
}