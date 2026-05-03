using UnityEngine;

public class ExitGame : MonoBehaviour
{
    public void Quit()
    {
        // Oyunun kapandığını test etmek için konsola yazdırır
        Debug.Log("Oyundan çıkılıyor...");

        // Gerçek oyun dosyasında uygulamayı kapatır
        Application.Quit();

        // Eğer Unity Editör içindeysek, Play modunu durdurur (Opsiyonel)
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}