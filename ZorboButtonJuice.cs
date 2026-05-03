using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ZorboButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rect;
    private Vector2 originalPos;
    private Vector3 originalScale;

    [Header("Mıknatıs ve Büyüme")]
    public float scaleMultiplier = 1.05f; 
    public float moveStrength = 10f;    
    public float lerpSpeed = 12f;

    [Header("Yazı Glitch Efekti")]
    public bool alwaysShowText = false;  // YENİ AYAR: True ise yazı hep görünür
    public bool enableGlitch = true; 
    public Graphic targetGraphic;        
    public float glitchDuration = 0.5f;  
    public Color[] glitchColors = { Color.cyan, Color.magenta, Color.yellow, Color.red, Color.white };
    
    public AnimationCurve alphaCurve = new AnimationCurve(
        new Keyframe(0f, 1f), 
        new Keyframe(0.2f, 0.3f), 
        new Keyframe(0.4f, 0.9f), 
        new Keyframe(0.7f, 0.1f), 
        new Keyframe(0.85f, 0.8f),
        new Keyframe(1f, 1f)
    );

    private bool isHovering = false;
    private bool isPressed = false; 
    private Vector2 targetPos;
    private Vector3 targetScale;
    private Color originalGraphicColor;
    private Coroutine glitchRoutine;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalPos = rect.anchoredPosition;
        originalScale = transform.localScale;
        targetPos = originalPos;
        targetScale = originalScale;

        // ÖNEMLİ: Eğer yazı başlangıçta kapalıysa Awake rengi yanlış alabilir.
        // Bu yüzden eğer renk siyah/şeffaf gelirse manuel bir renk atıyoruz.
        if (targetGraphic != null) 
        {
            originalGraphicColor = targetGraphic.color;
            // Eğer kaydedilen renk tamamen şeffafsa, görünür olması için alfayı 1 yapalım
            if(originalGraphicColor.a <= 0.1f) originalGraphicColor.a = 1f;
        }
    }

    void OnEnable()
    {
        isHovering = false;
        isPressed = false;
        ResetPosition();

        if (glitchRoutine != null) StopCoroutine(glitchRoutine);
        
        if (targetGraphic != null)
        {
            // YENİ: Ayar açıksa başlangıçta açık bırak, kapalıysa gizle
            targetGraphic.gameObject.SetActive(alwaysShowText); 
            if (alwaysShowText) 
            {
                targetGraphic.color = originalGraphicColor;
            }
        }
    }

    void OnDisable()
    {
        isHovering = false;
        isPressed = false;
    }

    void Update()
    {
        HandleMovement();
    }

    public void OnPointerEnter(PointerEventData eventData) 
    {
        isHovering = true;
        if (targetGraphic != null)
        {
            // Obje kapalıysa aç (alwaysShowText false ise devreye girer)
            if (!targetGraphic.gameObject.activeSelf) 
            {
                targetGraphic.gameObject.SetActive(true); 
            }
            
            // Yazının rengini ve alfasını anında sıfırla ki görünür olsun
            targetGraphic.color = originalGraphicColor; 
            
            if (enableGlitch) StartGlitch();
        }
    }

    public void OnPointerExit(PointerEventData eventData) 
    {
        isHovering = false;
        if (glitchRoutine != null) StopCoroutine(glitchRoutine);
        
        if (targetGraphic != null)
        {
            if (alwaysShowText) 
            {
                // YENİ: Yazı hep kalacaksa, glitch yarıda kesildiğinde rengi düzelt
                targetGraphic.color = originalGraphicColor;
            } 
            else 
            {
                // YENİ: Sadece alwaysShowText kapalıysa yazıyı gizle
                targetGraphic.gameObject.SetActive(false); 
            }
        }
    }

    private void HandleMovement()
    {
        if (isPressed) { targetScale = originalScale * 0.95f; targetPos = originalPos; }
        else if (isHovering)
        {
            targetScale = originalScale * scaleMultiplier;
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector2 buttonCenter = (corners[0] + corners[2]) / 2f;
            Vector2 mousePos = Input.mousePosition;
            Vector2 dir = (mousePos - buttonCenter);
            float dist = Mathf.Clamp(dir.magnitude, 0, moveStrength);
            targetPos = originalPos + (dir.normalized * dist);
        }
        else { targetPos = originalPos; targetScale = originalScale; }

        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPos, Time.unscaledDeltaTime * lerpSpeed);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * lerpSpeed);
    }

    private void ResetPosition()
    {
        if (rect != null) rect.anchoredPosition = originalPos;
        transform.localScale = originalScale;
        targetPos = originalPos;
        targetScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData) { isPressed = true; }
    public void OnPointerUp(PointerEventData eventData) { isPressed = false; }

    private void StartGlitch()
    {
        if (glitchRoutine != null) StopCoroutine(glitchRoutine);
        glitchRoutine = StartCoroutine(GlitchEffectRoutine());
    }

    IEnumerator GlitchEffectRoutine()
    {
        float elapsed = 0f;
        while (elapsed < glitchDuration)
        {
            elapsed += Time.unscaledDeltaTime; 
            float t = elapsed / glitchDuration;
            float currentAlpha = alphaCurve.Evaluate(t) * originalGraphicColor.a; 

            if (t < 0.85f)
            {
                if (Time.frameCount % 4 == 0)
                {
                    Color randColor = glitchColors[Random.Range(0, glitchColors.Length)];
                    randColor.a = currentAlpha;
                    targetGraphic.color = randColor;
                }
                else
                {
                    Color c = targetGraphic.color;
                    c.a = currentAlpha;
                    targetGraphic.color = c;
                }
            }
            else
            {
                targetGraphic.color = originalGraphicColor;
            }
            yield return null;
        }
        targetGraphic.color = originalGraphicColor;
    }
}