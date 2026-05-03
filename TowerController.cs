using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TowerController : MonoBehaviour
{
    [System.Serializable]
    public class TowerPartData
    {
        public GameObject partObject;
        public Vector3 originalLocalPos;
        public SpriteRenderer spriteRenderer;
    }

    [Header("Referanslar")]
    public GameObject corePrefab;
    public GameObject foundationPrefab;
    public GameObject subHolderPrefab;
    public GameObject coreHolderPrefab;
    public GameObject circlePrefab;

    [Header("Ses Efektleri")]
    public AudioClip partDestroySFX;
    public AudioClip coreDestroySFX;
    public AudioClip towerDestroySFX;

    [Header("Yıkım Alanı Ayarları")]
    [Tooltip("Kuleyi yok etmek için gereken dairesel mesafe (yarıçap).")]
    public float detectionRadius = 3f; // Dikdörtgen yerine artık yarıçap kullanıyoruz
    [Tooltip("Yıkım alanının merkeze olan uzaklığı.")]
    public Vector2 detectionOffset = new Vector2(0f, 0f);

    [Header("Kule Ayarları")]
    public float manaDrainRate = 10f;
    public float foundationHealth = 100f;
    private float currentFloorProgress = 0f;

    [Header("Mesafe Ayarları")]
    public float floorToSubHolderY = 2.1f;
    // public float subHolderToMidPartY = 0.5f; // BU KAT KALDIRILDI
    public float midPartToCoreHolderGap = 0.4f;
    public float coreHolderToCoreY = 0.6f;

    private List<TowerPartData> activePartsData = new List<TowerPartData>();
    private GameObject spawnedCircle;
    private Vector3 initialCircleScale;

    private PlayerStats player;
    private StatController playerStatController;
    private bool isBeingDestroyed = false;
    public void SetupTower()
    {
        activePartsData.Clear();
        int sortingOrder = 1;

        if (circlePrefab != null)
        {
            spawnedCircle = Instantiate(circlePrefab, transform.position, Quaternion.identity, transform);
            initialCircleScale = spawnedCircle.transform.localScale;
        }

        float foundationY = 0f;
        float subY = foundationY + floorToSubHolderY;
        // MidY katı çıkarıldı, doğrudan Holder hesaplanıyor
        float holderY = subY + midPartToCoreHolderGap;
        float coreY = holderY + coreHolderToCoreY;

        // YIKILMA SIRASI (Bir kat eksiltildi)
        AddPartToList(coreHolderPrefab, holderY, sortingOrder + 5, "Holder");
        // "Mid" parçası AddPartToList(subHolderPrefab...) buradan silindi.
        AddPartToList(subHolderPrefab, subY, sortingOrder + 3, "Sub");
        AddPartToList(corePrefab, coreY, sortingOrder + 10, "Core");
        AddPartToList(foundationPrefab, foundationY, sortingOrder, "Foundation");
    }
    void AddPartToList(GameObject prefab, float y, int order, string nameTag)
    {
        Vector3 spawnPos = new Vector3(0, y, 0);
        GameObject obj = Instantiate(prefab, transform.position + spawnPos, Quaternion.identity, transform);
        obj.name = nameTag;

        TowerPartData data = new TowerPartData();
        data.partObject = obj;
        data.originalLocalPos = spawnPos;
        data.spriteRenderer = obj.GetComponentInChildren<SpriteRenderer>();
        if (data.spriteRenderer != null) data.spriteRenderer.sortingOrder = order;

        activePartsData.Add(data);
    }

    void Update()
    {
        if (player == null) player = FindObjectOfType<PlayerStats>();
        if (playerStatController == null && player != null) playerStatController = player.GetComponent<StatController>();
        if (player == null) return;

        // --- DAİRE KONTROLÜ (YENİ) ---
        Vector2 center = (Vector2)transform.position + detectionOffset;
        float distance = Vector2.Distance(player.transform.position, center);

        if (distance <= detectionRadius)
        {
            if (HintManager.Instance != null)
            {
              
              //  HintManager.Instance.ShowHint("tower_prox", "Kulenin dairesine girdin! Burada bekleyerek kuleyi aşındırabilirsin.", "YIKIM ALANI", false, 4f);
            }

            isBeingDestroyed = true;
            HandleDestruction();
        }
        else
        {
            if (isBeingDestroyed) ResetVisualEffects();
            isBeingDestroyed = false;
        }

        HandleCoreHover();
    }

    void HandleDestruction()
    {
        bool isFastPressing = InputManager.GetKey("FastDestroy");
        float speed = isFastPressing ? 2.5f : 1f;
        if (Input.GetKey(KeyCode.F))
            playerStatController.TakeDamage(manaDrainRate * 1.5f * Time.deltaTime);
        else if (player.mana > 0)
            player.mana -= manaDrainRate * Time.deltaTime;
        else { ResetVisualEffects(); return; }

        ApplyGlitch();
        UpdateCircle();

        currentFloorProgress += 20f * speed * Time.deltaTime;

        if (currentFloorProgress >= foundationHealth) DestroyNextPart();
    }

    void UpdateCircle()
    {
        if (spawnedCircle == null) return;
        float progressPercent = 1f - (currentFloorProgress / foundationHealth);
        spawnedCircle.transform.localScale = initialCircleScale * progressPercent;
    }

    void DestroyNextPart()
    {
        if (activePartsData.Count == 0) return;

        GameObject objToDestroy = activePartsData[0].partObject;
        string partName = objToDestroy.name;

        activePartsData.RemoveAt(0);
        Destroy(objToDestroy);

        currentFloorProgress = 0;
        if (spawnedCircle != null) spawnedCircle.transform.localScale = initialCircleScale;

        if (AudioManager.instance != null)
        {
            if (partName == "Core" && coreDestroySFX != null)
                AudioManager.instance.PlaySFXAtPosition(coreDestroySFX, transform.position, 1.1f, 1f);
            else if (partDestroySFX != null)
                AudioManager.instance.PlaySFXAtPosition(partDestroySFX, transform.position, 1f, Random.Range(0.9f, 1.1f));
        }

        ApplyImpactEffects();

        if (activePartsData.Count == 0)
        {
            if (AudioManager.instance != null && towerDestroySFX != null)
                AudioManager.instance.PlaySFXAtPosition(towerDestroySFX, transform.position, 1.2f, 0.9f);

            Destroy(gameObject);
        }
    }

    void ApplyGlitch()
    {
        foreach (var data in activePartsData)
        {
            if (data.partObject == null) continue;
            if (Random.value < 0.05f && data.spriteRenderer != null)
                data.spriteRenderer.color = new Color(Random.value, 0.8f, 1f, 1f);

            Vector3 jitter = (Vector3)Random.insideUnitCircle * 0.04f;
            data.partObject.transform.localPosition = data.originalLocalPos + jitter;
        }
    }

    void ResetVisualEffects()
    {
        foreach (var data in activePartsData)
        {
            if (data.partObject == null) continue;
            data.partObject.transform.localPosition = data.originalLocalPos;
            if (data.spriteRenderer != null) data.spriteRenderer.color = Color.white;
        }
    }

    void HandleCoreHover()
    {
        foreach (var data in activePartsData)
        {
            if (data.partObject != null && data.partObject.name == "Core")
            {
                float bob = Mathf.Sin(Time.time * 2f) * 0.1f;
                data.partObject.transform.localPosition = data.originalLocalPos + new Vector3(0, bob, 0);
                break;
            }
        }
    }

    void ApplyImpactEffects()
    {
        StartCoroutine(ShakeCamera(0.15f, 0.2f));
    }

    IEnumerator ShakeCamera(float d, float m)
    {
        Vector3 p = Camera.main.transform.localPosition;
        float e = 0;
        while (e < d) { Camera.main.transform.localPosition = p + (Vector3)Random.insideUnitCircle * m; e += Time.deltaTime; yield return null; }
        Camera.main.transform.localPosition = p;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere((Vector2)transform.position + detectionOffset, detectionRadius);
    }
}