using UnityEngine;

public class BaseSword : MonoBehaviour
{
    public TrailRenderer swordTrail;

    void Awake()
    {
        if (swordTrail == null) 
            swordTrail = GetComponentInChildren<TrailRenderer>();

        if (swordTrail != null) 
        {
            swordTrail.emitting = false;
            Debug.Log("<color=green>BaseSword:</color> TrailRenderer başarıyla bulundu: " + gameObject.name);
        }
        else
        {
            Debug.LogError("<color=red>BaseSword:</color> TrailRenderer BULUNAMADI! Objenin altında bir TrailRenderer olduğundan emin ol.");
        }
    }

    public void SetTrail(bool state)
    {
        if (swordTrail != null)
        {
            swordTrail.emitting = state;
            Debug.Log("<color=cyan>BaseSword:</color> Trail durumu değişti: " + state);
        }
    }
}