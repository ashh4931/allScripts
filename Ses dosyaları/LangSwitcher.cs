using UnityEngine;

 
using UnityEngine;
using UnityEngine.Localization.Settings; // Localization için gerekli
using System.Collections;

public class LangSwitcher : MonoBehaviour
{
    private bool isChanging = false;

    void Update()
    {
        // X tuşuna basınca bir sonraki dile geç
       if (InputManager.GetKeyDown("ChangeLanguage") && !isChanging)
        {
            StartCoroutine(ChangeToNextLocale());
        }
    }

    IEnumerator ChangeToNextLocale()
    {
        isChanging = true;

        // Mevcut dillerin listesini al
        var locales = LocalizationSettings.AvailableLocales.Locales;
        if (locales.Count <= 1) yield break;

        // Şu anki dilin indeksini bul
        int currentIndex = locales.IndexOf(LocalizationSettings.SelectedLocale);
        
        // Bir sonraki indeksi hesapla (liste sonuna gelince başa dön)
        int nextIndex = (currentIndex + 1) % locales.Count;

        // Yeni dili ayarla
        LocalizationSettings.SelectedLocale = locales[nextIndex];

        Debug.Log($"Dil Değiştirildi: {locales[nextIndex].LocaleName}");

        // Küçük bir bekleme süresi (spam engellemek için)
        yield return new WaitForSeconds(0.2f);
        isChanging = false;
    }
}