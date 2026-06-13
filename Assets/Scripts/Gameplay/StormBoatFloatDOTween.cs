using UnityEngine;
using DG.Tweening;

public class StormBoatFloatDOTween : MonoBehaviour
{
    [Header("Storm Movement Settings")]
    [Tooltip("Biên độ nhấp nhô trục Y tối đa (px).")]
    public float yAmplitude = 45f;
    
    [Tooltip("Thời gian cơ bản cho một chu kỳ nhấp nhô (giây).")]
    public float yPeriod = 1.5f;
    
    [Tooltip("Biên độ nghiêng lắc tối đa (độ).")]
    public float rotationAmplitude = 15f;
    
    [Tooltip("Thời gian cơ bản cho một chu kỳ nghiêng lắc (giây).")]
    public float rotPeriod = 1.7f;

    private RectTransform rectTransform;
    private Vector2 initialAnchoredPos;
    private Vector3 initialLocalRotation;
    
    private Sequence stormPosSeq;
    private Sequence stormRotSeq;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            initialAnchoredPos = rectTransform.anchoredPosition;
            initialLocalRotation = rectTransform.localEulerAngles;
        }
    }

    private void OnEnable()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null) return;

        // Reset về vị trí xuất phát ban đầu
        rectTransform.anchoredPosition = initialAnchoredPos;
        rectTransform.localEulerAngles = initialLocalRotation;

        // Tạo chuỗi chuyển động nhấp nhô không đều (lên nhanh chậm khác nhau để tạo cảm giác sóng xô)
        stormPosSeq = DOTween.Sequence();
        // Trồi lên nhanh (OutQuad)
        stormPosSeq.Append(rectTransform.DOAnchorPosY(initialAnchoredPos.y + yAmplitude * 0.7f, yPeriod * 0.25f).SetEase(Ease.OutQuad));
        // Sụt sâu xuống chân sóng (InQuad)
        stormPosSeq.Append(rectTransform.DOAnchorPosY(initialAnchoredPos.y - yAmplitude, yPeriod * 0.35f).SetEase(Ease.InQuad));
        // Trồi nhẹ lên trở lại (OutSine)
        stormPosSeq.Append(rectTransform.DOAnchorPosY(initialAnchoredPos.y + yAmplitude * 0.4f, yPeriod * 0.2f).SetEase(Ease.OutSine));
        // Rơi nhẹ về vị trí cân bằng
        stormPosSeq.Append(rectTransform.DOAnchorPosY(initialAnchoredPos.y, yPeriod * 0.2f).SetEase(Ease.InOutSine));
        stormPosSeq.SetLoops(-1);

        // Tạo chuỗi nghiêng lắc lệch pha với nhấp nhô (tạo sự hỗn loạn tự nhiên của bão tố)
        stormRotSeq = DOTween.Sequence();
        // Nghiêng phải nhanh
        stormRotSeq.Append(rectTransform.DOLocalRotate(new Vector3(0f, 0f, rotationAmplitude * 0.7f), rotPeriod * 0.25f).SetEase(Ease.OutSine));
        // Đập nghiêng sang trái dữ dội
        stormRotSeq.Append(rectTransform.DOLocalRotate(new Vector3(0f, 0f, -rotationAmplitude), rotPeriod * 0.35f).SetEase(Ease.InOutQuad));
        // Lắc nhẹ ngược lại
        stormRotSeq.Append(rectTransform.DOLocalRotate(new Vector3(0f, 0f, rotationAmplitude * 0.3f), rotPeriod * 0.2f).SetEase(Ease.OutSine));
        // Trở về cân bằng
        stormRotSeq.Append(rectTransform.DOLocalRotate(new Vector3(0f, 0f, 0f), rotPeriod * 0.2f).SetEase(Ease.InOutSine));
        stormRotSeq.SetLoops(-1);
    }

    private void OnDisable()
    {
        KillTweens();

        // Khôi phục vị trí mặc định khi tắt
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = initialAnchoredPos;
            rectTransform.localEulerAngles = initialLocalRotation;
        }
    }

    private void OnDestroy()
    {
        KillTweens();
    }

    private void KillTweens()
    {
        if (stormPosSeq != null)
        {
            stormPosSeq.Kill();
            stormPosSeq = null;
        }
        if (stormRotSeq != null)
        {
            stormRotSeq.Kill();
            stormRotSeq = null;
        }
    }
}
