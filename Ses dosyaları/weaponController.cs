using UnityEngine;

public class alpersWeaponController : MonoBehaviour
{
    GameObject tragetWeapon;
    GameObject currentWeapon;


    // Bu script karakterin tuttuğu silah ile ilgili işlemleri yapar
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        trypickup(); 
    }

    void trypickup()
    {
     if (InputManager.GetKeyDown("Interact") && tragetWeapon != null && currentWeapon == null)
        {
            tragetWeapon.GetComponent<weapon>().isEquipped = true;
            tragetWeapon.transform.SetParent(this.transform.Find("hand"));
            currentWeapon = tragetWeapon;
            Debug.Log("weaponContoller: Silah alındı");
        }
        else if (InputManager.GetKeyDown("Interact") && currentWeapon != null)
        {
            tragetWeapon.GetComponent<weapon>().isEquipped = false;
            tragetWeapon.transform.SetParent(null);
            Debug.Log("weaponContoller: Silah bırakıldı");
            currentWeapon = null;
        }
        else if (Input.GetKeyDown(KeyCode.E) && tragetWeapon == null)
        {
            Debug.Log("weaponContoller: E tuşuna basıldı ama silah menzilde değil");
        }
        else if (Input.GetKeyDown(KeyCode.E) && currentWeapon == null)
        {
            Debug.Log("weaponContoller: E tuşuna basıldı ama silah tutulmuyor");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("weapon"))
        {
            tragetWeapon = collision.gameObject;
            tragetWeapon.GetComponent<weapon>().isEquipped = true;
         Debug.Log("weaponContoller: Silah menzile girdi" );
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        tragetWeapon = null;
        Debug.Log("weaponContoller: Silah menzilden çıktı");
    }
}
