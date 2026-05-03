using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class ZorboAngrySizeInteract : MonoBehaviour, IPointerClickHandler
{
    private Image bodyImage;
    private Color originalColor;
    
    private ZorboBlinker blinker; 
    private ZorboTotalScreenImpact screenImpact; 

    [Header("Göz Bebekleri (Image & RectTransform)")]
    public Image leftEyeImage;
    public Image rightEyeImage;

    [Header("Normal Sprite'lar")]
    public Sprite standardLeftEyeSprite; 
    public Sprite standardRightEyeSprite; 

    [Header("Kızgın Sprite'lar")]
    public Sprite angryLeftEyeSprite; 
    public Sprite angryRightEyeSprite; 

    [Header("Boyut Ayarları")]
    public float angryHeight = 180f; // İstediğin yükseklik
    private Vector2 originalSize; // Başlangıçtaki genişlik ve yüksekliği saklar

    [Header("Yaralanma Ayarları")]
    public GameObject[] woundPrefabs; 
    public Color flashColor = Color.red; 
    public float recoverSpeed = 5f;
    public float angerDuration = 3f; 

    void Awake()
    {
        bodyImage = GetComponent<Image>();
        originalColor = bodyImage.color;
        
        blinker = GetComponentInParent<ZorboBlinker>();
        screenImpact = FindObjectOfType<ZorboTotalScreenImpact>();

        // Başlangıç boyutunu kaydediyoruz (Geri dönmek için)
        if (leftEyeImage != null)
            originalSize = leftEyeImage.rectTransform.sizeDelta;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        bodyImage.color = flashColor;
        SpawnWound();

        if(screenImpact) screenImpact.enabled = true;

        StopAllCoroutines(); 
        StartCoroutine(BecomeAngryRoutine());
    }

    void Update()
    {
        bodyImage.color = Color.Lerp(bodyImage.color, originalColor, Time.deltaTime * recoverSpeed);
    }

    IEnumerator BecomeAngryRoutine()
    {
        if (blinker) blinker.enabled = false;

        // KIZGIN MOD: Sprite değiştir ve Yüksekliği 180 yap
        SetEyesAppearance(angryLeftEyeSprite, angryRightEyeSprite, angryHeight);

        yield return new WaitForSeconds(angerDuration);

        // NORMAL MOD: Sprite'ı ve Boyutu eski haline döndür
        SetEyesAppearance(standardLeftEyeSprite, standardRightEyeSprite, originalSize.y);
        
        if (blinker) blinker.enabled = true; 
    }

    // Hem sprite'ı hem de yüksekliği aynı anda değiştiren yardımcı fonksiyon
    void SetEyesAppearance(Sprite leftSprite, Sprite rightSprite, float targetHeight)
    {
        if (leftEyeImage != null && rightEyeImage != null)
        {
            // Sprite'ları değiştir
            leftEyeImage.sprite = leftSprite;
            rightEyeImage.sprite = rightSprite;

            // Yüksekliği (Height) değiştir (Genişlik/Width aynı kalır)
            leftEyeImage.rectTransform.sizeDelta = new Vector2(originalSize.x, targetHeight);
            rightEyeImage.rectTransform.sizeDelta = new Vector2(originalSize.x, targetHeight);
        }
    }

    void SpawnWound()
    {
        if (woundPrefabs == null || woundPrefabs.Length == 0) return;
        int randomIndex = Random.Range(0, woundPrefabs.Length);
        GameObject selectedWound = Instantiate(woundPrefabs[randomIndex], Vector3.zero, Quaternion.identity);
        selectedWound.transform.SetParent(this.transform, false); 
        selectedWound.transform.localPosition = new Vector3(Random.Range(-50f, 50f), Random.Range(-50f, 50f), 0f);
        selectedWound.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
    }
}