using System.Collections; // 🔴 Coroutine (Zamanlayıcı) kullanmak için bu şart!
using UnityEngine;

public class EarthQuake : MonoBehaviour
{
    [Header("Savaş Ayarları (Alan Hasarı)")]
    public float damageAmount = 50f;   
    public float attackRadius = 5f;    
    public LayerMask enemyLayer;       

    // 🔴 YENİ: İtme Ayarları
    [Header("Geri İtme (Knockback) Ayarları")]
    public float knockbackForce = 15f; // Düşmanların ne kadar uzağa uçacağı
    public float stunDuration = 0.3f;  // Havada uçarken/sersemlerken geçecek süre

    [Header("Görsel ve Ses Ayarları")]
    public float cameraShakeIntensity = 0.4f;
    public float cameraShakeDuration = 2f;
    public AudioClip stoneBodySound;
    public AudioClip earthQuakeSound;
    public GameObject earthQuakeVFX;

    public void use()
    {
        playSoundEffect();
        Debug.Log("EarthQuake skill used.");
        
        // Yere vurma animasyonuna/efektine denk gelmesi için ufak bir gecikme
        Invoke("TriggerEarthquakeEffect", 0.2f); 
    }

    void playSoundEffect()
    {
        if (stoneBodySound != null) AudioSource.PlayClipAtPoint(stoneBodySound, Camera.main.transform.position);
        if (earthQuakeSound != null) AudioSource.PlayClipAtPoint(earthQuakeSound, Camera.main.transform.position);
    }

    void TriggerEarthquakeEffect()
    {
        Vector2 centerPosition = transform.position;
        if (GetComponentInParent<Rigidbody2D>() != null)
        {
            centerPosition = GetComponentInParent<Rigidbody2D>().position;
        }

        if (Camera.main.GetComponent<CameraController>() != null)
        {
            Camera.main.GetComponent<CameraController>().Shake(cameraShakeDuration, cameraShakeIntensity);
        }

        if (earthQuakeVFX != null)
        {
            Instantiate(earthQuakeVFX, centerPosition, Quaternion.identity);
        }

        DealAreaDamage(centerPosition);
    }

    void DealAreaDamage(Vector2 center)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(center, attackRadius, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            
            if (damageable != null)
            {
                // 1. Hasarı Vur
                damageable.TakeDamage(damageAmount);
                
                // 2. 🔴 YENİ: Düşmanı geriye doğru fırlat!
                StartCoroutine(ApplyKnockback(enemy, center));
            }
        }
    }

    // 🔴 YENİ: Düşmanı savurup kısa süreliğine sersemleten sihirli zamanlayıcı
    private IEnumerator ApplyKnockback(Collider2D enemy, Vector2 center)
    {
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        
        // Düşmanın yapay zeka kodunu buluyoruz (Bütün NPC'lerin atası olduğu için NewBaseEnemyAI arıyoruz)
        NewBaseEnemyAI enemyAI = enemy.GetComponent<NewBaseEnemyAI>(); 

        if (enemyRb != null && enemyAI != null)
        {
            // 1. Düşmanın yapay zekasını (yürümesini) kısa süreliğine KAPAT.
            // (Kapatmazsak bizim fırlatma hızımızı anında ezip sana doğru yürümeye devam eder)
            enemyAI.enabled = false;

            // 2. İtme yönünü hesapla: (Düşmanın Konumu) EKSİ (Depremin Merkezi)
            Vector2 pushDirection = (enemy.transform.position - (Vector3)center).normalized;

            // 3. Gücü uygula ve fırlat!
            enemyRb.linearVelocity = pushDirection * knockbackForce;

            // 4. Havada uçması ve sersemlemesi için belirlediğimiz süre kadar bekle
            yield return new WaitForSeconds(stunDuration);

            // 5. Süre bitince, düşman hala hayattaysa yapay zekasını geri aç
            if (enemy != null && enemyAI != null)
            {
                enemyRb.linearVelocity = Vector2.zero; // Kaymayı durdur
                enemyAI.enabled = true; // Tekrar saldırmaya/yürümeye başlasın
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}