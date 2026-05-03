using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;
using System.Collections;

public class LanguageManager : MonoBehaviour
{
    public TMP_Dropdown languageDropdown;

    void Start()
    {
        int ID = PlayerPrefs.GetInt("LocaleKey", 0);
        languageDropdown.value = ID;
        // Başlangıçta hangi dilin yüklendiğini görmek istersen:
       // Debug.Log($"<color=cyan>LanguageManager:</color> Başlangıç dili ID'si: {ID}");
    }

    public void OnLanguageChanged(int index)
    {
        StartCoroutine(SetLocale(index));
    }

   IEnumerator SetLocale(int _localeID)
{
    yield return LocalizationSettings.InitializationOperation;
    
    // Listeden ilgili Locale objesini alıyoruz
    var selectedLocale = LocalizationSettings.AvailableLocales.Locales[_localeID];
    
    // Seçimi uyguluyoruz
    LocalizationSettings.SelectedLocale = selectedLocale;

    // HATANIN ÇÖZÜMÜ: NativeName, Identifier içinde değil direkt selectedLocale içindedir.
    // Eğer o da olmazsa selectedLocale.name kullanabilirsin.
    Debug.Log($"<color=green>Dil Değiştirildi:</color> Index: {_localeID} - Dil: {selectedLocale.LocaleName}");

    PlayerPrefs.SetInt("LocaleKey", _localeID);
}
}