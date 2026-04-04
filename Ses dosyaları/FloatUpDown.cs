using UnityEngine;

public class ItemVisuals : MonoBehaviour
{
    [Header("Referanslar (Sürükle-Bırak)")]
    public SpriteRenderer shadowRenderer; 
    public Transform shadowTransform;    

    [Header("Salınım Ayarları")]
    public float amplitude = 0.25f;
    public float frequency = 1f;

    [Header("Gölge Görsel Ayarlar")]
    public float minScale = 0.7f;  
    public float maxScale = 1.2f;  
    public float minAlpha = 0.2f;  
    public float maxAlpha = 0.8f;  

    private Vector3 startPos; // Yere düştüğündeki GERÇEK merkez noktası
    private Vector3 shadowStartPos; 
    private Vector3 shadowStartScale;
    
    private weapon weaponScript; 
    private Rigidbody2D rb; 
    
    // 🔴 YENİ: Objenin dalgalanma miktarını aklında tutacak
    private float currentYOffset = 0f; 
    private bool wasEquipped;

    void Start()
    {
        weaponScript = GetComponent<weapon>();
        rb = GetComponent<Rigidbody2D>();
        
        if (shadowTransform != null)
        {
            shadowStartScale = shadowTransform.localScale;
            shadowStartPos = shadowTransform.localPosition; 
        }
        
        startPos = transform.position;
    }

    void Update()
    {
        // 1. Eşya Kuşanıldıysa
        if (weaponScript != null && weaponScript.isEquipped)
        {
            wasEquipped = true;
            if (shadowRenderer != null) shadowRenderer.enabled = false; // Eldeyken gölgeyi kapat
            return;
        }

        // 2. Eşya Yere Yeni Bırakıldıysa
        if (wasEquipped)
        {
            if (shadowRenderer != null) shadowRenderer.enabled = true;
            startPos = transform.position;
            currentYOffset = 0f; 
            wasEquipped = false;
        }

        // 🔴 3. Eşya Havada Savruluyorsa (Pinata Patlaması)
        if (rb != null && rb.linearVelocity.magnitude > 0.1f)
        {
            // Kaymayı engellemek için süzülme payını gerçek pozisyondan çıkarıyoruz!
            startPos = transform.position - (Vector3.up * currentYOffset);
            
            // Havada uçarken gölge donup kalmasın diye varsayılan haline getiriyoruz
            ResetShadow();
            return; 
        }

        // 4. Eşya Durduysa Dalgalanmaya Başla
        ApplyFloatingEffect();
    }

    private void ApplyFloatingEffect()
    {
        float sinValue = Mathf.Sin(Time.time * frequency);
        currentYOffset = sinValue * amplitude; // Yüksekliği hafızaya al

        // ANA OBJEYİ HAREKET ETTİR
        transform.position = startPos + Vector3.up * currentYOffset;

        // GÖLGEYİ AYARLA
        if (shadowTransform != null && shadowRenderer != null)
        {
            float t = Mathf.InverseLerp(-1f, 1f, sinValue);

            shadowTransform.localPosition = shadowStartPos + Vector3.down * currentYOffset;
            shadowTransform.localScale = shadowStartScale * Mathf.Lerp(minScale, maxScale, t);

            Color c = shadowRenderer.color;
            c.a = Mathf.Lerp(maxAlpha, minAlpha, t);
            shadowRenderer.color = c;
        }
    }

    // 🔴 YENİ: Uçarken Gölgeyi Düzeltme Fonksiyonu
    private void ResetShadow()
    {
        if (shadowTransform != null && shadowRenderer != null)
        {
            // Uçarken süzülme etkisini yumuşakça sıfırla
            currentYOffset = Mathf.Lerp(currentYOffset, 0f, Time.deltaTime * 10f);
            
            // Gölgeyi objenin tam altına (orijinal yerine) sabitle ve tam boyutuna getir
            shadowTransform.localPosition = shadowStartPos;
            shadowTransform.localScale = shadowStartScale * maxScale;

            Color c = shadowRenderer.color;
            c.a = maxAlpha; // En koyu halinde kalsın
            shadowRenderer.color = c;
        }
    }
}