using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class AtmosphereManager : MonoBehaviour
{
    [Header("Post Processing Ayarları")]
    public Volume globalVolume;
    private LiftGammaGain _liftGammaGain;

    [Header("Boss Dalga Gece Ayarları (Senin Değerlerin)")]
    public Vector4 bossLift = new Vector4(0.7102129f, 0.7958285f, 1f, -0.07736944f);
    public Vector4 bossGamma = new Vector4(0.981627f, 0.8872262f, 1f, -0.1160541f);
    public Vector4 bossGain = new Vector4(0.821689f, 0.8851088f, 1f, -0.1289488f);

    [Header("Geçiş Ayarları")]
    public float transitionDuration = 3f; // 3 saniyede geceye dönecek

    // Normal dalgalardaki aydınlık değerleri hafızada tutacağız
    private Vector4 _normalLift;
    private Vector4 _normalGamma;
    private Vector4 _normalGain;

    private Coroutine _transitionCoroutine;

    private void Awake()
    {
        // Volume içinden Lift Gamma Gain efektini bulup alıyoruz
        if (globalVolume.profile.TryGet(out _liftGammaGain))
        {
            // Oyun başladığında, Unity arayüzündeki mevcut (gündüz) ayarlarını hafızaya kaydediyoruz 
            // ki Boss bitince geri dönebilelim.
            _normalLift = _liftGammaGain.lift.value;
            _normalGamma = _liftGammaGain.gamma.value;
            _normalGain = _liftGammaGain.gain.value;
        }
        else
        {
            Debug.LogWarning("[AtmosphereManager] Volume profilinde Lift Gamma Gain bulunamadı! Lütfen ekleyin.");
        }
    }

    private void OnEnable()
    {
        WaveEvents.OnWaveStarted += HandleWaveStarted;
        WaveEvents.OnWaveFinished += HandleWaveFinished;
    }

    private void OnDisable()
    {
        WaveEvents.OnWaveStarted -= HandleWaveStarted;
        WaveEvents.OnWaveFinished -= HandleWaveFinished;
    }

    private void HandleWaveStarted(int waveIndex, int wavePoints, WaveType type)
    {
        if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);

        if (type == WaveType.Boss)
        {
            // Boss geldiyse, senin belirlediğin gece değerlerine geçiş yap
            _transitionCoroutine = StartCoroutine(TransitionLGG(bossLift, bossGamma, bossGain));
        }
        else
        {
            // Normal dalgaysa gündüz değerlerine geri dön
            _transitionCoroutine = StartCoroutine(TransitionLGG(_normalLift, _normalGamma, _normalGain));
        }
    }

    private void HandleWaveFinished()
    {
        if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);
        
        // Dalga bittiğinde (dinlenme aşamasında) gündüze dön
        _transitionCoroutine = StartCoroutine(TransitionLGG(_normalLift, _normalGamma, _normalGain));
    }

    // Üç değeri aynı anda yumuşakça değiştiren Coroutine
    private IEnumerator TransitionLGG(Vector4 targetLift, Vector4 targetGamma, Vector4 targetGain)
    {
        if (_liftGammaGain == null) yield break;

        // Geçişin başlayacağı anki mevcut değerleri alıyoruz
        Vector4 startLift = _liftGammaGain.lift.value;
        Vector4 startGamma = _liftGammaGain.gamma.value;
        Vector4 startGain = _liftGammaGain.gain.value;
        
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;

            // Vector4.Lerp ile dört değeri (X,Y,Z,W) tek seferde yumuşakça hedefe doğru çekiyoruz
            _liftGammaGain.lift.value = Vector4.Lerp(startLift, targetLift, t);
            _liftGammaGain.gamma.value = Vector4.Lerp(startGamma, targetGamma, t);
            _liftGammaGain.gain.value = Vector4.Lerp(startGain, targetGain, t);
            
            yield return null; 
        }

        // Geçiş bitince hedef değerlere tam olarak oturduğundan emin oluyoruz
        _liftGammaGain.lift.value = targetLift;
        _liftGammaGain.gamma.value = targetGamma;
        _liftGammaGain.gain.value = targetGain;
    }
}