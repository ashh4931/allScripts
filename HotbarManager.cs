using UnityEngine;
using System.Collections.Generic;

public class HotbarManager : MonoBehaviour
{

    [Header("Ses Efektleri")]
    public AudioClip notEnoughManaClip;
    [Header("Referanslar")]
    public GameObject player;
    public List<HotbarSlot> slots = new List<HotbarSlot>();
    public Hudtest hud;

    [Header("Başlangıç Yetenekleri")]
    public SkillData assignedSkill;
    public SkillData assignedSkill2;
    public SkillData assignedSkill3;
    public SkillData assignedSkill4;
    public SkillData assignedSkill5;

    [Header("Atama Modu (UI)")]
    public GameObject assignPromptUI; // 🔴 EKSİK OLAN SATIR BURASIYDI
    private SkillData pendingSkill;   // 🔴 EKSİK OLAN İKİNCİ SATIR BURASIYDI

    // Hangi yeteneğin ne zaman tekrar kullanılabileceğini aklında tutan sözlük
    private Dictionary<SkillData, float> skillCooldowns = new Dictionary<SkillData, float>();

    // Oyuncunun can/mana harcamasını yapacak script
    private StatController playerStatsController;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player != null)
        {
            playerStatsController = player.GetComponent<StatController>();
        }

        SetSkillToSlot(0, assignedSkill);
        SetSkillToSlot(1, assignedSkill2);
        SetSkillToSlot(2, assignedSkill3);
        SetSkillToSlot(3, assignedSkill4);
        SetSkillToSlot(4, assignedSkill5);

        // Başlangıçta o yazıyı gizle (eğer atadıysan)
        if (assignPromptUI != null) assignPromptUI.SetActive(false);
    }

    void Update()
    {
        //if (Time.timeScale == 0f) return;
        // --- 1. DURUM: ATAMA MODU (Yetenekleri Slotlara Yerleştirme) ---
        if (pendingSkill != null)
        {
            // Doğrudan Unity'nin KeyCode değerlerini kullanıyoruz
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) FinishAssign(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) FinishAssign(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) FinishAssign(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) FinishAssign(3);
            else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) FinishAssign(4);
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                pendingSkill = null;
                if (assignPromptUI != null) assignPromptUI.SetActive(false);
            }
            return;
        }

        // --- 2. DURUM: NORMAL KULLANIM MODU (Yetenek Fırlatma) ---
        if (InputManager.GetKeyDown("Skill1")) UseSkillInSlot(0);
        if (InputManager.GetKeyDown("Skill2")) UseSkillInSlot(1);
        if (InputManager.GetKeyDown("Skill3")) UseSkillInSlot(2);
        if (InputManager.GetKeyDown("Skill4")) UseSkillInSlot(3);
        if (InputManager.GetKeyDown("Skill5")) UseSkillInSlot(4);
    }

    public void PrepareToAssign(SkillData skillToAssign)
    {
        pendingSkill = skillToAssign;
        // Debug.Log($"{skillToAssign.skillName} seçildi. Hangi slota koymak istersin? (1,2,3,4,5)");

        // Varsa uyarı yazısını ekranda göster
        if (assignPromptUI != null) assignPromptUI.SetActive(true);
    }

    // Seçilen tuşa basıldığında yeteneği o slota yerleştiren fonksiyon
    void FinishAssign(int slotIndex)
    {
        // YENİ: Eğer bu yetenek zaten başka bir slottaysa, eski yerinden sil!
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].assignedSkill == pendingSkill)
            {
                SetSkillToSlot(i, null); // Eski slotu boşalt
            }
        }

        // Yeni slota yerleştir
        SetSkillToSlot(slotIndex, pendingSkill);
        //Debug.Log($"{pendingSkill.skillName}, {slotIndex + 1}. slota başarıyla atandı!");

        // Atama modunu kapat
        pendingSkill = null;
        if (assignPromptUI != null) assignPromptUI.SetActive(false);
    }

    void UseSkillInSlot(int index)
    {
        if (index >= slots.Count || slots[index].assignedSkill == null)
        {
            Debug.Log("Bu slot boş!");
            return;
        }

        SkillData skillToUse = slots[index].assignedSkill;

        // --- 1. COOLDOWN (BEKLEME SÜRESİ) KONTROLÜ ---
        if (skillCooldowns.ContainsKey(skillToUse))
        {
            if (Time.time < skillCooldowns[skillToUse])
            {
                float kalanSure = skillCooldowns[skillToUse] - Time.time;
                //Debug.Log($"{skillToUse.skillName} henüz hazır değil! Kalan: {kalanSure:F1} sn.");
                return; // Kod burada durur, yetenek çalışmaz
            }
        }

       // --- 2. MANA VE STAMINA KONTROLÜ ---
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            if (stats.mana < skillToUse.manaCost)
            {
                Debug.Log($"Yeterli Mana yok! Gereken: {skillToUse.manaCost}");

                // --- YENİ EKLENEN SES KODU ---
                if (notEnoughManaClip != null && AudioManager.instance != null)
                {
                    // Oyuncunun pozisyonunda SFX kanalından sesi çalar
                    AudioManager.instance.PlaySFXAtPosition(notEnoughManaClip, player.transform.position);
                }
                // -----------------------------

                return;
            }
            if (stats.stamina < skillToUse.staminaCost)
            {
                Debug.Log($"Yeterli Stamina yok! Gereken: {skillToUse.staminaCost}");

                // EFEKTİ BURADA TETİKLİYORUZ
                if (hud != null)
                {
                    hud.ShakeStaminaBar();
                }

                return;
            }
        }

        // --- 3. BEDELLERİ ÖDE ---
        // --- 3. BEDELLERİ ÖDE ---
        if (playerStatsController != null)
        {
            playerStatsController.UseMana(skillToUse.manaCost);
            playerStatsController.UseStamina(skillToUse.staminaCost);
        }

        // --- 4. YETENEĞİ ÇALIŞTIR ---
        string keyUsed = "Skill" + (index + 1);
        skillToUse.Cast(player, keyUsed);

        // --- 5. COOLDOWN'I BAŞLAT ---
        skillCooldowns[skillToUse] = Time.time + skillToUse.cooldown;

        // EKSİK KALAN SATIR BURASIYDI: Slotun görselini de başlat!
        slots[index].StartCooldown(skillToUse.cooldown);
    }

    public void SetSkillToSlot(int slotIndex, SkillData newSkill)
    {
        if (slotIndex < slots.Count)
        {
            slots[slotIndex].AssignSkill(newSkill);
        }
    }
}