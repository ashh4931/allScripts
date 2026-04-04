using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DaktiloEfekti : MonoBehaviour
{
    [Header("Görsel Ayarlar")]
    public TextMeshProUGUI hikayeTextObjesi;
    public ScrollRect scrollRect;

    [Header("Hikaye İçeriği")]
    [TextArea(15, 20)]
    public string yazilacakHikaye;

    [Header("Zamanlama Ayarları")]
    public float harfYazmaHizi = 0.03f;
    public float paragrafArasiBekleme = 2.0f;

    [Header("HUD Boot & Sahne Geçişi")]
    public string yuklenecekSahneAdi;
    // Yeni scriptini buraya referans olarak veriyoruz
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

        // BU SATIRI SİL VEYA YORUMA AL (Panelin kapanmasını engellemek için)
        // if (hudBootScript != null) hudBootScript.gameObject.SetActive(false); 

        hikayeTextObjesi.text = "";
        paragraflar = yazilacakHikaye.Split(new string[] { "\n\n", "\r\n\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        if (!string.IsNullOrEmpty(yuklenecekSahneAdi))
            StartCoroutine(SahneyiArkaplandaHazirla());

        StartCoroutine(TumHikayeyiOynat());
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            atlaBasildi = true;
    }
    IEnumerator SahneyiArkaplandaHazirla()
    {
        asyncLoad = SceneManager.LoadSceneAsync(yuklenecekSahneAdi);
        asyncLoad.allowSceneActivation = false;

        // Sahne yüklenene kadar döngüde kal
        while (!asyncLoad.isDone)
        {
            // .progress değeri 0 ile 0.9 arasında döner (allowSceneActivation false iken)
            float ilerleme = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            Debug.Log($"[ZORBO SİSTEM] Sahne Yükleme Durumu: %{ilerleme * 100:F0}");

            // Yükleme %90'a ulaştığında Unity beklemeye geçer
            if (asyncLoad.progress >= 0.9f)
            {
                Debug.Log("<color=green>[ZORBO SİSTEM] Sahne arka planda tamamen hazır!</color>");
                yield break; // Döngüden çık
            }

            yield return null;
        }
    }


    IEnumerator TumHikayeyiOynat()
    {
        // ... Paragrafları yazdıran döngü ...
        for (int i = 0; i < paragraflar.Length; i++)
        {
            yield return StartCoroutine(ParagrafYaz(paragraflar[i], i != 0));
            // ... bekleme mantığı ...
            float timer = 0;
            while (timer < paragrafArasiBekleme)
            {
                timer += Time.deltaTime;
                if (InputTuket()) break;
                yield return null;
            }
        }

        // --- TÜM YAZILAR BİTTİĞİNDE BURAYA GELİR ---
        yield return StartCoroutine(HUDBootGecisSureci()); // Glitch ve sahne geçişini başlat
    }
   IEnumerator HUDBootGecisSureci()
{
    if (hudBootScript == null)
    {
        if (asyncLoad != null) asyncLoad.allowSceneActivation = true;
        yield break;
    }

    // 1. Beklemeyi neredeyse sıfıra indir
    yield return new WaitForSeconds(0.1f);

    // 2. Glitch başlasın
    hudBootScript.SystemOnline();

    // 3. KRİTİK DEĞİŞİKLİK: 
    // Animasyonun bitmesini BEKLEMEDEN sahneye "hazırlan" emri veriyoruz.
    // Animasyon süresinin %70'i kadar bekleyip sahneyi tetikliyoruz.
    yield return new WaitForSeconds(hudBootScript.bootDuration * 0.85f);

    if (asyncLoad != null)
    {
        // Sahne arka planda hazır olana kadar bekle (eğer çok büyükse)
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // Sahneyi aç! 
        // (Bu sırada glitch'in son %30'u hala oynuyor olacak, bu da lagı gizler)
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
            if (InputTuket()) { hikayeTextObjesi.text = baslangicMetni + paragraf; OtomatikKaydir(); yield break; }
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