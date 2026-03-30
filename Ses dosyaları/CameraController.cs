using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.3f; // Kamera ne kadar sürede hedefe ulaşsın? (Düşük sayı = Daha hızlı takip)
    
    private Vector3 velocity = Vector3.zero;
    private Vector3 offset;
    private Vector3 shakeOffset;

    void Start()
    {
        if (target != null)
            offset = transform.position - target.position;
    }

void LateUpdate()
{
    if (target == null) return;

    // Hedef pozisyon (Z eksenini offset'ten alarak garantiye alıyoruz)
    Vector3 targetPosition = new Vector3(target.position.x + offset.x, target.position.y + offset.y, offset.z);

    // Yumuşatma
    Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

    // Uygulama (Z'yi kameranın mevcut Z'sine sabitlemek en güvenlisi)
    transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z) + shakeOffset;
}
    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    System.Collections.IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            shakeOffset = (Vector3)Random.insideUnitCircle * magnitude;
            elapsed += Time.deltaTime;
            yield return null;
        }
        shakeOffset = Vector3.zero;
    }
}