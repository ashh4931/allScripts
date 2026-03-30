using UnityEngine;
using TMPro;
using System.Collections;

public class ShowMoney : MonoBehaviour
{
    private TextMeshProUGUI paraYazisi;
    public GameObject Player;

    void Awake()
    {
        paraYazisi = GetComponent<TextMeshProUGUI>();
    }
public void UpdateAmount()
{
    if (paraYazisi != null && Player != null)
    {
        string yeniDeger = Player.GetComponent<StatController>().checkMoney().ToString();

        // Eğer eski yazı ile yeni yazı farklıysa (yani para toplandıysa)
        if (paraYazisi.text != yeniDeger)
        {
            paraYazisi.text = yeniDeger;
            StopCoroutine("YaziAnimasyonu"); // Varsa önceki animasyonu durdur
            StartCoroutine(YaziAnimasyonu()); // Yeni animasyonu başlat
        }
    }
}

IEnumerator YaziAnimasyonu()
{
    // Yazıyı %20 büyüt
    paraYazisi.transform.localScale = Vector3.one * 1.2f;
    paraYazisi.color = Color.yellow; // Rengini geçici olarak sarı yap

    // 0.1 saniye bekle
    yield return new WaitForSeconds(0.1f);

    // Eski haline yavaşça döndür (Yumuşak geçiş için)
    float t = 0;
    while (t < 1)
    {
        t += Time.deltaTime * 5f;
        paraYazisi.transform.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one, t);
        paraYazisi.color = Color.Lerp(Color.yellow, Color.white, t);
        yield return null;
    }
}
 

    void Start()
    {
        StartCoroutine(ParaGuncellemeDongusu());
    }

    IEnumerator ParaGuncellemeDongusu()
    {
        while (true)
        {
            UpdateAmount();
            yield return new WaitForSeconds(0.2f); 
        }
    }
}