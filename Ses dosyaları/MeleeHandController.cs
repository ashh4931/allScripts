using System.Collections;
using UnityEngine;

public class MeleeHandController : MonoBehaviour
{
    public bool isSpaceTraveling = false;
    private float originalHandOffset;
    private SpriteRenderer spriteRenderer;

    [Header("Combo Ayarları")]
    public int comboStep = 0;
    public float comboResetTime = 1f;   // Animasyon bittikten sonraki bekleme süresi
    private float lastAttackEndTime;    // lastClickTime yerine bunu kullanacağız

    public Animator animator;
    private bool isAttacking = false;
    public bool isIdle = true;

    [Header("Silah Verisi")]
    public New_WeaponData currentWeaponData;
    public GameObject currentWeapon;

    [Header("Bağlantılar")]
    public Transform bodyCenter;

    [Header("Pozisyon Ayarları")]
    public float handOffsetDistance = 1.2f;
    public float followSpeed = 15f;
    
    [Header("Efektler")]
    public TrailRenderer swordTrail;
    private Vector2 lastFacingDir = Vector2.right;

    // --- YENİ EKLENEN SES DEĞİŞKENLERİ ---
    [Header("Ses Ayarları")]
    public AudioSource audioSource;
    public AudioClip attackSound;
    [Range(0.5f, 1.5f)] public float minPitch = 0.9f;
    [Range(0.5f, 1.5f)] public float maxPitch = 1.1f;
    // -------------------------------------

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalHandOffset = handOffsetDistance;

        // Eğer AudioSource atanmamışsa, bu objede var mı diye kontrol et
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void Start()
    {
        uzayaGönder();
    }

    void Update()
    {
        // Eğer silahsızsa hiçbir işlem yapma
        if (!GetComponentInParent<karakterWeaponController>().isHoldingMeleeWeapon)
            return;

        if (!isSpaceTraveling)
            HandleMovement();

        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
            Debug.Log("MeleeHandController: Sol tıklandı, saldırı deneniyor. Combo Step: " + comboStep);
        }

        ComboResetControl();
    }

    void TryAttack()
    {
        // Eğer zaten saldırıyorsak, yeni bir saldırı komutu göndermeyi engelleyebiliriz 
        // veya animasyonun kesilmesine izin verebiliriz.
        if (!isAttacking && (Time.time - lastAttackEndTime > comboResetTime))
        {
            comboStep = 0;
        }

        if (comboStep >= 3) comboStep = 0;
        comboStep++;
        comboStep = Mathf.Clamp(comboStep, 1, 3);

        animator.SetInteger("comboStep", comboStep);
        animator.SetTrigger("saldir");

        isAttacking = true;
        isIdle = false;

        // --- YENİ EKLENEN SES OYNATMA KODU ---
        PlayAttackSound();
        // -------------------------------------
    }

    // Ses oynatma metodumuz
    private void PlayAttackSound()
    {
        if (audioSource != null && attackSound != null)
        {
            // Pitch değerini belirlediğimiz min ve max aralığında rastgele seç
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            
            // Sesi oynat (PlayOneShot kullanarak seslerin üst üste binmesine izin veririz)
            audioSource.PlayOneShot(attackSound);
        }
        else
        {
            Debug.LogWarning("MeleeHandController: AudioSource veya AttackSound atanmamış!");
        }
    }

    void ComboResetControl()
    {
        if (!isAttacking && comboStep > 0 && Time.time - lastAttackEndTime > comboResetTime)
        {
            comboStep = 0;
            animator.SetInteger("comboStep", 0);
        }
    }
    
    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
        isIdle = true;
        Debug.Log("<color=yellow>MeleeHandController:</color> Animasyon bitti (Event tetiklendi). Trail kapanıyor...");
        lastAttackEndTime = Time.time;
    }

    public void uzayaGönder()
    {
        isSpaceTraveling = true;
        SetAlpha(0f);
        Debug.Log("El görünmez oldu ve uzaya fırlatıldı!");
    }

    public void dünyayaÇağır()
    {
        handOffsetDistance = originalHandOffset;
        isSpaceTraveling = false;
        SetAlpha(1f);
        Debug.Log("El dünyaya döndü ve görünür oldu!");
    }

    private void SetAlpha(float alpha)
    {
        if (spriteRenderer != null)
        {
            Color tempColor = spriteRenderer.color;
            tempColor.a = alpha;
            spriteRenderer.color = tempColor;
        }

        if (currentWeapon != null)
        {
            SpriteRenderer weaponSR = currentWeapon.GetComponent<SpriteRenderer>();
            if (weaponSR != null)
            {
                Color wColor = weaponSR.color;
                wColor.a = alpha;
                weaponSR.color = wColor;
            }
        }
    }

    void HandleMovement()
    {
        // Mevcut movement kodu
    }
}