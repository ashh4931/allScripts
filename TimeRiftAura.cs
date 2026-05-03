using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TimeRiftAura : MonoBehaviour
{
    [Header("Yırtık Ayarları")]
    [Range(0.1f, 0.9f)] public float slowPower = 0.7f;
    public float lifetime = 5f;
    [Tooltip("Yavaşlatma başlamadan önce ne kadar beklenecek? (Sesin süresine göre ayarla)")]
    public float activationDelay = 1.0f; // 🔴 YENİ

    [Header("Görsel Ayarlar")]
    public GameObject riftOpenEffect;
    public AudioClip riftOpenSound;

    private List<NewBaseEnemyAI> slowedEnemies = new List<NewBaseEnemyAI>();
    private List<NPCBullet> slowedBullets = new List<NPCBullet>();
    
    private bool isActive = false; // 🔴 Yavaşlatma aktif mi kontrolü

    void Start()
    {
        if (riftOpenEffect != null) Instantiate(riftOpenEffect, transform.position, Quaternion.identity);
        
        // Sesin süresini otomatik alabiliriz veya elinle activationDelay'e yazabilirsin
        if (riftOpenSound != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFXAtPosition(riftOpenSound, transform.position);
        }

        // Aktivasyon sürecini başlat
        StartCoroutine(ActivateAfterDelay());
        
        // Toplam ömür (bekleme süresi + aktif kalma süresi)
        Destroy(gameObject, lifetime + activationDelay);
    }

    private IEnumerator ActivateAfterDelay()
    {
        // 🔴 Ses efekti bitene kadar bekle
        yield return new WaitForSeconds(activationDelay);
        isActive = true;

        // 🔴 AKTİF OLDUĞU AN: Halihazırda çemberin içinde olanları hemen yakala
        Collider2D[] objectsInRift = Physics2D.OverlapCircleAll(transform.position, GetComponent<CircleCollider2D>().radius);
        foreach (var obj in objectsInRift)
        {
            ApplySlowToTarget(obj);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return; // 🔴 Henüz ses bitmediyse yavaşlatma yapma
        ApplySlowToTarget(other);
        Debug.Log(other);
    }

    // Yavaşlatma mantığını merkezi bir fonksiyona aldık
    private void ApplySlowToTarget(Collider2D other)
    {
        NewBaseEnemyAI enemy = other.GetComponent<NewBaseEnemyAI>();
        if (enemy != null && !slowedEnemies.Contains(enemy))
        {
            enemy.ApplyTimeSlow(slowPower);
            slowedEnemies.Add(enemy);
            return;
        }

        NPCBullet bullet = other.GetComponent<NPCBullet>();
        if (bullet != null && !slowedBullets.Contains(bullet))
        {
            bullet.ApplyTimeSlow(slowPower);
            slowedBullets.Add(bullet);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Alandan çıkanları her zaman normale döndür (aktif olsun olmasın)
        NewBaseEnemyAI enemy = other.GetComponent<NewBaseEnemyAI>();
        if (enemy != null && slowedEnemies.Contains(enemy))
        {
            enemy.RemoveTimeSlow();
            slowedEnemies.Remove(enemy);
        }

        NPCBullet bullet = other.GetComponent<NPCBullet>();
        if (bullet != null && slowedBullets.Contains(bullet))
        {
            bullet.RemoveTimeSlow();
            slowedBullets.Remove(bullet);
        }
    }

    void OnDestroy()
    {
        foreach (NewBaseEnemyAI enemy in slowedEnemies)
        {
            if (enemy != null) enemy.RemoveTimeSlow();
        }
        foreach (NPCBullet bullet in slowedBullets)
        {
            if (bullet != null) bullet.RemoveTimeSlow();
        }
    }
}