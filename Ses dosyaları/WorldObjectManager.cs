using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldObjectManager : MonoBehaviour
{
    [System.Serializable]
    public class RespawnableObject
    {
        public string name;
        public GameObject prefab;
        public Vector3 position;
        public Quaternion rotation;
        public GameObject currentRef;
    }

    [Header("Ayarlar")]
    [Tooltip("Normal yenilenme süresi (300s = 5 Dakika)")]
    public float respawnInterval = 300f;

    [Header("Zamanlama")]
    public float initialStartDelay = 5f;

    private List<RespawnableObject> objectRegistry = new List<RespawnableObject>();
    private float nextCheckTime;
    private bool systemActivated = false;

    void Start()
    {
        // "DestructibleObject" tag'ine sahip tüm objeleri bul ve listeye ekle
        GameObject[] breakables = GameObject.FindGameObjectsWithTag("DestructibleObject");

        foreach (GameObject obj in breakables)
        {
            if (obj != null)
            {
                RespawnableObject data = new RespawnableObject();
                data.name = obj.name;
                data.prefab = obj;
                data.position = obj.transform.position;
                data.rotation = obj.transform.rotation;
                data.currentRef = obj;
                objectRegistry.Add(data);
            }
        }

        // Oyun başında belirlenen süre kadar bekle (5 saniye)

    }
    public void ActivateSystem()
    {
        StartCoroutine(ActivationRoutine());
    }
    IEnumerator ActivationRoutine()
    {
        // Başlangıç objelerini glitch ile aktif et
        foreach (var item in objectRegistry)
        {
            if (item.currentRef != null)
            {
                SpriteBootGlitch glitch = item.currentRef.GetComponent<SpriteBootGlitch>();
                if (glitch != null)
                {
                    glitch.SystemOnline();
                }
            }
        }

        systemActivated = true;
        nextCheckTime = Time.time + respawnInterval;
        yield return null;
    }
    void Update()
    {
        if (!systemActivated) return;

        if (Time.time >= nextCheckTime)
        {
            CheckAndRespawnObjects();
            nextCheckTime = Time.time + respawnInterval;
        }
    }

    void CheckAndRespawnObjects()
    {
        foreach (var item in objectRegistry)
        {
            if (item.currentRef == null)
            {
                Respawn(item);
            }
        }
    }

    void Respawn(RespawnableObject item)
    {
        GameObject newObj = Instantiate(item.prefab, item.position, item.rotation);
        item.currentRef = newObj;

        SpriteBootGlitch glitch = newObj.GetComponent<SpriteBootGlitch>();
        if (glitch != null)
        {
            glitch.SystemOnline();
        }
    }
}