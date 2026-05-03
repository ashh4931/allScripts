
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class LootObject : MonoBehaviour
{

    [Header("Ses Ayarları")]
    [SerializeField] private AudioClip collectSound;
    [Range(0f, 1f)][SerializeField] private float volume = 0.8f;

    [Header("Ses Çeşitliliği (Pitch)")]
    [Range(0f, 0.3f)][SerializeField] private float pitchRandomness = 0.1f;

    [Header("Efekt Ayarları")]
    [SerializeField] private GameObject collectParticlePrefab; // Buraya patlama/parlama prefabını koyun
    [SerializeField] private float particleDestroyDelay = 2f; // Partikülün silinme süresi

    public GameObject player;
    [Header("Eşya Verisi")]
    public ItemData data;


    public void OnCollected()
    {
        // Ses ve Partikül efektlerini burada çalıştır
        PlaySound2D();
        SpawnParticle();

        // Eşyayı yok et
        Destroy(gameObject);
    }

    // PlaySound2D ve SpawnParticle fonksiyonların burada kalabilir...


    private void PlaySound2D()
    {
        if (collectSound == null) return;

        GameObject tempGO = new GameObject("TempAudio_" + collectSound.name);

        // HİLE BURADA: Obje oluşur oluşmaz kameranın pozisyonuna ışınla ve kameraya bağla
        if (Camera.main != null)
        {
            tempGO.transform.SetParent(Camera.main.transform);
            tempGO.transform.localPosition = Vector3.zero; // Tam kameranın içi
        }

        AudioSource source = tempGO.AddComponent<AudioSource>();

        source.clip = collectSound;
        source.volume = volume;

        // 0f zaten 2D'dir ama kameraya bağladığımız için her ihtimale karşı 
        // sesin hiçbir şekilde uzaklığa göre azalmamasını garantiliyoruz.
        source.spatialBlend = 0f;
        source.rolloffMode = AudioRolloffMode.Linear; // Mesafe hesaplamasını devre dışı bırakır

        float randomPitch = Random.Range(1f - pitchRandomness, 1f + pitchRandomness);
        source.pitch = randomPitch;

        source.Play();

        // Pitch değişince sesin süresi de değişir, yoksa ses bitmeden obje silinebilir
        float destroyDelay = collectSound.length / Mathf.Max(0.01f, source.pitch);
        Destroy(tempGO, destroyDelay);
    }

    private void SpawnParticle()
    {
        if (collectParticlePrefab != null)
        {
            // Partikülü eşyanın olduğu pozisyonda oluştur
            GameObject effect = Instantiate(collectParticlePrefab, transform.position, Quaternion.identity);

            // Belirli bir süre sonra (örneğin 2 saniye) efekti sahneden sil
            Destroy(effect, particleDestroyDelay);
        }
    }



}