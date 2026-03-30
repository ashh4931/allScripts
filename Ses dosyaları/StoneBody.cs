using UnityEngine;

public class StoneBody : MonoBehaviour
{
    [Header("Ses Ayarları")]
    public AudioClip stoneBodySound;
    public AudioClip stopStoneBodySound;
    
    [Header("Ayarlar")]
    public float stoneBodyDuration = 10f;
    public float defenseBoostAmount = 10f; 

    [Header("Görsel Ayarlar")]
    public SpriteRenderer mainRenderer;
    // FF7575 renginin Unity karşılığı. İstersen Inspector'dan bu rengi sonradan değiştirebilirsin!
    public Color activeColor = new Color(1f, 0.46f, 0.46f, 1f); 

    private bool isActive;
    private Color originalColor = Color.white;
    private StatController statController;
    private PlayerStats stats;

   

   public void use()
    {
        if (isActive) return; 

        // 🔴 HAYAT KURTARAN DÜZELTME: Referansları Start'ta değil, tam kullanacağımız anda buluyoruz!
        // Böylece script kapalı bile olsa hata vermez.
        StatController statController = GetComponentInParent<StatController>();
        PlayerStats stats = GetComponentInParent<PlayerStats>();

        // Güvenlik Duvarı: Eğer hala bulamazsa (script yanlış objeye atılmışsa) oyunu çökertme, konsola hata yazdır.
        if (statController == null || stats == null)
        {
            Debug.LogError("StoneBody: StatController veya PlayerStats bulunamadı! Bu script Player'ın altındaki bir objede olmalı.");
            return; 
        }

        isActive = true;

        // 1. SES ÇAL
        if (stoneBodySound != null) AudioSource.PlayClipAtPoint(stoneBodySound, Camera.main.transform.position);

        // 2. RENGİ DEĞİŞTİR
        if (mainRenderer != null) 
        {
            mainRenderer.color = activeColor;
        }

        // 3. İSTATİSTİKLERİ DEĞİŞTİR (Artık statController kesinlikle dolu!)
        statController.BoostDefense(defenseBoostAmount, stoneBodyDuration); 
        float speedDecrease = -(stats.movSpeed / 2f);
        statController.BoostMovementSpeed(speedDecrease, stoneBodyDuration); 
statController.isStoneActive = true;
        // 4. SÜRE BİTİNCE KAPAT
        Invoke("RemoveStoneEffect", stoneBodyDuration);

    }
    void RemoveStoneEffect()
    {
         StatController statController = GetComponentInParent<StatController>();
        PlayerStats stats = GetComponentInParent<PlayerStats>();
        isActive = false;

        // 1. KAPANIŞ SESİNİ ÇAL
    // 1. KAPANIŞ SESİNİ ÇAL
        if (stopStoneBodySound != null) AudioSource.PlayClipAtPoint(stopStoneBodySound, Camera.main.transform.position);

        // 2. RENGİ ESKİ HALİNE DÖNDÜR
        if (mainRenderer != null) 
        {
            mainRenderer.color = originalColor;
        }
        statController.isStoneActive = false;
    }
}