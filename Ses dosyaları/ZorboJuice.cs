using UnityEngine;

public partial class ZorboJuice : MonoBehaviour
{
    [Header("Yüzme (Floating) Ayarları")]
    public bool enableFloating = true;
    public float floatSpeed = 2f;
    public float floatAmount = 10f;
    public float timeOffset = 0f; // Parçaların aynı anda inip kalkmaması için

    [Header("Nefes Alma (Breathing) Ayarları")]
    public bool enableBreathing = false;
    public float breathSpeed = 1.5f;
    public float breathAmount = 0.05f;

    private Vector3 startPos;
    private Vector3 startScale;

    void Start()
    {
        startPos = transform.localPosition;
        startScale = transform.localScale;
    }

    void Update()
    {
        // 1a. Yüzen Parçalar (Eller ve Kafa için)
        if (enableFloating)
        {
            float newY = Mathf.Sin((Time.time + timeOffset) * floatSpeed) * floatAmount;
            transform.localPosition = startPos + new Vector3(0, newY, 0);
        }

        // 1c. Nefes Alma (Gövde için)
        if (enableBreathing)
        {
            float scaleMod = Mathf.Sin(Time.time * breathSpeed) * breathAmount;
            transform.localScale = startScale + new Vector3(scaleMod, scaleMod, 0);
        }
    }
}