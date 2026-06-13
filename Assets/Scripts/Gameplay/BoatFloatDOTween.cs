using UnityEngine;
using DG.Tweening;

public class BoatFloatDOTween : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector2 initialAnchoredPos;
    private Vector3 initialLocalRotation;
    
    private Sequence posSeq;
    private Sequence rotSeq;

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

        // Reset to initial state
        rectTransform.anchoredPosition = initialAnchoredPos;
        rectTransform.localEulerAngles = initialLocalRotation;

        // Float up and down sequence (total 3s loop)
        posSeq = DOTween.Sequence();
        posSeq.Append(rectTransform.DOAnchorPosY(initialAnchoredPos.y + 15f, 0.75f).SetEase(Ease.OutSine));
        posSeq.Append(rectTransform.DOAnchorPosY(initialAnchoredPos.y - 15f, 1.5f).SetEase(Ease.InOutSine));
        posSeq.Append(rectTransform.DOAnchorPosY(initialAnchoredPos.y, 0.75f).SetEase(Ease.InSine));
        posSeq.SetLoops(-1);

        // Rotation tilt sequence (total 3s loop)
        rotSeq = DOTween.Sequence();
        rotSeq.Append(rectTransform.DOLocalRotate(new Vector3(0f, 0f, 2.5f), 0.75f).SetEase(Ease.OutSine));
        rotSeq.Append(rectTransform.DOLocalRotate(new Vector3(0f, 0f, -2.5f), 1.5f).SetEase(Ease.InOutSine));
        rotSeq.Append(rectTransform.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.75f).SetEase(Ease.InSine));
        rotSeq.SetLoops(-1);
    }

    private void OnDisable()
    {
        KillTweens();

        // Restore to initial state
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
        if (posSeq != null)
        {
            posSeq.Kill();
            posSeq = null;
        }
        if (rotSeq != null)
        {
            rotSeq.Kill();
            rotSeq = null;
        }
    }
}
