using UnityEngine;
using System.Collections.Generic;

public class ZorboDynamicChainBones : MonoBehaviour
{
    [Header("Kemik Kurulumu (Hiyerarşi Sırasıyla)")]
    public Transform[] bones;

    [Header("Dinamik Ayarlar")]
    [Range(0f, 20f)] public float inertia = 1.5f;   
    [Range(0f, 20f)] public float spring = 15f;     
    [Range(0f, 1f)] public float damping = 0.85f;   
    
    [Header("Güvenlik Sınırı")]
    public float maxVelocityForce = 0.5f; // Zincirin çıldırmasını önleyen hız sınırı

    private Vector3 lastParentPosition;
    private List<float> boneAngles; 
    private List<float> boneVelocities;
    private bool isFirstFrame = true; 

    void Awake()
    {
        boneAngles = new List<float>(new float[bones.Length]);
        boneVelocities = new List<float>(new float[bones.Length]);
    }

    void OnEnable()
    {
        isFirstFrame = true; 
        if (boneAngles != null)
        {
            for (int i = 0; i < boneAngles.Count; i++)
            {
                boneAngles[i] = 0f;
                boneVelocities[i] = 0f;
                if (bones != null && i < bones.Length && bones[i] != null)
                {
                    bones[i].localRotation = Quaternion.Euler(0, 0, 0);
                }
            }
        }
    }

    void Update()
    {
        if (bones == null || bones.Length == 0) return;

        if (isFirstFrame)
        {
            lastParentPosition = transform.position;
            isFirstFrame = false;
            return;
        }

        Vector3 currentPos = transform.position;
        Vector3 parentVelocity = currentPos - lastParentPosition;
        lastParentPosition = currentPos;

        // FİZİK PATLAMASI ENGELLEYİCİSİ (Hızı limitliyoruz)
        float forceX = Mathf.Clamp(parentVelocity.x, -maxVelocityForce, maxVelocityForce);

        for (int i = 0; i < bones.Length; i++)
        {
            float childInertia = forceX * inertia * (i + 1);

            boneVelocities[i] += childInertia - (boneAngles[i] * spring * Time.unscaledDeltaTime);
            boneVelocities[i] *= damping;
            boneAngles[i] += boneVelocities[i] * Time.unscaledDeltaTime;

            bones[i].localRotation = Quaternion.Euler(0, 0, boneAngles[i]);
        }
    }
}