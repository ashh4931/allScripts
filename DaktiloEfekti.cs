using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
// Localization kütüphanesini dahil ediyoruz
using UnityEngine.Localization; 

public class DaktiloEfekti : MonoBehaviour
{
    [Header("Görsel Ayarlar")]
    public TextMeshProUGUI hikayeTextObjesi;
    public ScrollRect scrollRect;

    [Header("Hikaye İçeriği")]
    // 'string' yerine 'LocalizedString' kullanarak tablo bağlantısı kuruyoruz
    public LocalizedString hikayeReferansi; 

    [Header("Zamanlama Ayarları")]
    public float harfYazmaHizi = 0.03f;
    public float paragrafArasiBekleme = 2.0f;

    [Header("HUD Boot & Sahne Geçişi")]
    public string yuklenecekSahneAdi;
    public HUDBootGlitch2 hudBootScript;
    public float gecisOncesiBekleme = 1.0f;

    private VerticalLayoutGroup contentLayout;
    private ContentSizeFitter contentFitter;
    private AsyncOperation asyncLoad;
    private string[] paragraflar;
    private bool atlaBasildi = false;

    void Start()
    {
        if (scrollRect != null && scrollRect.content != null)
        {
            contentLayout = scrollRect.content.GetComponent<VerticalLayoutGroup>();
            contentFitter = scrollRect.content.GetComponent<ContentSizeFitter>();
        }

        hikayeTextObjesi.text = "";

        // Önce metni tablodan çekiyoruz, sonra hikaye başlıyor
        StartCoroutine(HikayeyiYukleVeBaslat());

        if (!string.IsNullOrEmpty(yuklenecekSahneAdi))
            StartCoroutine(SahneyiArkaplandaHazirla());
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            atlaBasildi = true;
    }

    // Localization asenkron çalıştığı için metnin yüklenmesini bekleyen Coroutine
    IEnumerator HikayeyiYukleVeBaslat()
    {
        // Tablodan metni getir
        var metniGetir = hikayeReferansi.GetLocalizedStringAsync();
        
        // İşlem tamamlanana kadar bekle
        yield return metniGetir;

        if (metniGetir.IsDone)
        {
            string gelenHikaye = metniGetir.Result;
            
            // Çekilen metni paragraflara ayır
            paragraflar = gelenHikaye.Split(new string[] { "\n\n", "\r\n\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            
            // Tüm hikayeyi oynatacak ana süreci başlat
            StartCoroutine(TumHikayeyiOynat());
        }
    }

    IEnumerator SahneyiArkaplandaHazirla()
    {
        asyncLoad = SceneManager.LoadSceneAsync(yuklenecekSahneAdi);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                yield break; 
            }
            yield return null;
        }
    }

    IEnumerator TumHikayeyiOynat()
    {
        for (int i = 0; i < paragraflar.Length; i++)
        {
            yield return StartCoroutine(ParagrafYaz(paragraflar[i], i != 0));

            float timer = 0;
            while (timer < paragrafArasiBekleme)
            {
                timer += Time.deltaTime;
                if (InputTuket()) break;
                yield return null;
            }
        }

        yield return StartCoroutine(HUDBootGecisSureci()); 
    }

    IEnumerator HUDBootGecisSureci()
    {
        if (hudBootScript == null)
        {
            if (asyncLoad != null) asyncLoad.allowSceneActivation = true;
            yield break;
        }

        yield return new WaitForSeconds(0.1f);
        hudBootScript.SystemOnline();
        yield return new WaitForSeconds(hudBootScript.bootDuration * 0.85f);

        if (asyncLoad != null)
        {
            while (asyncLoad.progress < 0.9f) yield return null;
            asyncLoad.allowSceneActivation = true;
        }
    }

    IEnumerator ParagrafYaz(string paragraf, bool boslukEkle)
    {
        atlaBasildi = false;
        if (boslukEkle) hikayeTextObjesi.text += "\n\n";
        string baslangicMetni = hikayeTextObjesi.text;

        foreach (char harf in paragraf)
        {
            if (InputTuket()) 
            { 
                hikayeTextObjesi.text = baslangicMetni + paragraf; 
                OtomatikKaydir(); 
                yield break; 
            }
            hikayeTextObjesi.text += harf;
            OtomatikKaydir();
            yield return new WaitForSeconds(harfYazmaHizi);
        }
    }

    bool InputTuket() { if (atlaBasildi) { atlaBasildi = false; return true; } return false; }

    void OtomatikKaydir()
    {
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            if (contentLayout != null) contentLayout.CalculateLayoutInputVertical();
            if (contentFitter != null) contentFitter.SetLayoutVertical();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}