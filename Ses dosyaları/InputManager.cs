using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    // Singleton yapısı: Her yerden kolayca erişmek için
    public static InputManager instance;

    public Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>();

    void Awake()
    {
        // Singleton güvenliği: Eğer sahnede birden fazla InputManager varsa, yenisini sil
        if (instance == null) instance = this;
        else Destroy(gameObject);

        // --- VARSAYILAN TUŞ ATAMALARI ---
        // Menüler ve Etkileşim
        keys.Add("SkillTree", KeyCode.K);
        keys.Add("Inventory", KeyCode.I);
        keys.Add("Interact", KeyCode.E); // Yerdeki silahı alma, kapı açma vs.
        keys.Add("OpenStatMenu", KeyCode.C);
        keys.Add("TurretMenu", KeyCode.T);
        // Hotbar Yetenekleri (Şu an 1, 2, 3, 4, 5 ama oyuncu ileride Q, R, F yapabilir)
        keys.Add("Skill1", KeyCode.Alpha1);
        keys.Add("Skill2", KeyCode.Alpha2);
        keys.Add("Skill3", KeyCode.Alpha3);
        keys.Add("Skill4", KeyCode.Alpha4);
        keys.Add("Skill5", KeyCode.Alpha5);
        keys.Add("EscapeMenu",KeyCode.Escape); 
        keys.Add("SpawnNPC", KeyCode.Z);
        keys.Add("ChangeLanguage", KeyCode.X);

    }

    // Tuş basıldı mı kontrolü
    public static bool GetKeyDown(string keyName)
    {

        if (instance != null && instance.keys.ContainsKey(keyName))
        {
            return Input.GetKeyDown(instance.keys[keyName]);
        }
        return false;
    }
}