using UnityEngine;
using DG.Tweening;

public class MainMenuEnvironmentEffects : MonoBehaviour
{
    public enum EffectType { Sway, Wave }
    
    [Header("Loại hiệu ứng")]
    public EffectType effectType = EffectType.Sway;

    [Header("Cấu hình Sway (Đung đưa tán cây)")]
    public float swayAngle = 2f;
    public float swayDuration = 4f;

    [Header("Cấu hình Wave (Sóng nhấp nhô)")]
    public Vector2 waveOffset = new Vector2(15f, 10f);
    public float waveDuration = 5f;
    [Tooltip("Độ trễ bắt đầu hoạt ảnh (giúp lệch pha giữa các lớp sóng)")]
    public float startDelay = 0f;
    [Tooltip("Đảo ngược hướng di chuyển của sóng")]
    public bool invertDirection = false;

    private Vector3 startLocalPos;
    private Vector3 startLocalRot;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Lưu vị trí và rotation ban đầu
        startLocalPos = rectTransform != null ? (Vector3)rectTransform.anchoredPosition : transform.localPosition;
        startLocalRot = transform.localEulerAngles;

        ApplyEffect();
    }

    void OnDestroy()
    {
        // Dọn dẹp tween tránh rò rỉ bộ nhớ
        transform.DOKill();
        if (rectTransform != null) rectTransform.DOKill();
    }

    private void ApplyEffect()
    {
        if (effectType == EffectType.Sway)
        {
            // Tắt Animator nếu có để tránh xung đột ghi đè transform
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = false;
            }

            float startZ = startLocalRot.z;
            if (startZ > 180f) startZ -= 360f; // Chuẩn hóa góc quay float

            // Xoay nhẹ tán cây từ Z-swayAngle sang Z+swayAngle qua lại vô tận
            transform.localEulerAngles = new Vector3(0f, 0f, startZ - swayAngle);
            transform.DOLocalRotate(new Vector3(0f, 0f, startZ + swayAngle), swayDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
        else if (effectType == EffectType.Wave)
        {
            // Sóng vỗ chéo tịnh tiến nhẹ
            Vector2 actualOffset = invertDirection ? -waveOffset : waveOffset;
            if (rectTransform != null)
            {
                Vector2 startAnchorPos = rectTransform.anchoredPosition;
                rectTransform.anchoredPosition = startAnchorPos - actualOffset * 0.5f;
                var t = rectTransform.DOAnchorPos(startAnchorPos + actualOffset * 0.5f, waveDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
                if (startDelay > 0f)
                {
                    t.SetDelay(startDelay);
                }
            }
            else
            {
                Vector3 actualOffset3 = new Vector3(actualOffset.x, actualOffset.y, 0f);
                transform.localPosition = startLocalPos - actualOffset3 * 0.5f;
                var t = transform.DOLocalMove(startLocalPos + actualOffset3 * 0.5f, waveDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
                if (startDelay > 0f)
                {
                    t.SetDelay(startDelay);
                }
            }
        }
    }
}
