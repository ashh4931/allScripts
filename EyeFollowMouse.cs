using UnityEngine;

public class ZorboFocusEyeFollow : MonoBehaviour
{
    public RectTransform leftPupil;
    public RectTransform rightPupil;
    public Camera uiCamera; 

    [Header("Referanslar")]
    // Karakterin merkezi veya gözlerin olduğu yer (Sağ/Sol ayrımı için)
    public Transform characterReference; 
    // Fare sağa geçince bakılacak sabit hedef (Boş bırakma!)
    public Transform focusTarget; 

    [Header("Ayarlar")]
    public float movementRadius = 40f; 
    public float smoothness = 20f;
    // Karakterin tam merkezinden ne kadar sağa gidince odak değişsin? (Offset)
    public float rightSideThreshold = 0f; 

    void Update()
    {
        if (leftPupil == null || rightPupil == null || uiCamera == null || characterReference == null) return;

        // 1. Koordinatları al
        Vector2 mousePos = Input.mousePosition;
        Vector2 charScreenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, characterReference.position);
        
        Vector2 currentTargetPos;

        // 2. Kontrol: Fare karakterin sağında mı?
        if (mousePos.x > charScreenPos.x + rightSideThreshold)
        {
            // Eğer sağdaysa, fareyi değil belirlediğin Focus Target'ı takip et
            if (focusTarget != null)
                currentTargetPos = RectTransformUtility.WorldToScreenPoint(uiCamera, focusTarget.position);
            else
                currentTargetPos = charScreenPos; // Target yoksa merkeze bak
        }
        else
        {
            // Sol taraftaysa normal fare takibi
            currentTargetPos = mousePos;
        }

        // 3. Gözleri güncelle
        UpdateEye(leftPupil, currentTargetPos);
        UpdateEye(rightPupil, currentTargetPos);
    }

    void UpdateEye(RectTransform pupil, Vector2 targetPos)
    {
        Vector2 socketScreenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, pupil.parent.position);
        Vector2 direction = targetPos - socketScreenPos;

        Vector2 targetOffset = Vector2.ClampMagnitude(direction, movementRadius);
        pupil.anchoredPosition = Vector2.Lerp(pupil.anchoredPosition, targetOffset, Time.deltaTime * smoothness);
    }
}