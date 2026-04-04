using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using System.Collections.Generic;
public class ElectricField : MonoBehaviour
{
    public GameObject shockPrefab;
    public GameObject electricFieldVFXPrefab;
    public AudioClip electricFieldSound;
    public AudioClip loadSound;
    public AudioClip openingSound;
private StatController statController;
    private AudioSource electricFieldAudio;
    private AudioSource openingAudio;

    GameObject VFX;
    private bool isActive = false;

    public float fadeDuration = 2f; // Fade süresi
public float manaCostBase = 1.5f; // temel mana maliyeti (size=1 için)
public float manaCostExponent = 1.3f; // üst kuvvet, 1 olursa lineer, 2 olursa kare, 3 olursa küp

private float fieldSize;
    void Awake()
    {
        // Electric Field için ayrı AudioSource
        electricFieldAudio = gameObject.AddComponent<AudioSource>();
        electricFieldAudio.playOnAwake = false;
        electricFieldAudio.loop = true;
        electricFieldAudio.spatialBlend = 0f; // 2D ses

        // Opening sound için ayrı AudioSource
        openingAudio = gameObject.AddComponent<AudioSource>();
        openingAudio.playOnAwake = false;
        openingAudio.loop = false;
        openingAudio.spatialBlend = 0f;
    }

    public void StartLoadSound()
    {
        electricFieldAudio.Stop();
        electricFieldAudio.clip = loadSound;
        electricFieldAudio.loop = true;
        electricFieldAudio.volume = 1f;
        electricFieldAudio.Play();
    }

    public void playOpeningSound()
    {
        openingAudio.Stop();
        openingAudio.clip = openingSound;
        openingAudio.volume = 1f;
        openingAudio.Play();
        StartCoroutine(FadeOut(openingAudio, fadeDuration));
    }

    private IEnumerator FadeOut(AudioSource source, float duration)
    {
        float startVolume = source.volume;

        while (source.volume > 0)
        {
            source.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        source.Stop();
        source.volume = startVolume;
    }

 public void use(float size)
{
    isActive = true;
    fieldSize = size;

    statController = GetComponentInParent<StatController>();

    electricFieldAudio.Stop();
    electricFieldAudio.clip = electricFieldSound;
    electricFieldAudio.loop = true;
    electricFieldAudio.volume = 1f;
    electricFieldAudio.Play();

    VFX = Instantiate(electricFieldVFXPrefab, transform.position, Quaternion.identity);
    VFX.transform.SetParent(this.transform);
    VFX.transform.localScale = Vector3.one * size;

    createCollider(size);

    StartCoroutine(DrainMana());
}

    private void createCollider(float size)
    {
        CircleCollider2D collider = VFX.AddComponent<CircleCollider2D>();
        collider.radius = size*1.5f;
        collider.isTrigger = true;
    }
private IEnumerator DrainMana()
{
    while (isActive)
    {
        // Üs ile exponential hesaplama: mana = base * (size^exponent) * deltaTime
        float manaCost = manaCostBase * Mathf.Pow(fieldSize, manaCostExponent) * Time.deltaTime;

        statController.UseMana(manaCost);

        // Mana biterse skill otomatik kapansın
        if (statController.GetComponentInParent<PlayerStats>().mana <= 0)
        {
            StopSkill();
            yield break;
        }

        yield return null;
    }   
}


IEnumerator ApplyShockContinuously(GameObject target)
{
    while (isActive && target != null)
    {
        GameObject shock = Instantiate(shockPrefab, target.transform.position, Quaternion.identity);
        shock.transform.SetParent(target.transform);

        yield return new WaitForSeconds(1f); // Her saniye
    }
}

private Dictionary<GameObject, Coroutine> activeShocks = new Dictionary<GameObject, Coroutine>();

void OnTriggerEnter2D(Collider2D collision)
{
    if (!activeShocks.ContainsKey(collision.gameObject))
    {
        Coroutine shockCoroutine = StartCoroutine(ApplyShockContinuously(collision.gameObject));
        activeShocks.Add(collision.gameObject, shockCoroutine);
    }
}

void OnTriggerExit2D(Collider2D collision)
{
    if (isActive && activeShocks.TryGetValue(collision.gameObject, out Coroutine shockCoroutine))
    {
        StopCoroutine(shockCoroutine);
        activeShocks.Remove(collision.gameObject);
    }
}



    public void StopSkill()
{
    isActive = false;

    electricFieldAudio.Stop();

    if (VFX != null)
        Destroy(VFX);
}

}
