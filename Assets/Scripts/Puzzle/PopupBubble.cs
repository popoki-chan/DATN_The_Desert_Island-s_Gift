using UnityEngine;
using DG.Tweening;

public class PopupBubble : MonoBehaviour
{
    public GameObject dotSmallVisual, dotLargeVisual, contentVisual;
    public float showDuration = 3.0f, dotPopTime = 0.2f, expandTime = 0.4f;
    public bool playOnDefaultInteract = true;

    private Sequence currentSequence;
    private Interactable interactable;

    void Awake()
    {
        interactable = GetComponent<Interactable>();
    }

    void Start()
    {
        Hide();
    }

    void OnEnable()
    {
        if (playOnDefaultInteract && interactable != null) interactable.OnDefaultInteract += PlayAnimation;
    }

    void OnDisable()
    {
        if (playOnDefaultInteract && interactable != null) interactable.OnDefaultInteract -= PlayAnimation;
        if (currentSequence != null) currentSequence.Kill();
    }

    public void PlayAnimation()
    {
        if (!enabled) return;
        if (currentSequence != null && currentSequence.IsActive()) return;

        if (dotSmallVisual) dotSmallVisual.SetActive(true);
        if (dotLargeVisual) dotLargeVisual.SetActive(true);
        if (contentVisual) contentVisual.SetActive(true);

        currentSequence = DOTween.Sequence();

        if (dotSmallVisual)
        {
            dotSmallVisual.transform.localScale = Vector3.zero;
            currentSequence.Append(dotSmallVisual.transform.DOScale(Vector3.one, dotPopTime).SetEase(Ease.OutBack));
        }
        if (dotLargeVisual)
        {
            dotLargeVisual.transform.localScale = Vector3.zero;
            currentSequence.AppendInterval(0.05f);
            currentSequence.Append(dotLargeVisual.transform.DOScale(Vector3.one, dotPopTime).SetEase(Ease.OutBack));
        }
        if (contentVisual)
        {
            contentVisual.transform.localScale = Vector3.zero;
            currentSequence.Append(contentVisual.transform.DOScaleX(1f, expandTime).SetEase(Ease.OutQuint));
            currentSequence.Join(contentVisual.transform.DOScaleY(1f, expandTime * 0.7f).SetEase(Ease.OutCubic));
        }

        currentSequence.AppendInterval(showDuration);

        if (contentVisual) currentSequence.Append(contentVisual.transform.DOScale(Vector3.zero, 0.1f).SetEase(Ease.InQuad));
        if (dotLargeVisual) currentSequence.Join(dotLargeVisual.transform.DOScale(Vector3.zero, 0.08f).SetEase(Ease.InQuad));
        if (dotSmallVisual) currentSequence.Join(dotSmallVisual.transform.DOScale(Vector3.zero, 0.08f).SetEase(Ease.InQuad));

        currentSequence.OnComplete(() => currentSequence = null);
    }

    public void Hide()
    {
        if (currentSequence != null)
        {
            currentSequence.Kill();
            currentSequence = null;
        }
        if (dotSmallVisual) dotSmallVisual.SetActive(false);
        if (dotLargeVisual) dotLargeVisual.SetActive(false);
        if (contentVisual) contentVisual.SetActive(false);
    }
}