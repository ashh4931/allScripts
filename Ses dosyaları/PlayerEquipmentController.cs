using UnityEngine;
using UnityEngine.XR;

public class PlayerEquipmentController : MonoBehaviour
{
    private float timer = 0f;
    private bool hasWeapon = false;
    GameObject currentWeapon;
    GameObject closestWeapon;
    public GameObject hand;

    void Start()
    {

    }
    void Update()
    {

        //buraya her 5 saniyede bir çalışacak ve en yakın silahın ismini yazdıracak bir fonksiyon ekle
       // InvokeRepeating("PrintWeaponName", 0f, 1f);

        
        if (Input.GetKeyDown(KeyCode.Z) && closestWeapon != null && !hasWeapon)
        {
            currentWeapon = closestWeapon; // Önce referansı ata
            tryPickupWeapon(currentWeapon); // Fonksiyonu çağır
            hasWeapon = true;
        }
        else if (Input.GetKeyDown(KeyCode.Z) && hasWeapon)
        {
            // Silahı bırak
            currentWeapon.transform.SetParent(null);
            if (currentWeapon.GetComponent<Collider2D>() != null)
            {
                currentWeapon.GetComponent<Collider2D>().enabled = true;
            }
            hasWeapon = false;
        }
    }


    void tryPickupWeapon(GameObject weapon)
    {
       if(weapon == null)
        {
            Debug.Log("kılıç yok");
            return;
        }
       
        // Silahı elin (hand) içine yerleştir
        weapon.transform.SetParent(hand.transform);

        // Pozisyonu ve açısını sıfırla (Elin tam üstüne otursun)
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;

        // FİZİK DÜZELTMELERİ:
        // Silahın yerdeki fizik etkileşimlerini kapat ki karakteri itmesin
         
        if (weapon.GetComponent<Collider2D>() != null)
        {
            weapon.GetComponent<Collider2D>().enabled = false;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        closestWeapon = collision.gameObject;

    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == closestWeapon)
        {
            closestWeapon = null;
        }
    }



    /////////////////////////////////////////////////
    void PrintWeaponName()
    {
        if (closestWeapon != null)
        {
            Debug.Log("En yakın silah: " + closestWeapon.name);
        }
    }
}
