using UnityEngine;
using System.Collections;

public class NPCStatusManager : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private NewBaseEnemyAI enemyAI;
    private Color normalColor;

    private bool isEffectActive = false;

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        enemyAI = GetComponent<NewBaseEnemyAI>();
        
        if (spriteRenderer != null) 
            normalColor = spriteRenderer.color;
    }

    // --- 1. BUZ ETKİSİ (Yavaşlatma) ---
    public void ApplyIceEffect(float duration, float slowMultiplier)
    {
        StopAllCoroutines();
        StartCoroutine(IceRoutine(duration, slowMultiplier));
    }

    IEnumerator IceRoutine(float duration, float slowMultiplier)
    {
        if (enemyAI != null) 
            enemyAI.CurrentMoveSpeed = enemyAI.BaseMoveSpeed * (1f - slowMultiplier);
        
        if (spriteRenderer != null) 
            spriteRenderer.color = new Color(0.5f, 0.8f, 1f); // Buz mavisi

        yield return new WaitForSeconds(duration);
        ResetEffects();
    }

    // --- 2. ATEŞ ETKİSİ (Zamanla Hasar) ---
    public void ApplyBurnEffect(float duration, float damagePerSec)
    {
        StopAllCoroutines();
        StartCoroutine(BurnRoutine(duration, damagePerSec));
    }

    IEnumerator BurnRoutine(float duration, float damagePerSec)
    {
        if (spriteRenderer != null) 
            spriteRenderer.color = new Color(1f, 0.4f, 0f); // Turuncu/Ateş rengi

        float timer = 0;
        while (timer < duration)
        {
            if (enemyAI != null) 
                enemyAI.TakeDamage(damagePerSec);
            
            timer += 1f;
            yield return new WaitForSeconds(1f);
        }
        ResetEffects();
    }

    // --- 3. ZEHR ETKİSİ (Zamanla Hasar + Yeşil Renk) ---
    public void ApplyPoisonEffect(float duration, float damagePerSec)
    {
        StopAllCoroutines();
        StartCoroutine(PoisonRoutine(duration, damagePerSec));
    }

    IEnumerator PoisonRoutine(float duration, float damagePerSec)
    {
        if (spriteRenderer != null) 
            spriteRenderer.color = new Color(0f, 0.8f, 0.2f); // Zehir yeşili

        float timer = 0;
        while (timer < duration)
        {
            if (enemyAI != null) 
                enemyAI.TakeDamage(damagePerSec);
            
            timer += 1f;
            yield return new WaitForSeconds(1f);
        }
        ResetEffects();
    }

    // --- 4. ELEKTRİK ETKİSİ (Sersemletme/Stun) ---
    public void ApplyStunEffect(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(StunRoutine(duration));
    }

    IEnumerator StunRoutine(float duration)
    {
        if (enemyAI != null) 
            enemyAI.CurrentMoveSpeed = 0; // Hareketi durdur
        
        if (spriteRenderer != null) 
            spriteRenderer.color = Color.yellow;

        yield return new WaitForSeconds(duration);
        ResetEffects();
    }

    private void ResetEffects()
    {
        if (enemyAI != null) 
            enemyAI.CurrentMoveSpeed = enemyAI.BaseMoveSpeed;
        
        if (spriteRenderer != null) 
            spriteRenderer.color = normalColor;
    }
}