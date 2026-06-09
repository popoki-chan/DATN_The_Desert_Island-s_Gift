using UnityEngine;
using DG.Tweening;
using Spine.Unity;

[RequireComponent(typeof(Interactable))]
public class GiveItem : MonoBehaviour
{
    [Header("1. View Nội Dung Giấy Note (Tùy chọn)")]
    public GameObject noteView;

    [Header("2. Thay đổi ngoại hình Nhân vật (Tùy chọn)")]
    public GameObject characterHoldingVisual;

    [Header("3. Hiệu ứng Bỏ Chạy (Dành cho Bạch Tuộc)")]
    public Transform escapePoint;
    public float runDuration = 1.5f;
    public Ease runEase = Ease.InBack;
    public bool fadeOutWhileRunning = true;

    [Tooltip("Kéo object 'Code' (hoặc các vật giấu kín) đang bị ẩn vào đây để nó hiện ra sau khi bạch tuộc chạy mất")]
    public GameObject[] objectsToRevealAfterFlee;

    [Header("4. Chờ ghép Animation (Spine 2D)")]
    [SpineAnimation]
    public string spineRunAnimation = "run";
    public bool loopSpineAnim = true;

    private Interactable coreLogic;
    private SkeletonAnimation skeletonAnimation;

    void Awake()
    {
        coreLogic = GetComponent<Interactable>();
        skeletonAnimation = GetComponent<SkeletonAnimation>();
    }

    void OnEnable()
    {
        if (coreLogic != null) coreLogic.OnDefaultInteract += HandleItemGiven;
    }

    void OnDisable()
    {
        if (coreLogic != null) coreLogic.OnDefaultInteract -= HandleItemGiven;
    }

    private void HandleItemGiven()
    {
        if (TryGetComponent<PopupBubble>(out var bubble))
        {
            bubble.Hide();
            bubble.enabled = false;
        }

        if (characterHoldingVisual != null)
        {
            characterHoldingVisual.SetActive(true);
        }

        if (escapePoint != null)
        {
            if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;

            if (skeletonAnimation != null && !string.IsNullOrEmpty(spineRunAnimation))
            {
                skeletonAnimation.AnimationState.SetAnimation(0, spineRunAnimation, loopSpineAnim);
            }

            Sequence runSeq = DOTween.Sequence();
            runSeq.Join(transform.DOMove(escapePoint.position, runDuration).SetEase(runEase));

            if (fadeOutWhileRunning)
            {
                if (skeletonAnimation != null)
                {
                    runSeq.Join(DOTween.To(() => skeletonAnimation.skeleton.A, x => skeletonAnimation.skeleton.A = x, 0f, runDuration));
                }
                else
                {
                    SpriteRenderer[] allSprites = GetComponentsInChildren<SpriteRenderer>();
                    foreach (var sr in allSprites)
                    {
                        runSeq.Join(sr.DOFade(0f, runDuration));
                    }
                }
            }

            runSeq.OnComplete(() =>
            {
                if (objectsToRevealAfterFlee != null && objectsToRevealAfterFlee.Length > 0)
                {
                    foreach (GameObject obj in objectsToRevealAfterFlee)
                    {
                        if (obj != null) obj.SetActive(true);
                    }
                }

                gameObject.SetActive(false);
            });

            return;
        }

        if (noteView != null && ViewManager.Instance != null)
        {
            ViewManager.Instance.ChangeView(noteView);
            coreLogic.isLocked = false;
            coreLogic.requiredItemId = "";
            coreLogic.isZoomable = true;
            coreLogic.targetView = noteView;
        }

        this.enabled = false;
    }

    void OnDestroy()
    {
        transform.DOKill();
    }
}