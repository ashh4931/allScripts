using UnityEngine;

// Kalkanlar da hasar alabileceği için IDamageable arayüzünü (interface) kullanıyoruz
public class EnemyShield : MonoBehaviour, IDamageable
{
    [Header("Kalkan Ayarları")]
    public float maxHealth = 300f; // Kalkanın kendi canı (Çekirdekten çok daha yüksek)
    private float currentHealth;

    public SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        // Eğer Inspector'dan sürüklemeyi unutursan diye kendi üstündeki SpriteRenderer'ı otomatik bulur
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(float damage, float minDamage = 0f, float maxDamage = 0f)
    {
        currentHealth -= damage;
        
        // Kalkan kırılırsa objeyi kapat (Böylece çekirdeğe giden yol açılır)
        if (currentHealth <= 0)
        {
            gameObject.SetActive(false); 
        }
    }

    // 🔴 İŞTE EKSİK OLAN VE HATAYA SEBEP OLAN FONKSİYON BURASI!
    // Boss'un beyni kalkanların rengini değiştirmek istediğinde bu fonksiyonu çağıracak
    public void SetColor(Color newColor)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = newColor;
        }
    }
}