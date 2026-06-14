using UnityEngine;
using DG.Tweening;

public class SeagullFlightDOTween : MonoBehaviour
{
    [Header("Flight Animation Settings")]
    public float flapDuration = 0.15f;
    public float flapScaleYMultiplier = 0.7f;
    public float tiltDuration = 0.3f;
    public float tiltAngle = 10f;

    [Header("Movement Settings")]
    public bool enableMovement = true;
    public Vector3 startLocalPosition = new Vector3(1000f, 300f, 0f);
    public Vector3 targetLocalPosition = new Vector3(-1000f, 100f, 0f);
    public float moveDuration = 5f;
    public Ease moveEase = Ease.Linear;

    private Tween flapTween;
    private Tween tiltTween;
    private Tween moveTween;
    private Vector3 originalScale;
    private Vector3 originalLocalPos;
    private bool hasCachedScale = false;
    private bool hasCachedPos = false;

    void OnEnable()
    {
        if (!hasCachedScale)
        {
            originalScale = transform.localScale;
            hasCachedScale = true;
        }

        if (!hasCachedPos)
        {
            originalLocalPos = transform.localPosition;
            hasCachedPos = true;
        }

        // 1. Wing flap (Scale Y squash/stretch)
        flapTween = transform.DOScaleY(originalScale.y * flapScaleYMultiplier, flapDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.Linear);

        // 2. Tilting (Z rotation)
        transform.localRotation = Quaternion.identity;
        tiltTween = transform.DOLocalRotate(new Vector3(0f, 0f, tiltAngle), tiltDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        // 3. Movement
        if (enableMovement)
        {
            transform.localPosition = startLocalPosition;
            moveTween = transform.DOLocalMove(targetLocalPosition, moveDuration)
                .SetEase(moveEase);
        }
    }

    void OnDisable()
    {
        KillTweens();
        RestoreOriginals();
    }

    void OnDestroy()
    {
        KillTweens();
    }

    private void KillTweens()
    {
        flapTween?.Kill();
        tiltTween?.Kill();
        moveTween?.Kill();
    }

    private void RestoreOriginals()
    {
        if (hasCachedScale)
        {
            transform.localScale = originalScale;
        }
        if (hasCachedPos)
        {
            transform.localPosition = originalLocalPos;
        }
        transform.localRotation = Quaternion.identity;
    }
}
