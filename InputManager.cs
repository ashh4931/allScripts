using UnityEngine;
using System.Collections.Generic;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    public Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>();

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        // --- TUŞLARI YÜKLE VEYA VARSAYILANLARI ATA ---
        AddKey("SkillTree", KeyCode.K);
        AddKey("Inventory", KeyCode.I);
        AddKey("Interact", KeyCode.E);
        AddKey("OpenStatMenu", KeyCode.C);
        AddKey("TurretMenu", KeyCode.T);
        
        AddKey("Skill1", KeyCode.Alpha1);
        AddKey("Skill2", KeyCode.Alpha2);
        AddKey("Skill3", KeyCode.Alpha3);
        AddKey("Skill4", KeyCode.Alpha4);
        AddKey("Skill5", KeyCode.Alpha5);
        
        AddKey("EscapeMenu", KeyCode.Escape);
        AddKey("SpawnNPC", KeyCode.Z);
        AddKey("ChangeLanguage", KeyCode.X);
        
        AddKey("PickUpWeapon", KeyCode.E);
        AddKey("Attack", KeyCode.Mouse0);
        AddKey("WeaponSpecial", KeyCode.Mouse1);
        AddKey("Reload", KeyCode.R);
        AddKey("OpenShield", KeyCode.Mouse1); 
        AddKey("FastDestroy", KeyCode.F);
        //AddKey("Shift",KeyCode.Shift); //KOŞMA EKLENİCEK
    }

        private void AddKey(string keyName, KeyCode defaultKey)
    {
        string savedKey = PlayerPrefs.GetString("Key_" + keyName, defaultKey.ToString());
        KeyCode parsedKey = (KeyCode)Enum.Parse(typeof(KeyCode), savedKey);
        keys.Add(keyName, parsedKey);
    }

    // Ayarlar menüsünden tuşu değiştirmek ve kaydetmek için kullanılacak
    public void ChangeKey(string keyName, KeyCode newKey)
    {
        if (keys.ContainsKey(keyName))
        {
            keys[keyName] = newKey;
            PlayerPrefs.SetString("Key_" + keyName, newKey.ToString());
            Debug.Log(keyName + " tuşu " + newKey.ToString() + " olarak değiştirildi.");
        }
    }

    public static bool GetKeyDown(string keyName)
    {
        if (instance != null && instance.keys.ContainsKey(keyName))
            return Input.GetKeyDown(instance.keys[keyName]);
        return false;
    }

    public static bool GetKey(string keyName)
    {
        if (instance != null && instance.keys.ContainsKey(keyName))
            return Input.GetKey(instance.keys[keyName]);
        return false;
    }

    public static bool GetKeyUp(string keyName)
    {
        if (instance != null && instance.keys.ContainsKey(keyName))
            return Input.GetKeyUp(instance.keys[keyName]);
        return false;
    }
}