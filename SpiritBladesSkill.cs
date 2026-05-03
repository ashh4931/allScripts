using UnityEngine;
using System.Collections.Generic;

public class SpiritBladesSkill : MonoBehaviour
{
    [Header("Referanslar")]
    public GameObject bladePrefab;
    
    [Header("Yelpaze (Takip) Ayarları")]
    public float trailingDistance = 1.2f;
    public float fanSpreadAngle = 40f; 

    [Header("Saldırı Ayarları")]
    public float detectionRadius = 10f;
    public float attackCooldown = 1.5f;
    public LayerMask enemyLayer;

    // Yetenek Verileri (Data'dan gelecek)
    private int bladeCount;
    private float skillTimer;
    private bool isActive = false;

    private SpiritBlade[] blades;
    private Vector3 lastPos;
    private Vector3 backwardDir = Vector3.down;
    private float nextAttackTime;
    
    private float targetTiltAngle = 0f;
    private float currentTiltAngle = 0f;

    // 🔴 YENİ: Yetenek Data'sından çağrılan Use fonksiyonu
    public void Use(int count, float duration)
    {
        // Eğer zaten aktif kılıçlar varsa, eskileri silip yenilerini çağır (Refresh)
        ClearBlades();

        bladeCount = count;
        skillTimer = duration;
        isActive = true;

        blades = new SpiritBlade[bladeCount];
        for (int i = 0; i < bladeCount; i++)
        {
            GameObject newBlade = Instantiate(bladePrefab, transform.position, Quaternion.identity);
            blades[i] = newBlade.GetComponent<SpiritBlade>();
            
            blades[i].enemyLayer = this.enemyLayer; 
            blades[i].Setup(this.transform); 
        }
        lastPos = transform.position;
    }

    void Update()
    {
        if (!isActive) return;

        // 🔴 YENİ: Süre Kontrolü
        skillTimer -= Time.deltaTime;
        if (skillTimer <= 0)
        {
            ClearBlades();
            isActive = false;
            return;
        }

        Vector3 currentPos = transform.position;
        Vector3 moveDelta = currentPos - lastPos;
        
        if (moveDelta.sqrMagnitude > 0.0001f)
        {
            backwardDir = -moveDelta.normalized;
            targetTiltAngle = -moveDelta.normalized.x * 25f; 
        }
        else
        {
            targetTiltAngle = 0f; 
        }
        lastPos = currentPos;

        currentTiltAngle = Mathf.Lerp(currentTiltAngle, targetTiltAngle, Time.deltaTime * 6f);

        float startAngle = -fanSpreadAngle / 2f;
        float angleStep = (bladeCount > 1) ? (fanSpreadAngle / (bladeCount - 1)) : 0f;

        for (int i = 0; i < bladeCount; i++)
        {
            if (blades[i] == null) continue;

            float currentAngleOffset = startAngle + (i * angleStep);
            Vector3 bladeBackwardDir = Quaternion.Euler(0, 0, currentAngleOffset) * backwardDir;
            
            blades[i].targetRestPos = currentPos + bladeBackwardDir * trailingDistance;
            blades[i].targetRestAngle = currentTiltAngle; 
        }

        if (Time.time >= nextAttackTime)
        {
            SearchAndAttackAll();
        }
    }

    void SearchAndAttackAll()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
        if (hitEnemies.Length == 0) return;

        List<Transform> availableTargets = new List<Transform>();
        foreach (var hit in hitEnemies)
        {
            // Canı olan düşmanları listeye ekle
            if (hit.GetComponent<IDamageable>() != null)
            {
                availableTargets.Add(hit.transform);
            }
        }

        bool attacked = false;

        foreach (var blade in blades)
        {
            if (blade != null && blade.currentState == SpiritBlade.BladeState.Trailing && availableTargets.Count > 0)
            {
                int randIndex = Random.Range(0, availableTargets.Count);
                Transform chosenTarget = availableTargets[randIndex];

                blade.AttackTarget(chosenTarget);
                attacked = true;

                availableTargets.RemoveAt(randIndex); 
            }
        }

        if (attacked)
        {
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void ClearBlades()
    {
        if (blades == null) return;
        foreach (SpiritBlade blade in blades)
        {
            if (blade != null) Destroy(blade.gameObject);
        }
    }

    void OnDestroy()
    {
        ClearBlades();
    }
}