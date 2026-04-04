using System.Collections;
using UnityEngine;

public class NewGhostNPC : NewBaseEnemyAI
{
    [Header("Sis Parçaları (Sürükle-Bırak)")]
    public Transform[] fogParts; // Hayaletin içindeki sis sprite'ları
    private Vector3[] initialFogPositions;

    private NewGhostEnemyData ghostData;
    private float timeAlive = 0f;
    private bool isAttackingSequence = false;
    private bool hasDamagedThisDash = false;

    // Durum takibi
    private enum GhostState { Chasing, Dashing, Retreating, Waiting }
    private GhostState currentState = GhostState.Chasing;
    // 🔴 YENİ: Hayaletin o an nereye baktığını aklında tutacak
    private float currentFacingDir = 1f;
    protected override void Start()
    {
        base.Start();
        ghostData = baseData as NewGhostEnemyData;

        // Sis parçalarının başlangıç pozisyonlarını hafızaya al (Dalgalanma için)
        if (fogParts != null && fogParts.Length > 0)
        {
            initialFogPositions = new Vector3[fogParts.Length];
            for (int i = 0; i < fogParts.Length; i++)
            {
                initialFogPositions[i] = fogParts[i].localPosition;
            }
        }
    }

    protected override void Update()
    {
        base.Update();

        if (isDead || isFrozen || player == null) return;

        timeAlive += Time.deltaTime;

        AnimateFog();
        HandleRotation(); // 🔴 YENİ EKLENDİ: Eğilme hesaplamalarını her karede yapacak
    }
    // --- EĞİLME (TILT) ANİMASYONU ---
    // --- EĞİLME VE YÖN (TILT & FACING) ANİMASYONU ---
    private void HandleRotation()
    {
        float targetZRotation = 0f;

        if (currentState == GhostState.Chasing)
        {
            // 1. KOVALARKEN: Sürekli oyuncuyu takip et
            Vector2 direction = (player.position - transform.position).normalized;
            currentFacingDir = Mathf.Sign(direction.x);
            targetZRotation = -currentFacingDir * 25f;
        }
        else if (currentState == GhostState.Dashing)
        {
            // 2. ATILIRKEN: Yönünü GÜNCELLEME! Atılmaya başladığı yöne bakmaya devam et.
            // Böylece oyuncunun içinden geçip arkasına düşse bile geriye (oyuncuya) dönüp bakmaz.
            targetZRotation = -currentFacingDir * 37.5f;
        }
        else if (currentState == GhostState.Retreating)
        {
            // 3. KAÇARKEN: Gittiği yöne (Fiziksel hızına) bakarak kaç
            if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            {
                currentFacingDir = Mathf.Sign(rb.linearVelocity.x);
            }
            targetZRotation = -currentFacingDir * 15f;
        }
        else if (currentState == GhostState.Waiting)
        {
            // 4. BEKLERKEN: Etrafta köpekbalığı gibi süzülürken gittiği yöne doğru bak ve hafifçe eğil
            if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            {
                currentFacingDir = Mathf.Sign(rb.linearVelocity.x);
            }
            targetZRotation = -currentFacingDir * 10f;
        }

        // Aniden küt diye dönmek yerine süzülerek (Slerp) yumuşakça eğil
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, targetZRotation), Time.deltaTime * 8f);

        // --- KAFAYI GİTTİĞİ YÖNE ÇEVİRME (fogParts[0]) ---
        if (fogParts != null && fogParts.Length > 0 && fogParts[0] != null)
        {
            Vector3 headScale = fogParts[0].localScale;
            // X ölçeğini her zaman currentFacingDir (+1 veya -1) değerine eşitle
            headScale.x = Mathf.Abs(headScale.x) * currentFacingDir;
            fogParts[0].localScale = headScale;
        }
    }
    protected override void HandleMovement()
    {
        if (isAttackingSequence) return; // Saldırı döngüsündeyse normal hareketi iptal et

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (currentState == GhostState.Chasing)
        {
            // Oyuncuya yaklaş
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * ghostData.moveSpeed;

            // Yeterince yaklaştıysa saldırıya geç
            if (distanceToPlayer <= ghostData.attackRange)
            {
                StartCoroutine(GhostAttackSequence());
            }
        }
        // --- KAFAYI OYUNCUYA ÇEVİRME (fogParts[0]) ---
        if (fogParts != null && fogParts.Length > 0 && fogParts[0] != null)
        {
            // Oyuncu hayaletin neresinde? (Sağındaysa 1, Solundaysa -1)
            float facingSign = Mathf.Sign(player.position.x - transform.position.x);

            // Kafanın mevcut boyutunu al, sadece X eksenini oyuncunun yönüne göre ayarla (+ veya -)
            Vector3 headScale = fogParts[0].localScale;
            headScale.x = Mathf.Abs(headScale.x) * facingSign;
            fogParts[0].localScale = headScale;
        }
    }

    protected override void HandleAttack()
    {
        // Saldırı mantığını yukarıda (HandleMovement içinde mesafeye bakarak) coroutine ile tetikliyoruz.
        // O yüzden bu fonksiyonu boş bırakabiliriz.
    }

    // --- VUR, İÇİNDEN GEÇ, KAÇ VE BEKLE DÖNGÜSÜ ---
    private IEnumerator GhostAttackSequence()
    {
        isAttackingSequence = true;
        hasDamagedThisDash = false;
        rb.linearVelocity = Vector2.zero;

        // 1. HAZIRLIK (Kısa bir duraksama, oyuncuya "Geliyorum!" hissi vermek için)
        yield return new WaitForSeconds(0.3f);

        // 2. ATILMA (Dash) - Oyuncunun içinden geçip arkasına uçma
        currentState = GhostState.Dashing;

        // 🔴 YENİ: ATILMA SESİNİ ÇAL! (Tam fırladığı an)
        if (ghostData.dashSound != null && audioSource != null)
        {
            // Hayalet her atıldığında sesin kalınlığı biraz değişsin ki organik duyulsun
            audioSource.pitch = Random.Range(0.85f, 1.15f);
            audioSource.PlayOneShot(ghostData.dashSound);
        }

        // Oyuncunun o anki konumunu ve yönünü al, oyuncunun biraz daha arkasını hedefle
        Vector3 dashDirection = (player.position - transform.position).normalized;
        Vector3 dashTarget = player.position + (dashDirection * ghostData.dashOvershoot);

        while (Vector3.Distance(transform.position, dashTarget) > 0.5f)
        {
            // Hedefe doğru füze gibi uç
            transform.position = Vector3.MoveTowards(transform.position, dashTarget, ghostData.dashSpeed * Time.deltaTime);

            // Eğer oyuncunun içinden geçiyorsa (ve bu dash'te henüz hasar vermediyse) Hasar Ver!
            if (!hasDamagedThisDash && Vector2.Distance(transform.position, player.position) < 1.5f)
            {
                player.GetComponent<StatController>()?.TakeDamage(ghostData.baseDamage);
                hasDamagedThisDash = true; // Aynı dash içinde 2 kere hasar vermesin
            }

            yield return null;
        }

        // 3. UZAĞA KAÇMA (Retreat)
        currentState = GhostState.Retreating;
        while (Vector2.Distance(transform.position, player.position) < ghostData.retreatDistance)
        {
            // Oyuncunun tersi yönüne doğru kaç
            Vector2 retreatDir = (transform.position - player.position).normalized;
            rb.linearVelocity = retreatDir * ghostData.moveSpeed;
            yield return null;
        }
        rb.linearVelocity = Vector2.zero;

        // 4. BEKLEME (Cooldown)
        currentState = GhostState.Waiting;

        // Öfke kontrolü: 15-20 saniyeyi geçtiyse bekleme süresi kısılır!
        bool isEnraged = timeAlive >= ghostData.enrageTimeThreshold;
        float waitTime = isEnraged ? ghostData.enragedCooldown : ghostData.normalCooldown;

        // Eğer öfkeliyse rengini kırmızımsı falan yapabilirsin (İsteğe bağlı görsel geri bildirim)
        if (isEnraged) AnimateEnrageGlow();

        yield return new WaitForSeconds(waitTime);

        // 5. DÖNGÜYÜ SIFIRLA VE TEKRAR KOVALAMAYA BAŞLA
        currentState = GhostState.Chasing;
        isAttackingSequence = false;
    }

    // --- SİS ANİMASYONU ---
    private void AnimateFog()
    {
        if (fogParts == null || fogParts.Length == 0) return;

        for (int i = 0; i < fogParts.Length; i++)
        {
            if (fogParts[i] != null)
            {
                // Her sis parçasına farklı bir faz (i) vererek birbirinden bağımsız, organik bir şekilde dalgalanmalarını sağlıyoruz
                float xOffset = Mathf.Sin(Time.time * ghostData.fogWobbleSpeed + i) * ghostData.fogWobbleAmount;
                float yOffset = Mathf.Cos(Time.time * ghostData.fogWobbleSpeed + i * 1.5f) * ghostData.fogWobbleAmount;

                fogParts[i].localPosition = initialFogPositions[i] + new Vector3(xOffset, yOffset, 0f);
            }
        }
    }

    private void AnimateEnrageGlow()
    {
        // Hayalet öfkelendiğinde oyuncuyu uyarmak için ufak bir belirti. 
        // İstersen sis parçalarının rengini hafif kırmızıya çalabilirsin.
        foreach (Transform fog in fogParts)
        {
            SpriteRenderer sr = fog.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.Lerp(sr.color, new Color(1f, 0.5f, 0.5f, sr.color.a), Time.deltaTime * 5f);
        }
    }
}