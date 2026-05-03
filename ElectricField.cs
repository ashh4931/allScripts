using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ElectricField : MonoBehaviour
{
    [Header("UI Ayarları")]
    [SerializeField] private GameObject chargeUIPanel;
    [SerializeField] private TextMeshProUGUI chargeValueText;

    [Header("Prefab ve Sesler")]
    [SerializeField] private GameObject shockPrefab;
    [SerializeField] private GameObject electricFieldVFXPrefab;
    [SerializeField] private AudioClip electricFieldSound;
    [SerializeField] private AudioClip openingSound;

    [Header("Dengeleme ve Hasar (Inspector'dan Değiştir!)")]
    [Tooltip("Saniye başına verilen temel hasar")]
    public float damageAmount = 15f;
    [Tooltip("Şarj olma hızı")]
    public float growthRate = 2.0f;
    [Tooltip("Temel mana maliyeti")]
    public float manaCostBase = 1.5f;
    [Tooltip("Mana maliyeti üstel katsayısı")]
    public float manaCostExponent = 1.3f;

    [Header("İnce Ayar (Mesafe Senkronizasyonu)")]
    [Range(0.1f, 2f)]
    [Tooltip("VFX görselinin collider ile tam eşleşmesi için çarpan (Görsel küçük kalıyorsa bunu artır)")]
    public float vfxVisualMultiplier = 1.0f;

    private StatController statController;
    private PlayerStats playerStats;
    private AudioSource electricFieldAudio;
    private AudioSource openingAudio;
    private GameObject VFX;

    private bool isActive = false;
    private bool isCharging = false;
    public bool IsActive => isActive;

    private string activeInputKey;
    private float finalFieldSize; // Bu bizim 'Yarıçap' (Radius) değerimiz olacak
    private Dictionary<GameObject, Coroutine> activeShocks = new Dictionary<GameObject, Coroutine>();

    void Awake()
    {
        statController = GetComponentInParent<StatController>();
        playerStats = GetComponentInParent<PlayerStats>();

        electricFieldAudio = gameObject.AddComponent<AudioSource>();
        electricFieldAudio.playOnAwake = false;
        electricFieldAudio.loop = true;

        openingAudio = gameObject.AddComponent<AudioSource>();
        openingAudio.playOnAwake = false;
        openingAudio.loop = false;

        if (chargeUIPanel != null) chargeUIPanel.SetActive(false);
    }

    public void BeginCharge(string inputKey)
    {
        if (isActive || isCharging) return;
        activeInputKey = inputKey;
        StartCoroutine(ChargeRoutine());
    }

    private IEnumerator ChargeRoutine()
    {
        isCharging = true;
        float currentChargeSize = 1.0f;
        float manaAtStart = playerStats.mana;

        if (chargeUIPanel != null) chargeUIPanel.SetActive(true);

        while (InputManager.GetKey(activeInputKey))
        {
            // Mana Maliyeti Formülü: $Cost = Base \cdot Size^{Exponent} \cdot \Delta t$
            float predictedCostPerSecond = manaCostBase * Mathf.Pow(currentChargeSize, manaCostExponent);

            if (predictedCostPerSecond < manaAtStart)
            {
                currentChargeSize += growthRate * Time.deltaTime;
            }

            if (chargeValueText != null)
                chargeValueText.text = "Charge: " + currentChargeSize.ToString("F1");

            yield return null;
        }

        isCharging = false;
        if (chargeUIPanel != null) chargeUIPanel.SetActive(false);

        finalFieldSize = currentChargeSize;
        ActivateField();
    }

    private void ActivateField()
    {
        isActive = true;
        
        if (openingSound != null) openingAudio.PlayOneShot(openingSound);
        electricFieldAudio.clip = electricFieldSound;
        electricFieldAudio.Play();

        // 1. GÖRSEL OLUŞTURMA
        VFX = Instantiate(electricFieldVFXPrefab, transform.position, Quaternion.identity);
        VFX.transform.SetParent(this.transform); 
        VFX.layer = LayerMask.NameToLayer("PlayerSkill");

        // 2. SENKRONİZASYON MANTIĞI
        // VFX'in kendi scale'ini ayarla (Eğer prefab çok büyük/küçükse vfxVisualMultiplier ile Inspector'dan düzelt)
        VFX.transform.localScale = Vector3.one * vfxVisualMultiplier;

        // Skill Objesinin Scale'ini finalFieldSize yapıyoruz.
        // Bu, hem görseli (VFX) hem de bu objedeki Collider'ı aynı oranda büyütür.
        this.transform.localScale = Vector3.one * finalFieldSize;

        // 3. COLLIDER AYARI
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
        
        col.isTrigger = true;
        col.enabled = true;
        col.radius = 1f; // Scale zaten finalFieldSize olduğu için radius 1f = dünya ölçeğinde tam olarak finalFieldSize birim.

        // 4. ALAN TARAMASI (OverlapCircle radius artık tam olarak finalFieldSize ile eşleşecek)
        Collider2D[] insideEnemies = Physics2D.OverlapCircleAll(transform.position, finalFieldSize, LayerMask.GetMask("Enemy"));
        foreach (var enemy in insideEnemies)
        {
            if (!activeShocks.ContainsKey(enemy.gameObject))
            {
                IDamageable dmg = enemy.GetComponentInParent<IDamageable>();
                if (dmg != null)
                {
                    Coroutine shockCoroutine = StartCoroutine(ApplyShockContinuously(enemy.gameObject));
                    activeShocks.Add(enemy.gameObject, shockCoroutine);
                }
            }
        }

        StartCoroutine(DrainMana());
    }

    public void StopSkill()
    {
        isActive = false;
        if (electricFieldAudio != null) electricFieldAudio.Stop();
        if (VFX != null) Destroy(VFX);

        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null) col.enabled = false;

        // Scale'i resetle ki bir sonraki açılışta devasa başlamasın
        this.transform.localScale = Vector3.one;

        activeShocks.Clear();
    }

    private IEnumerator DrainMana()
    {
        while (isActive)
        {
            float manaCost = manaCostBase * Mathf.Pow(finalFieldSize, manaCostExponent) * Time.deltaTime;
            statController.UseMana(manaCost);

            if (playerStats.mana <= 0)
            {
                StopSkill();
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator ApplyShockContinuously(GameObject target)
    {
        while (isActive && target != null)
        {
            IDamageable damageable = target.GetComponentInParent<IDamageable>();
            if (shockPrefab != null)
            {
                GameObject shock = Instantiate(shockPrefab, target.transform.position, Quaternion.identity);
                shock.transform.SetParent(target.transform);
            }

            if (damageable != null)
            {
                damageable.TakeDamage(damageAmount, damageAmount, damageAmount);
            }

            yield return new WaitForSeconds(1f);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isActive && !activeShocks.ContainsKey(collision.gameObject))
        {
            IDamageable damageable = collision.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                Coroutine shockCoroutine = StartCoroutine(ApplyShockContinuously(collision.gameObject));
                activeShocks.Add(collision.gameObject, shockCoroutine);
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (activeShocks.TryGetValue(collision.gameObject, out Coroutine shockCoroutine))
        {
            StopCoroutine(shockCoroutine);
            activeShocks.Remove(collision.gameObject);
        }
    }
}