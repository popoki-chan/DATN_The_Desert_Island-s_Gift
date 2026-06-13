using UnityEngine;
using DG.Tweening;

public class PanBackgroundDOTween : MonoBehaviour
{
    [Header("Panning Settings")]
    [Tooltip("Nếu tích, sẽ sử dụng vị trí X hiện tại trong Editor làm điểm xuất phát.")]
    public bool useCurrentXAsStart = true;
    
    [Tooltip("Vị trí X bắt đầu di chuyển (nếu không dùng vị trí hiện tại).")]
    public float startX = 157f;
    
    [Tooltip("Vị trí X đích đến.")]
    public float targetX = -157f;
    
    [Tooltip("Thời gian di chuyển (giây) - nên khớp với độ dài Cut1 (5s).")]
    public float duration = 5f;
    
    [Tooltip("Loại chuyển động mượt mà.")]
    public Ease easeType = Ease.Linear;

    private RectTransform rectTransform;
    private Vector2 initialAnchoredPos;
    private Tween panTween;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            initialAnchoredPos = rectTransform.anchoredPosition;
            if (useCurrentXAsStart)
            {
                startX = initialAnchoredPos.x;
            }
        }
    }

    private void OnEnable()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null) return;

        // Reset về vị trí bắt đầu
        rectTransform.anchoredPosition = new Vector2(startX, initialAnchoredPos.y);

        // Bắt đầu di chuyển từ phải qua trái
        panTween = rectTransform.DOAnchorPosX(targetX, duration)
            .SetEase(easeType);
    }

    private void OnDisable()
    {
        KillTween();

        // Trả về vị trí mặc định khi tắt
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = initialAnchoredPos;
        }
    }

    private void OnDestroy()
    {
        KillTween();
    }

    private void KillTween()
    {
        if (panTween != null)
        {
            panTween.Kill();
            panTween = null;
        }
    }
}
