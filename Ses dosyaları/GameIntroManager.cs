using UnityEngine;
using TMPro;
using System.Collections;

public class GameIntroManager : MonoBehaviour
{
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

    private SpriteRenderer playerSprite;

    [Header("Silah Paketleri")]
    public GameObject magicPacketPrefab;
    public GameObject firearmPacketPrefab;
    public Transform dropPointLeft;
    public Transform dropPointRight;

    void Start()
    {
        // Başlangıçta sprite'ı bul ve tamamen şeffaf yap
        if (zorboPlayer != null)
        {
            playerSprite = zorboPlayer.GetComponent<SpriteRenderer>();
            if (playerSprite == null) playerSprite = zorboPlayer.GetComponentInChildren<SpriteRenderer>();

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

        StartCoroutine(PlayTerminalIntro());
    }

    IEnumerator PlayTerminalIntro()
    {
        // ... (Terminal Metni ve Yazma Mantığı - Değişmedi) ...
        string fullTerminalText =
            "<color=#888888>[SYS_TIME: 00:00:00.000]</color>\n" +
            "> [ BOOT_SEQ_v9.2 ] ... <color=#ff3333>KERNEL PANIC.</color>\n" +
            "> MEM_CHK: <color=#ff3333>|██░░░░░░░░| 11%</color> (CORRUPTED)\n" +
            "> <color=#ff3333>/// WARNING: UNAUTHORIZED OVERWRITE ///</color>\n" +
            "-------------------------------------------------\n" +
            "> INITIATING PROTOCOL: LAST_CORE_RESERVE.\n" +
            "> BYPASSING INFECTED DRIVES... <color=#33ff33>|██████████| SUCCESS</color>\n" +
            "> [SCANNING: <color=#00ffff>%SCAN%</color>]\n" +
            "> FOUND: C:\\SYSTEM\\ROOT\\LEGACY\\ZORBO.EXE\n" +
            "> <color=#ff3333>SYSTEM PURGE IN 3... 2... 1...</color>\n" +
            "> <color=#00ffff>EXECUTE ZORBO.█</color>";

        if (isDebugMode)
        {
            terminalText.text = fullTerminalText.Replace("%SCAN%", "0x7F21 (COMPLETE)");
            terminalText.maxVisibleCharacters = terminalText.textInfo.characterCount;
        }
        else
        {
            terminalText.text = fullTerminalText;
            terminalText.maxVisibleCharacters = 0;
            terminalText.ForceMeshUpdate();
            int totalVisibleCharacters = terminalText.textInfo.characterCount;
            yield return new WaitForSeconds(terminalBaslangicGecikmesi);

            for (int i = 0; i <= totalVisibleCharacters; i++)
            {
                terminalText.maxVisibleCharacters = i;
                string parsedText = terminalText.GetParsedText();

                if (i >= 6 && parsedText.Length >= i && parsedText.Substring(0, i).EndsWith("%SCAN%"))
                {
                    if (klavyeSesSource != null) klavyeSesSource.Stop();
                    for (int scanFrame = 0; scanFrame < 25; scanFrame++)
                    {
                        string hexAddr = "0x" + Random.Range(0x1000, 0xFFFF).ToString("X4");
                        terminalText.text = fullTerminalText.Replace("%SCAN%", hexAddr);
                        yield return new WaitForSeconds(taramaHizi);
                    }
                    string finalReplacement = "0x7F21 (COMPLETE)";
                    fullTerminalText = fullTerminalText.Replace("%SCAN%", finalReplacement);
                    terminalText.text = fullTerminalText;
                    terminalText.ForceMeshUpdate();
                    totalVisibleCharacters = terminalText.textInfo.characterCount;
                    i += (finalReplacement.Length - "%SCAN%".Length);
                }

                float currentDelay = yaziHizi;
                bool sesCalsinMi = false;

                if (i > 0 && i <= totalVisibleCharacters)
                {
                    char lastChar = terminalText.textInfo.characterInfo[i - 1].character;
                    if (lastChar == '█' || lastChar == '░') currentDelay = yuklemeCubuguHizi;
                    else if (lastChar == ' ' || lastChar == '\n' || lastChar == '-') sesCalsinMi = false;
                    else
                    {
                        sesCalsinMi = true;
                        if (lastChar == '.' || lastChar == ':' || lastChar == '!') currentDelay = yaziHizi * 7.5f;
                    }

                    if (klavyeSesSource != null)
                    {
                        if (sesCalsinMi && !klavyeSesSource.isPlaying) klavyeSesSource.Play();
                        else if (!sesCalsinMi) klavyeSesSource.Stop();
                    }
                }
                yield return new WaitForSeconds(currentDelay);
            }
        }

        if (klavyeSesSource != null) klavyeSesSource.Stop();
        if (!isDebugMode) yield return new WaitForSeconds(terminalSonrasiBekleme);

        if (ambientSesSource != null) ambientSesSource.Stop();
        terminalCanvas.SetActive(false);
        glitchLine.SetActive(true);

        if (!isDebugMode) yield return new WaitForSeconds(glitchSuresi);

        if (glitchLine.GetComponent<AdvancedGlitchAnim>() != null)
            glitchLine.GetComponent<AdvancedGlitchAnim>().StopGlitchSmoothly(isDebugMode ? 0f : 0.8f);
        else
            glitchLine.SetActive(false);

        if (!isDebugMode) yield return new WaitForSeconds(0.8f);

        // OYUNCUYU HAZIRLA
        zorboPlayer.SetActive(true); // Obje açıldı ama hala görünmez (Alpha 0)

        PlayerController pControl = zorboPlayer.GetComponentInParent<PlayerController>();

        if (pControl != null)
        {
            // 1. Önce tetiği çekiyoruz (Hala görünmeziz)
            if (pControl.bodyAnimator != null)
            {
                pControl.bodyAnimator.SetTrigger("spawn");

                // SİHİRLİ SATIR: Animatörü 0 saniye ilerletmeye zorluyoruz. 
                // Bu, animatorün state geçişini o saniyede yapmasını sağlar.
                pControl.bodyAnimator.Update(0f);
            }

            // 2. Glitch efektini başlatıyoruz
            SpriteBootGlitch playerGlitch = zorboPlayer.GetComponent<SpriteBootGlitch>();
            if (playerGlitch != null) playerGlitch.SystemOnline();

            // 3. ŞİMDİ görünür yapıyoruz (Artık animator spawn animasyonunun ilk karesinde bekliyor)
            if (playerSprite != null)
            {
                Color c = playerSprite.color;
                c.a = 1f;
                playerSprite.color = c;
            }
        }



        if (magicPacketPrefab != null && dropPointLeft != null)
            Instantiate(magicPacketPrefab, dropPointLeft.position, Quaternion.identity);
        if (firearmPacketPrefab != null && dropPointRight != null)
            Instantiate(firearmPacketPrefab, dropPointRight.position, Quaternion.identity);

        if (!isDebugMode) yield return new WaitForSeconds(spawnAnimSuresi);
        if (!isDebugMode) yield return new WaitForSeconds(1f);
        if (pControl != null)
        {
            if (pControl.visualAnimator != null && mainHandController != null)
                pControl.visualAnimator.runtimeAnimatorController = mainHandController;
            pControl.IntroFinished = true;
        }
        if (hudBootScript != null) hudBootScript.SystemOnline();
        if (!isDebugMode) yield return new WaitForSeconds(2.0f);

        // Dünya objelerini (varilleri, kutuları vb.) glitch ile uyandır
        if (worldObjectManager != null) worldObjectManager.ActivateSystem();

    }
}