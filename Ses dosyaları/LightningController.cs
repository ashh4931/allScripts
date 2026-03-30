using UnityEngine;
using System.Collections;

public class LightningController : MonoBehaviour
{
    private ParticleSystem[] particles;

    void Awake()
    {
        particles = GetComponentsInChildren<ParticleSystem>();
    }

    public void PlayLightning(float duration = 1f)
    {
        StartCoroutine(PlayRoutine(duration));
    }

    IEnumerator PlayRoutine(float duration)
    {
        foreach (var ps in particles)
            ps.Play();

        yield return new WaitForSeconds(duration);

        foreach (var ps in particles)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
