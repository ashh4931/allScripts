using UnityEngine;
using TMPro;

public class ShowMoney : MonoBehaviour
{
    private TextMeshProUGUI paraYazisi;
    public GameObject Player;
    private StatController statController;
    
    private float logTimer = 0f; // Konsolu kitlememek için 2 saniyede bir log atacağız

    void Start()
    {
       // Debug.Log("<color=green>ShowMoney: SCRIPT AKTİF</color>");
        paraYazisi = GetComponent<TextMeshProUGUI>();
        
        if (paraYazisi == null) Debug.LogError("ShowMoney: TMP Bileşeni bulunamadı!");
        
        SetupReferences();
    }

    void SetupReferences()
    {
        if (Player == null) Player = GameObject.FindGameObjectWithTag("Player");
        
        if (Player != null)
        {
            statController = Player.GetComponent<StatController>();
           // Debug.Log($"ShowMoney: Takip edilen obje: {Player.name}, StatController: {(statController != null ? "OK" : "YOK")}");
        }
        else
        {
            Debug.LogWarning("ShowMoney: Sahnede Player bulunamadı!");
        }
    }

    void Update()
    {
        // Referanslar kaybolursa (re-spawn vb.) tekrar bul
        if (statController == null)
        {
            SetupReferences();
            return;
        }

        float suAnkiPara = statController.checkMoney();
        string yeniYazi = suAnkiPara.ToString("F0");

        // 2 saniyede bir durum raporu ver (Döngü çalışıyor mu anlamak için)
        logTimer += Time.deltaTime;
        if (logTimer >= 2f)
        {
            //Debug.Log($"<color=white>ShowMoney Döngüde: Script içindeki para: {suAnkiPara}, UI Metni: {paraYazisi.text}</color>");
            logTimer = 0f;
        }

        // Eğer UI metni güncel değilse güncelle
        if (paraYazisi != null && paraYazisi.text != yeniYazi)
        {
           // Debug.Log($"<color=yellow>ShowMoney: UI GÜNCELLENDİ! Eski: {paraYazisi.text}, Yeni: {yeniYazi}</color>");
            paraYazisi.text = yeniYazi;
        }
    }
}