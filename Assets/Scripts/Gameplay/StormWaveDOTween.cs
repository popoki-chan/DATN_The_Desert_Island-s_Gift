using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StormWaveDOTween : MonoBehaviour
{
    [Header("UV Scroll Settings")]
    [Tooltip("Tốc độ cuộn ngang của bề mặt sóng (hướng X).")]
    public float scrollSpeedX = 0.15f;
    
    [Header("Physical Swell Settings")]
    [Tooltip("Biên độ nhấp nhô lên xuống trục Y (px).")]
    public float yAmplitude = 25f;
    [Tooltip("Chu kỳ nhấp nhô Y (giây).")]
    public float yPeriod = 1.3f;
    
    [Tooltip("Biên độ dạt trái phải trục X (px).")]
    public float xAmplitude = 35f;
    [Tooltip("Chu kỳ dạt X (giây).")]
    public float xPeriod = 1.7f;

    [Header("Scale BreathingSettings")]
    [Tooltip("Tỷ lệ phóng to thu nhỏ tối đa theo chiều dọc.")]
    public float scaleYMultiplier = 1.15f;
    [Tooltip("Chu kỳ co giãn (giây).")]
    public float scalePeriod = 1.9f;

    private RectTransform rectTransform;
    private RawImage rawImage;
    private Vector2 initialAnchoredPos;
    private Vector3 initialScale;
    private Rect initialUvRect;

    private Sequence posSeq;
    private Sequence swaySeq;
    private Sequence scaleSeq;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rawImage = GetComponent<RawImage>();
        
        if (rectTransform != null)
        {
            initialAnchoredPos = rectTransform.anchoredPosition;
            initialScale = rectTransform.localScale;
        }

        if (rawImage != null)
        {
            initialUvRect = rawImage.uvRect;
        }
    }

    private void OnEnable()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (rawImage == null) rawImage = GetComponent<RawImage>();

        // Reset về vị trí ban đầu
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = initialAnchoredPos;
            rectTransform.localScale = initialScale;
        }

        if (rawImage != null)
        {
            rawImage.uvRect = initialUvRect;
        }

        // 1. Nhấp nhô trục Y (Lên/Xuống)
        if (rectTransform != null)
        {
            posSeq = DOTween.Sequence();
            posSeq.Append(rectTransform.DOAnchorPosY(initialAnchoredPos.y + yAmplitude, yPeriod * 0.5f).SetEase(Ease.InOutSine));
            posSeq.Append(rectTransform.DOAnchorPosY(initialAnchoredPos.y - yAmplitude, yPeriod * 0.5f).SetEase(Ease.InOutSine));
            posSeq.SetLoops(-1);

            // 2. Dạt trục X (Trái/Phải) - lệch chu kỳ để tạo sự tự nhiên
            swaySeq = DOTween.Sequence();
            swaySeq.Append(rectTransform.DOAnchorPosX(initialAnchoredPos.x - xAmplitude, xPeriod * 0.5f).SetEase(Ease.InOutSine));
            swaySeq.Append(rectTransform.DOAnchorPosX(initialAnchoredPos.x + xAmplitude, xPeriod * 0.5f).SetEase(Ease.InOutSine));
            swaySeq.SetLoops(-1);

            // 3. Phập phồng co giãn chiều dọc (Scale Y)
            scaleSeq = DOTween.Sequence();
            scaleSeq.Append(rectTransform.DOScaleY(initialScale.y * scaleYMultiplier, scalePeriod * 0.5f).SetEase(Ease.InOutQuad));
            scaleSeq.Append(rectTransform.DOScaleY(initialScale.y * (2f - scaleYMultiplier), scalePeriod * 0.5f).SetEase(Ease.InOutQuad));
            scaleSeq.SetLoops(-1);
        }
    }

    private void Update()
    {
        // 4. Cuộn bề mặt vân sóng liên tục bằng cách xê dịch UV Rect của RawImage
        if (rawImage != null)
        {
            var uv = rawImage.uvRect;
            uv.x += scrollSpeedX * Time.deltaTime;
            rawImage.uvRect = uv;
        }
    }

    private void OnDisable()
    {
        KillTweens();

        // Khôi phục trạng thái ban đầu khi tắt phân cảnh
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = initialAnchoredPos;
            rectTransform.localScale = initialScale;
        }

        if (rawImage != null)
        {
            rawImage.uvRect = initialUvRect;
        }
    }

    private void OnDestroy()
    {
        KillTweens();
    }

    private void KillTweens()
    {
        if (posSeq != null)
        {
            posSeq.Kill();
            posSeq = null;
        }
        if (swaySeq != null)
        {
            swaySeq.Kill();
            swaySeq = null;
        }
        if (scaleSeq != null)
        {
            scaleSeq.Kill();
            scaleSeq = null;
        }
    }
}
