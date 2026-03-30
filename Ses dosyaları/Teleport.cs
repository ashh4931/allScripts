using Unity.VisualScripting;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public GameObject VfxPrefab;
    public AudioClip teleportSound;
    private AudioSource audioSource;
    private Vector3 targetPosition;
    public  float shakeOffset = 1;
    public  float shakeDuration = 0.5f;

    public void Use()
    {
        Debug.Log("Teleport skill used.");

        Vector3 mousePos = Input.mousePosition;

        // Z eksenini 0 olarak ayarla (2D için)
        mousePos.z = 0f;

        // Ekran koordinatlarını dünya koordinatlarına çevir
        targetPosition = Camera.main.ScreenToWorldPoint(mousePos);



        Invoke("playSoundEffect", 0.3f);
        // 1 saniye sonra ışınlamayı yap
        Invoke("PlayVFXX", 0.8f);
        Invoke("DoTeleport", 1f);
    }

    void DoTeleport()
    {

        // Oyuncuyu hedef pozisyona ışınla
        GetComponentInParent<Rigidbody2D>().position = targetPosition;
        Camera.main.GetComponent<CameraController>().Shake(shakeDuration, shakeOffset);
        PlayVfx(GetComponentInParent<Rigidbody2D>().position);
    }
    private void playSoundEffect()
    {
        audioSource.PlayOneShot(teleportSound);
    }
    private void PlayVFXX()
    {
        GameObject vfx = Instantiate(VfxPrefab, GetComponentInParent<Rigidbody2D>().position, Quaternion.identity);
        ParticleSystem ps = vfx.GetComponent<ParticleSystem>(); if (ps != null)
        {
            Destroy(vfx, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {
            Destroy(vfx, 1.5f); // ParticleSystem yoksa 2 saniye sonra yok et
        }
    }
    public void PlayVfx(Vector3 position)
    {
        GameObject vfx = Instantiate(VfxPrefab, position, Quaternion.identity);
        ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            Destroy(vfx, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {
            Destroy(vfx, 1.5f); // ParticleSystem yoksa 2 saniye sonra yok et
        }
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
}

