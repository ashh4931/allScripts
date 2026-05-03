using UnityEngine;

public class Teleport : MonoBehaviour
{
    public GameObject VfxPrefab;
    public AudioClip teleportSound;
    private AudioSource audioSource;
    private Vector3 targetPosition;
    public float shakeOffset = 1;
    public float shakeDuration = 0.5f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Use()
    {
        // 🔴 DÜZELTME: Hedef hesaplaması DoubleTeleport'taki gibi yapıldı
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f; 
        targetPosition = mousePos;

        Invoke("playSoundEffect", 0.3f);
        Invoke("PlayVFXX", 0.8f);
        Invoke("DoTeleport", 1f);
    }

    void DoTeleport()
    {
        GetComponentInParent<Rigidbody2D>().position = targetPosition;
        
        // 🔴 GÜVENLİK: Eğer kamerada CameraController yoksa çökmesini engeller
        CameraController camController = Camera.main.GetComponent<CameraController>();
        if (camController != null) camController.Shake(shakeDuration, shakeOffset);
        
        PlayVfx(GetComponentInParent<Rigidbody2D>().position);
    }

    private void playSoundEffect()
    {
        // 🔴 GÜVENLİK: Ses dosyası atanmamışsa çökmemesi için kontrol eklendi
        if (audioSource != null && teleportSound != null)
        {
            audioSource.PlayOneShot(teleportSound);
        }
    }

    private void PlayVFXX()
    {
        PlayVfx(GetComponentInParent<Rigidbody2D>().position);
    }

    public void PlayVfx(Vector3 position)
    {
        if (VfxPrefab == null) return;
        
        GameObject vfx = Instantiate(VfxPrefab, position, Quaternion.identity);
        ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
        
        if (ps != null) Destroy(vfx, ps.main.duration + ps.main.startLifetime.constantMax);
        else Destroy(vfx, 1.5f);
    }
}