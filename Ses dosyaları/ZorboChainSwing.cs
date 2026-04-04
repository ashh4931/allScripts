using UnityEngine;
using System.Collections.Generic;

public class ZorboDynamicChainBones : MonoBehaviour
{
    [Header("Kemik Kurulumu (Hiyerarşi Sırasıyla)")]
    // Hiyerarşideki kemiklerini sırasıyla buraya sürükle: Bone1, Bone2, Bone3...
    public Transform[] bones;

    [Header("Dinamik Ayarlar")]
    [Range(0f, 20f)] public float inertia = 5f;    // Çocuk kemikler ne kadar geride kalacak?
    [Range(0f, 20f)] public float spring = 5f;     // Kemiklerin merkeze dönme gücü
    [Range(0f, 1f)] public float damping = 0.8f;   // Enerjinin ne kadar hızlı kaybolacağı (Sürtünme)

    private Vector3 lastParentPosition;
    private List<float> boneAngles; // Her kemiğin şu anki prosedürel açısı
    private List<float> boneVelocities;

    void Start()
    {
        lastParentPosition = transform.position;
        boneAngles = new List<float>(new float[bones.Length]);
        boneVelocities = new List<float>(new float[bones.Length]);
    }

    void Update()
    {
        if (bones == null || bones.Length == 0) return;

        // 1. Ana objenin hareket hızını (velocity) hesapla
        Vector3 currentPos = transform.position;
        Vector3 parentVelocity = currentPos - lastParentPosition;
        lastParentPosition = currentPos;

        // 2. Hareketi rotasyonel güce (inertia) çevir
        // Sadece X eksenindeki hareketi alıyoruz (Sallanma hissi için)
        float forceX = parentVelocity.x;

        // 3. Kemik Hiyerarşisi Boyunca Gecikmeyi Uygula
        for (int i = 0; i < bones.Length; i++)
        {
            // Gecikme gücü hiyerarşide aşağı indikçe artmalı
            float childInertia = forceX * inertia * (i + 1);

            // Hız güncelleme: Eylemsizlik + Yay Gücü (Geri dönme)
            boneVelocities[i] += childInertia - (boneAngles[i] * spring * Time.deltaTime);

            // Enerji sönümleme (Damping)
            boneVelocities[i] *= damping;

            // Açı güncelleme
            boneAngles[i] += boneVelocities[i] * Time.deltaTime;

            // Kemiği kendi içinde rotasyonla büküyoruz (S-curve hissi)
            // Varsayılan rotasyon Euler(0,0,0) down bakıyor varsayılmıştır
            // Eğer zincir sağa bakıyorsa -90 ekleyerek aşağı bakmasını sağlarız.
            // Buradaki -90 değerini zincirin dik durduğu açıya göre (-90, 90 veya 180) değiştirebilirsin.
            bones[i].localRotation = Quaternion.Euler(0, 0, boneAngles[i]);
        }
    }
}