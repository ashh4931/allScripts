using UnityEngine;
using TMPro; // TextMeshPro için gerekli
using UnityEngine.Localization.Settings; // Unity Localization Package kullanıyorsan
using System.Collections;

public class LanguageManager : MonoBehaviour
{
    public TMP_Dropdown languageDropdown;

    void Start()
    {
        // Oyun açıldığında Dropdown'ın mevcut dile göre ayarlanması
        int ID = PlayerPrefs.GetInt("LocaleKey", 0);
        languageDropdown.value = ID;
    }

    // Bu fonksiyonu Dropdown'ın "On Value Changed" kısmına bağlayacağız
    public void OnLanguageChanged(int index)
    {
        StartCoroutine(SetLocale(index));
    }

    IEnumerator SetLocale(int _localeID)
    {
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[_localeID];
        
        // Seçimi kaydetmek için (Opsiyonel)
        PlayerPrefs.SetInt("LocaleKey", _localeID);
    }
    
}