using System.Collections;
using UnityEngine;
using TMPro; // TextMeshPro kullanmak için bu kütüphane şart

public class DaktiloEfekti : MonoBehaviour
{
    [Header("Ayarlar")]
    public TextMeshProUGUI hikayeTextObjesi; // Yazının çıkacağı UI objesi
    
    [TextArea(5, 10)] // Unity editöründe yazıyı rahat girmek için metin kutusunu büyütür
    public string yazilacakHikaye; 
    
    public float yazmaHizi = 0.05f; // Harfler arası bekleme süresi (ne kadar küçük o kadar hızlı)

    void Start()
    {
        // Oyun başladığında metin kutusunu temizle
        hikayeTextObjesi.text = ""; 
        
        // Daktilo efektini başlatan Coroutine'i çağır
        StartCoroutine(HikayeyiYazdir());
    }

    IEnumerator HikayeyiYazdir()
    {
        // Metindeki her bir harfi tek tek döngüye sok
        foreach (char harf in yazilacakHikaye)
        {
            hikayeTextObjesi.text += harf; // Harfi mevcut yazıya ekle
            yield return new WaitForSeconds(yazmaHizi); // Belirlenen süre kadar bekle
        }
    }
}