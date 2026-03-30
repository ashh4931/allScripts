using UnityEngine;
using System.Collections.Generic;

public class PlayerTurretController : MonoBehaviour
{
    [Header("Pivot Noktaları")]
    public Transform[] turretPivots; // 4 adet

    [Header("Envanter")]
    // Aynı taretten birden fazla alabileceği için listemiz hepsini tutacak
    public List<PlayerTurretData> ownedTurrets = new List<PlayerTurretData>();

    // Hangi pivotta HANGİ DATA takılı? (Dropdown menüsü için bilmemiz şart)
    public PlayerTurretData[] equippedData = new PlayerTurretData[4];
    
    // Hangi pivotta HANGİ OBJE (Oyun Dünyasında) var?
    private GameObject[] equippedTurretObjects = new GameObject[4];

    private PlayerStats stats;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
    }

    public bool BuyTurret(PlayerTurretData newTurret)
    {
        // Bu taretten elimizde kaç tane var sayalım
        int currentCount = 0;
        foreach (var t in ownedTurrets)
        {
            if (t == newTurret) currentCount++;
        }

        // Eğer 4 tane varsa daha fazla alamaz (Çünkü sadece 4 yuva var)
        if (currentCount >= 4)
        {
            Debug.Log("Bu taretten maksimum limite (4) ulaştın!");
            return false;
        }

        if (stats != null && stats.money >= newTurret.price)
        {
            stats.money -= newTurret.price;
            ownedTurrets.Add(newTurret);
            Debug.Log(newTurret.turretName + " satın alındı!");
            return true;
        }

        Debug.Log("Para yetersiz!");
        return false;
    }

    public void EquipTurret(PlayerTurretData turretToEquip, int pivotIndex)
    {
        if (pivotIndex < 0 || pivotIndex >= turretPivots.Length) return;

        // Önce mevcut yuvayı temizle
        UnequipTurret(pivotIndex);

        // Eğer gelen data NULL ise (yani Dropdown'dan "Yok" seçilmişse) sadece silmiş oluruz, gerisine gerek yok.
        if (turretToEquip == null) return;

        // Yeni turreti yarat ve pivota bağla
        Transform pivot = turretPivots[pivotIndex];
        GameObject spawnedTurret = Instantiate(turretToEquip.turretPrefab, pivot.position, pivot.rotation, pivot);
        
        AutoTurret autoScript = spawnedTurret.GetComponent<AutoTurret>();
        if (autoScript != null) autoScript.turretData = turretToEquip;

        // Hafızaya al
        equippedTurretObjects[pivotIndex] = spawnedTurret;
        equippedData[pivotIndex] = turretToEquip; // Dropdown'un neyin takılı olduğunu bilmesi için
    }

    public void UnequipTurret(int pivotIndex)
    {
        if (pivotIndex >= 0 && pivotIndex < equippedTurretObjects.Length)
        {
            if (equippedTurretObjects[pivotIndex] != null)
            {
                Destroy(equippedTurretObjects[pivotIndex]);
            }
            equippedTurretObjects[pivotIndex] = null;
            equippedData[pivotIndex] = null; // Datayı da boşaltıyoruz
        }
    }
}