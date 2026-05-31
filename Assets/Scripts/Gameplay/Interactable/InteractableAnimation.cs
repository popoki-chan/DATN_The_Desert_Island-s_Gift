using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
public class InteractableAnimation : MonoBehaviour
{
    public enum InteractionAction { None, Shake, Rotate, ShakeAndRotate }
    public enum CustomRotateMode { Relative, Toggle, Absolute }

    [Header("Hiệu ứng click")]
    public InteractionAction action = InteractionAction.None;

    [Header("Shake Setting")]
    public float shakeDuration = 0.6f;
    public float shakeAmplitude = 8f;
    public float shakeFrequency = 20f;

    [Header("DOTween Rotate Setting")]
    public CustomRotateMode rotateMode = CustomRotateMode.Relative;
    public float rotateAngle = 90f;
    public float targetAngle = 90f;
    public float rotateDuration = 0.4f;
    public Ease rotateEase = Ease.OutBack;

    [Header("Rotate Cài đặt thêm")]
    public bool rotateOnce = true;
    public bool allowShakeDuringRotate = true;

    [Header("DOTween Pickup Setting")]
    public float jumpHeight = 2f;       // Độ cao bay vút lên
    public float dipAmount = 0.4f;        // Độ trũng xuống sau khi bay lên (Boing)
    public float jumpDuration = 0.45f;     // Tổng thời gian bay và trũng

    [Header("Cài đặt Fade (Mờ dần)")]
    public bool useFade = false;           // Bật/tắt hiệu ứng mờ
    public float fadeDuration = 0.2f;     // Tốc độ mờ đi (Thường để ngắn hơn jumpDuration)

    // --- Internal State ---
    private Quaternion initialRotation;
    private float originalZAngle;
    private Interactable coreLogic;
    private SpriteRenderer spriteRenderer;

    private Coroutine shakeCoroutine = null;
    private bool isRotated = false;
    private bool toggleState = false;

    void Awake()
    {
        initialRotation = transform.rotation;
        originalZAngle = transform.eulerAngles.z;
        coreLogic = GetComponent<Interactable>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        if (coreLogic != null) coreLogic.OnDefaultInteract += TriggerFeedback;
    }

    void OnDisable()
    {
        if (coreLogic != null) coreLogic.OnDefaultInteract -= TriggerFeedback;
    }

    [ContextMenu("Test Trigger Feedback")]
    public void TriggerFeedback()
    {
        bool skipRotate = rotateOnce && isRotated;

        switch (action)
        {
            case InteractionAction.Shake:
                StartShake();
                break;

            case InteractionAction.Rotate:
                if (!skipRotate) StartRotate();
                break;

            case InteractionAction.ShakeAndRotate:
                if (skipRotate)
                {
                    if (allowShakeDuringRotate) StartShake();
                }
                else
                {
                    StartRotateThenShake();
                }
                break;
        }
    }

    // ==========================================
    // 1. SHAKE (DÙNG COROUTINE & MATHF.SIN CỦA BẠN)
    // ==========================================
    private void StartShake()
    {
        if (shakeCoroutine != null) return;

        // Tắt các hiệu ứng xoay DOTween đang chạy dở để tránh loạn góc
        transform.DOKill();
        shakeCoroutine = StartCoroutine(DoShakeRoutine());
    }

    private IEnumerator DoShakeRoutine()
    {
        float elapsed = 0f;
        Quaternion baseRot = transform.rotation;

        while (elapsed < shakeDuration)
        {
            float t = elapsed * shakeFrequency;
            // Công thức toán học Damped Sine Wave cực xịn của bạn
            float angle = Mathf.Sin(t) * shakeAmplitude * (1f - elapsed / shakeDuration);

            transform.rotation = baseRot * Quaternion.Euler(0f, 0f, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = baseRot;
        shakeCoroutine = null;
    }

    // ==========================================
    // 2. ROTATE (DÙNG DOTWEEN)
    // ==========================================
    private void StartRotate()
    {
        Tween rotTween = DoRotateTween();
        if (rotTween != null)
        {
            rotTween.OnComplete(() => {
                if (rotateOnce) isRotated = true;
            });
        }
    }

    private Tween DoRotateTween()
    {
        transform.DOKill(); // Dừng các xoay dở dang

        switch (rotateMode)
        {
            case CustomRotateMode.Relative:
                return transform.DORotate(new Vector3(0, 0, rotateAngle), rotateDuration, RotateMode.LocalAxisAdd).SetEase(rotateEase);

            case CustomRotateMode.Toggle:
                float endZ = !toggleState ? originalZAngle + rotateAngle : originalZAngle;
                toggleState = !toggleState;
                return transform.DORotate(new Vector3(0, 0, endZ), rotateDuration).SetEase(rotateEase);

            case CustomRotateMode.Absolute:
                return transform.DORotate(new Vector3(0, 0, targetAngle), rotateDuration).SetEase(rotateEase);
        }
        return null;
    }

    // ==========================================
    // 3. KẾT HỢP XOAY VÀ LẮC
    // ==========================================
    private void StartRotateThenShake()
    {
        Tween rotTween = DoRotateTween();

        if (rotTween != null)
        {
            rotTween.OnComplete(() => {
                if (rotateOnce) isRotated = true;
                // Khi DOTween xoay xong, lập tức gọi Coroutine Lắc của bạn
                StartShake();
            });
        }
        else
        {
            StartShake();
        }
    }

    // ==========================================
    // 4. ANIMATION NHẶT ĐỒ (DÙNG DOTWEEN)
    // ==========================================
    public void PlayPickupAnimation(Action onCompleteCallback)
    {
        transform.DOKill();
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);

        Sequence pickupSeq = DOTween.Sequence();

        // Lưu lại vị trí gốc
        float startY = transform.position.y;
        Vector3 startScale = transform.localScale;

        // Chia tỷ lệ thời gian: 60% thời gian đầu để bay lên, 40% để trũng xuống
        float upTime = jumpDuration * 0.6f;
        float downTime = jumpDuration * 0.4f;

        // NHỊP 1: Bay vút lên + Hơi phình to ra một chút (Tạo cảm giác căng mọng boing boing)
        pickupSeq.Append(transform.DOMoveY(startY + jumpHeight, upTime).SetEase(Ease.OutQuad));
        pickupSeq.Join(transform.DOScale(startScale * 1.1f, upTime).SetEase(Ease.OutQuad));

        // NHỊP 2: Rớt trũng xuống (Dip) + Teo nhỏ lại để chuẩn bị chui vào túi đồ
        pickupSeq.Append(transform.DOMoveY(startY + jumpHeight - dipAmount, downTime).SetEase(Ease.InOutSine));
        pickupSeq.Join(transform.DOScale(startScale * 0.7f, downTime).SetEase(Ease.InOutSine));

        // NHỊP 3: Mờ dần (Fade)
        if (useFade && spriteRenderer != null)
        {
            // Lệnh Insert giúp chèn hiệu ứng Fade vào thời điểm "upTime" 
            // Tức là ngay khi nó bay lên tới đỉnh và bắt đầu trũng xuống thì nó mới mờ đi
            pickupSeq.Insert(upTime, spriteRenderer.DOFade(0f, fadeDuration));
        }

        pickupSeq.OnComplete(() =>
        {
            onCompleteCallback?.Invoke();
        });
    }

    void OnDestroy()
    {
        transform.DOKill();
    }
}