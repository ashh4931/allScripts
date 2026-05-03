using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // URP için gerekli

public class LiftGammaGainReader : MonoBehaviour
{
    public Volume globalVolume;

    // Bu etiket sayesinde oyunu başlatmana bile gerek kalmadan, 
    // Inspector'da sağ tıklayıp bu metodu çalıştırabileceğiz!
    [ContextMenu("Değerleri Konsola Yazdır")]
    public void DegerleriYazdir()
    {
        if (globalVolume == null)
        {
            Debug.LogError("Lütfen Global Volume objesini atayın!");
            return;
        }

        if (globalVolume.profile.TryGet(out LiftGammaGain lgg))
        {
            Debug.Log("<color=cyan>--- BOSS WAVE GECE AYARLARI ---</color>");
            // Lift, Gamma ve Gain değerleri arka planda Vector4 (X, Y, Z, W) olarak tutulur.
            // X, Y, Z = Kırmızı, Yeşil, Mavi renk dengesi
            // W = Altındaki kaydırıcı (parlaklık) değeri
            Debug.Log($"<b>Lift:</b> new Vector4({lgg.lift.value.x}f, {lgg.lift.value.y}f, {lgg.lift.value.z}f, {lgg.lift.value.w}f);");
            Debug.Log($"<b>Gamma:</b> new Vector4({lgg.gamma.value.x}f, {lgg.gamma.value.y}f, {lgg.gamma.value.z}f, {lgg.gamma.value.w}f);");
            Debug.Log($"<b>Gain:</b> new Vector4({lgg.gain.value.x}f, {lgg.gain.value.y}f, {lgg.gain.value.z}f, {lgg.gain.value.w}f);");
            Debug.Log("<color=cyan>-------------------------------</color>");
        }
        else
        {
            Debug.LogError("Volume profilinde Lift, Gamma, Gain efekti bulunamadı!");
        }
    }
}