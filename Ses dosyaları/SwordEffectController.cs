using Unity.Mathematics;
using UnityEngine;

public class SwordEffectController : MonoBehaviour
{
    public GameObject vfxPrefab;


    public void PlayVfx(Vector3 position)
    {
        if(vfxPrefab != null)
        {
            GameObject vfx = Instantiate(vfxPrefab,position, quaternion.identity);
            ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                Destroy(vfx, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(vfx, 2f); // ParticleSystem yoksa 2 saniye sonra yok et
            }
        }
    }


}
