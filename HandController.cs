using System.Collections;
using UnityEngine;

public class HandController : MonoBehaviour
{
    public bool isSpaceTraveling = false;
    [Header("Silah Verisi")]
    public New_WeaponData currentWeaponData;
    public GameObject currentWeapon;

    [Header("Bağlantılar")]
    public Transform bodyCenter;
    public Animator characterAnimator;
    // --- YENİ EKLENEN: Karakterin ana Rigidbody2D bileşeni ---
    [Tooltip("Karakterin ana objesindeki Rigidbody2D bileşeni")]
    public Rigidbody2D playerRigidbody;

    [Header("Hareket Ayarları")]
    public float maxDistance = 2.0f;
    public float constMaxDistance; // Normal şartlarda elin takip edeceği maksimum mesafe
    public float followSpeed = 10f;

    [Header("Açı Kısıtlama")]
    [Range(0, 180)]
    public float clampAngle = 45f;

    [Header("Geri Tepme (Recoil) Ayarları")]
    public float handRecoilReturnSpeed = 10f; // Elin eski yerine dönme hızı (Ne kadar yüksek, o kadar yay gibi)
    private Vector3 recoilOffset; // Elin görsel olarak kayacağı mesafe
    public float bodyPushForce = 10f; // Vücuda uygulanacak itme kuvveti


    void Start()
    {
        constMaxDistance = maxDistance; // Başlangıçta normal mesafeyi kaydet

        // --- YENİ EKLENEN: Eğer atanmadıysa Rigidbody'yi bulmaya çalış ---
        if (playerRigidbody == null && bodyCenter != null)
        {
            playerRigidbody = bodyCenter.GetComponentInParent<Rigidbody2D>();
            if (playerRigidbody == null)
            {
                Debug.LogWarning("HandController: Karakterin Rigidbody2D bileşeni bulunamadı! Vücut itme özelliği çalışmayacak.");
            }
        }
    }
    public void uzayaGönder()
    {
        // ... (Uzaya gönderme kodları aynı) ...
        isSpaceTraveling = true;
        maxDistance = 5000f;
        transform.position += transform.right * 500f;
        Debug.Log("El uzaya gönderildi!");
    }

    public void dünyayaÇağır()
    {
        // ... (Dünyaya çağırma kodları aynı) ...
        isSpaceTraveling = false;
        maxDistance = constMaxDistance;
        Debug.Log("El dünyaya çağrıldı!");
    }
    void Update()
    {
        if (!GetComponentInParent<PlayerController>().IntroFinished)
        {
            return;
        }
        // 1. Durum Kontrolü: Elinde kılıç var mı?
        // (Parent scriptindeki bool değerine bakıyoruz)
        bool isMelee = false; // Varsayılan olarak false, güvenli kod
        var weaponController = GetComponentInParent<karakterWeaponController>();
        if (weaponController != null)
        {
            isMelee = weaponController.isHoldingMeleeWeapon;
        }

        // 2. Girdi Kontrolü (Ateş etme veya Kılıç sallama)
        HandleInput(isMelee);

        // 3. KRİTİK NOKTA:
        // Eğer elinde kılıç (melee) varsa, aşağıdaki hareket kodlarını ÇALIŞTIRMA.
        // Böylece script eli mouse'a döndürmeye çalışmaz ve Animator kontrolü ele alır.
        if (isMelee || isSpaceTraveling)
        {
            return;
        }

        // 4. Menzilli Silah veya Boş El Varsa Fareyi Takip Et
        // --- GÜNCELLENEN: Değişken adı daha net ---
        recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, Time.deltaTime * handRecoilReturnSpeed);
        HandleMovement();
    }

    void HandleInput(bool isMelee)
    {
        if (InputManager.GetKeyDown("Attack"))
        {
            if (isMelee)
            {
                // Animator'daki "Attack" tetikleyicisini çalıştır
                if (characterAnimator != null)
                {
                    characterAnimator.SetTrigger("Attack");
                }
            }
            else if (currentWeapon != null && currentWeaponData.weaponType == WeaponType.Gun)
            {
                Shoot();
            }
        }
    }

    void Shoot()
    {
       // Debug.Log(currentWeaponData.weaponName + " ateşlendi!");

        // --- GÜNCELLENEN: Hem eli görsel olarak salla, hem vücudu it ---
        // 1. Eli görsel olarak salla (mevcut kod, biraz daha şiddetli yapabilirsin)
        ApplyHandRecoil(-transform.right, 0.5f);

        // 2. Vücudu fiziksel olarak it
        ApplyBodyPush(-transform.right, bodyPushForce);
    }

    void HandleMovement()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        Vector2 directionToMouse = (mousePos - bodyCenter.position).normalized;
        float distToMouse = Vector3.Distance(bodyCenter.position, mousePos);

        float faceX = 0, faceY = 0;
        if (characterAnimator != null)
        {
            faceX = characterAnimator.GetFloat("moveX");
            faceY = characterAnimator.GetFloat("moveY");
        }

        Vector2 facingDir = new Vector2(faceX, faceY).normalized;
        if (facingDir == Vector2.zero) facingDir = directionToMouse;

        float angleDifference = Vector2.SignedAngle(facingDir, directionToMouse);
        float clampedAngle = Mathf.Clamp(angleDifference, -clampAngle, clampAngle);
        Vector2 finalDirection = RotateVector(facingDir, clampedAngle);

        float currentTargetDist = Mathf.Clamp(distToMouse, 0, maxDistance);

        // KRİTİK NOKTA: recoilOffset'i hedefe ekliyoruz.
        Vector3 targetPosition = bodyCenter.position + (Vector3)finalDirection * currentTargetDist + recoilOffset;

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

        float rotAngle = Mathf.Atan2(finalDirection.y, finalDirection.x) * Mathf.Rad2Deg;

        if (finalDirection.x < 0) transform.rotation = Quaternion.Euler(0, 180, -rotAngle + 180);
        else transform.rotation = Quaternion.Euler(0, 0, rotAngle);
    }

    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
    }

    // --- GÜNCELLENEN VE YENİ FONKSİYONLAR ---

    // Sadece eli görsel olarak sallayan fonksiyon (eski ApplyRecoil)
    public void ApplyHandRecoil(Vector2 direction, float intensity)
    {
        recoilOffset += (Vector3)direction * intensity;
    }

    // Karakterin vücudunu fiziksel olarak iten YENİ fonksiyon
    // HandController.cs içindeki fonksiyonu bu şekilde güncelle:
    public void ApplyBodyPush(Vector2 direction, float force)
    {
        /* if (playerRigidbody == null) return;

         PlayerController pc = playerRigidbody.GetComponent<PlayerController>();
         if (pc != null)
         {
             // Daha önce konuştuğumuz gibi yönü ters çeviriyoruz (-)
             pc.ApplyKnockback(-direction * force);
         }*/
    }

    public void SetWeapon(GameObject weaponObj, New_WeaponData newData)
    {
        // ... (Aynı fonksiyon) ...
        currentWeapon = weaponObj;
        currentWeaponData = newData;
      //  Debug.Log("Yeni menzilli silah kuşanıldı: " + newData.weaponName);
    }
    // Diğer scriptlerdeki 'ApplyRecoil' çağrılarını bozmamak için bu ismi koruyoruz
    // HandController.cs içindeki fonksiyonu bu şekilde sadeleştirin:
    public void ApplyRecoil(Vector2 fireDirection, float intensity)
    {
        // 1. Eli görsel olarak sars (Mermi yönünde sarsılabilir, sorun değil)
        recoilOffset += (Vector3)fireDirection * intensity;

        // 2. VÜCUT İTME MATEMATİĞİ
        /* PlayerController pc = playerRigidbody.GetComponent<PlayerController>();
         if (pc != null)
         {
             // BAŞINA EKSİ KOYDUK: 
             // fireDirection (Mermi yönü) sağ ise, -fireDirection (İtme yönü) sol olur.
             Vector2 pushDirection = -fireDirection.normalized;

             pc.ApplyKnockback(pushDirection * intensity * bodyPushForce);
         }*/
    }
    public void ClearWeapon()
    {
        // ... (Aynı fonksiyon) ...
        currentWeapon = null;
        Debug.Log("Silah bilgisi temizlendi.");
    }



    public void TriggerRecoil(Vector2 aimDirection, float visualKickAmount, float physicalPushForce)
    {
        // 1. Geri tepme yönünü bul (Ateş edilen yönün tam tersi)
        Vector2 recoilDirection = aimDirection.normalized;

        // 2. GÖRSEL TEPKİ: Eli geri tepme yönünde kaydır
        recoilOffset += (Vector3)recoilDirection * visualKickAmount;

        // 3. FİZİKSEL TEPKİ: Karakteri geri it
        /*  if (playerRigidbody != null)
          {
              PlayerController pc = playerRigidbody.GetComponent<PlayerController>();
              if (pc != null)
              {
                  // PushDirection zaten ters yön olduğu için direkt veriyoruz, ekstra eksi (-) koymuyoruz!
                  pc.ApplyKnockback(recoilDirection * physicalPushForce);
              }
          }*/
    }
}