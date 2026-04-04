using UnityEngine;

public class NPCWeaponHandler : MonoBehaviour
{
    private Transform firePoint;
    private GameObject currentWeapon;

    public void EquipRandomWeapon(GameObject[] weaponPrefabs)
    {
        if (weaponPrefabs == null || weaponPrefabs.Length == 0) return;

        // Rastgele silah seç ve El (HandPivot) objesinin içine doğur
        GameObject randomWeapon = weaponPrefabs[Random.Range(0, weaponPrefabs.Length)];
        currentWeapon = Instantiate(randomWeapon, transform.position, transform.rotation, transform);
        //currentWeapon.GetComponents<weapon>().isEquipped = true;
        // SİHRİ BURADA YAPIYORUZ: Oyuncuya ait scriptleri kapatıyoruz ki NPC bizim tuşlarımızla ateş etmesin!
        MonoBehaviour[] allScripts = currentWeapon.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in allScripts)
        {
            script.enabled = false;
        }

        // Silahın içindeki FirePoint'i (namluyu) buluyoruz
        Transform[] allChildren = currentWeapon.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.name == "firePoint" || child.name == "FirePoint")
            {
                firePoint = child;
                break;
            }
        }
    }
    // AudioSource ve AudioClip parametrelerini ekledik
    // 🔴 DEĞİŞTİRİLEN: Fonksiyona "volume" parametresi eklendi
    public void Fire(GameObject bulletPrefab, AudioSource source, AudioClip fireClip, float volume = 1f)
    {
        if (firePoint != null && bulletPrefab != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // Sesi çal
            if (source != null && fireClip != null)
            {
                source.pitch = Random.Range(0.85f, 1.15f);

                // 🔴 DEĞİŞTİRİLEN: PlayOneShot'ın ikinci parametresi sesi (volume) belirler!
                source.PlayOneShot(fireClip, volume);
            }
        }
    }
}