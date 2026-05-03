using UnityEngine;

public class karakterWeaponController : MonoBehaviour
{
    GameObject targetWeapon;
    GameObject currentWeapon;
    public bool isHoldingMeleeWeapon = false; // Karakterin yakın dövüş silahı tutup tutmadığını takip eder
    public MeleeHandController meleeHandController; // MeleeHandController referansı
                                                    // Bu script karakterin tuttuğu silah ile ilgili işlemleri yapar

    // Update is called once per frame
    void Start()
    {
        meleeHandController = GetComponentInChildren<MeleeHandController>();
    }

    void Update()
    {
        trypickup();
    }
void dropCurrentWeapon()
    {
        if (currentWeapon != null)
        {
            weapon weaponScript = currentWeapon.GetComponent<weapon>();
            
            if (weaponScript != null)
            {
                weaponScript.isEquipped = false;
                weaponScript.SetEquipped(false);
            }

            // 1. Ebeveyn bağını kopar
            currentWeapon.transform.SetParent(null);

            // 2. Rotasyonu sıfırla (Dünya koordinatlarına göre 0 yapar)
            currentWeapon.transform.rotation = Quaternion.identity;

            // Eğer 2D bir oyunda silahın z ekseninde dönmesini istemiyorsan:
            // currentWeapon.transform.eulerAngles = Vector3.zero;

            Debug.Log("weaponContoller: Silah bırakıldı ve rotasyon sıfırlandı");
            
            currentWeapon = null;
            GetComponentInChildren<HandController>().ClearWeapon();
            isHoldingMeleeWeapon = false;
            
            GetComponentInChildren<HandController>().dünyayaÇağır();
            meleeHandController.uzayaGönder();
        }
    }

    void pickup()
    {
        weapon weaponScript = targetWeapon.GetComponent<weapon>();
        weaponScript.isEquipped = true;
        weaponScript.SetEquipped(true);
        // 1. Silah tipine göre hedef eli belirle
        Transform hand;
        if (weaponScript.weaponData.weaponType == WeaponType.Melee)
        {
            GetComponentInChildren<HandController>().uzayaGönder(); // Melee silahı uzaya gönder
            hand = FindDeepChild(transform, "meleeHand");
            isHoldingMeleeWeapon = true;
            GetComponentInChildren<MeleeHandController>().dünyayaÇağır();
        }
        else
        {

            hand = FindDeepChild(transform, "rightHand");
            meleeHandController.uzayaGönder(); // Melee silahı dünyaya çağır
            isHoldingMeleeWeapon = false;

        }

        if (hand != null && weaponScript.gripPoint != null)
        {
            // 2. Ebeveyn yap
            targetWeapon.transform.SetParent(hand);

            // 3. Ölçek (Scale) düzeltmesi
            targetWeapon.transform.localScale = new Vector3(
                1f / hand.lossyScale.x,
                1f / hand.lossyScale.y,
                1f / hand.lossyScale.z
            );

            // 4. Rotasyonu sıfırla
            targetWeapon.transform.localRotation = Quaternion.identity;

            // 5. Grip Point (Tutma Noktası) Hizalaması
            targetWeapon.transform.position = hand.position;
            Vector3 offset = targetWeapon.transform.position - weaponScript.gripPoint.position;
            targetWeapon.transform.position += offset;

            // 6. HandController'a bilgi gönder
            GetComponentInChildren<HandController>().SetWeapon(targetWeapon, weaponScript.weaponData);
        }

        currentWeapon = targetWeapon;
        if (weaponScript.weaponData.weaponType == WeaponType.Melee)
        {
            if (meleeHandController != null)
            {
                meleeHandController.currentWeapon = currentWeapon;
                Debug.Log("KWC: Melee silah MeleeHandController'a atandı: " + currentWeapon.name);
            }
        }
    }
    void trypickup()
    {
        if (InputManager.GetKeyDown("PickUpWeapon") && targetWeapon != null && currentWeapon == null)
        {
            pickup();
        }
        else if (InputManager.GetKeyDown("PickUpWeapon") && currentWeapon != null)
        {
            dropCurrentWeapon();

        }
        else if (InputManager.GetKeyDown("PickUpWeapon") && targetWeapon == null)
        {
            //Debug.Log("weaponContoller: E tuşuna basıldı ama silah menzilde değil");
        }
        else if (InputManager.GetKeyDown("PickUpWeapon") && currentWeapon == null)
        {
           // Debug.Log("weaponContoller: E tuşuna basıldı ama silah tutulmuyor");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("weapon"))
        {
            targetWeapon = collision.gameObject;

            // Debug.Log("weaponContoller: Silah menzile girdi");
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {



        targetWeapon = null;
        //Debug.Log("weaponContoller: Silah menzilden çıktı");
    }
    Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;
            Transform result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

}
