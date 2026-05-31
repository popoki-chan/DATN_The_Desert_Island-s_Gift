using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

[RequireComponent(typeof(Interactable))]
public class SeagullPuzzle : MonoBehaviour
{
    [Header("1. Khai báo ID Quả")]
    public string uncarvedFruitId = "strange_fruit";
    public string carvedFruitId = "strange_fruit_txt";

    [Header("2. Các điểm neo")]
    public Transform spawnPoint;
    public Transform rockPoint;
    public Transform flyOutPoint;

    [Header("3. Cấu hình Hải Âu")]
    public Transform seagullVisual;
    public float flyDuration = 2f;

    [Header("4. Sự kiện & Bong bóng")]
    public UnityEvent onEndingTriggered;
    public PopupBubble popupBubble;
    public CutscenePlayer endingCutscenePlayer;

    [Header("5. Visual Quả khi cắp")]
    public GameObject uncarvedFruitVisual;
    public GameObject carvedFruitVisual;
    public Sprite carryingFruitSprite;
    public Sprite carryingCarvedFruitSprite;
    public Transform fruitHoldPoint;

    private SpriteRenderer seagullSpriteRenderer;
    private Sprite originalSprite;

    private bool isFlying = false;
    private Interactable interactable;
    private Tween delayedReturnTween;

    void Awake()
    {
        interactable = GetComponent<Interactable>();
        if (seagullVisual != null)
        {
            seagullSpriteRenderer = seagullVisual.GetComponent<SpriteRenderer>();
            if (seagullSpriteRenderer != null)
            {
                originalSprite = seagullSpriteRenderer.sprite;
            }
        }
    }

    void OnDestroy()
    {
        if (seagullVisual != null)
        {
            seagullVisual.DOKill();
        }
        delayedReturnTween?.Kill();
    }

    void Start()
    {
        if (uncarvedFruitVisual != null) uncarvedFruitVisual.SetActive(false);
        if (carvedFruitVisual != null) carvedFruitVisual.SetActive(false);
    }

    void OnEnable()
    {
        if (interactable != null) interactable.OnDefaultInteract += HandleInteraction;
    }

    void OnDisable()
    {
        if (interactable != null) interactable.OnDefaultInteract -= HandleInteraction;
    }

    private void HandleInteraction()
    {
        if (isFlying) return;

        Item selectedItem = Inventory.Instance?.currentSelectedItem;

        // Nếu tay không HOẶC item sai -> Gọi popup
        if (selectedItem == null || (selectedItem.itemId != uncarvedFruitId && selectedItem.itemId != carvedFruitId))
        {
            if (popupBubble != null) popupBubble.PlayAnimation();
            return;
        }

        // Nếu đúng item
        if (selectedItem.itemId == uncarvedFruitId)
        {
            Inventory.Instance.ConsumeSelectedItem();
            FlyAwayAndReturn();
        }
        else if (selectedItem.itemId == carvedFruitId)
        {
            Inventory.Instance.ConsumeSelectedItem();
            FlyAwayToEnding();
        }
    }

    private void FlyAwayAndReturn()
    {
        isFlying = true;
        if (popupBubble != null) popupBubble.Hide();
        if (uncarvedFruitVisual != null)
        {
            Transform parentTransform = fruitHoldPoint != null ? fruitHoldPoint : seagullVisual;
            if (parentTransform != null)
            {
                uncarvedFruitVisual.transform.SetParent(parentTransform);
                uncarvedFruitVisual.transform.localPosition = Vector3.zero;
                uncarvedFruitVisual.transform.localRotation = Quaternion.identity;
            }
            uncarvedFruitVisual.SetActive(true);
        }
        if (carvedFruitVisual != null) carvedFruitVisual.SetActive(false);
        if (seagullSpriteRenderer != null && carryingFruitSprite != null)
        {
            seagullSpriteRenderer.sprite = carryingFruitSprite;
        }

        seagullVisual.DOMove(flyOutPoint.position, flyDuration).OnComplete(() => {
            if (uncarvedFruitVisual != null) uncarvedFruitVisual.SetActive(false);
            if (seagullSpriteRenderer != null)
            {
                seagullSpriteRenderer.sprite = originalSprite;
            }
            seagullVisual.position = spawnPoint.position;
            delayedReturnTween = DOVirtual.DelayedCall(3f, () => {
                if (seagullVisual != null && rockPoint != null)
                {
                    seagullVisual.DOMove(rockPoint.position, flyDuration).OnComplete(() => isFlying = false);
                }
            });
        });
    }

    private void FlyAwayToEnding()
    {
        isFlying = true;
        if (popupBubble != null) popupBubble.Hide();
        if (uncarvedFruitVisual != null) uncarvedFruitVisual.SetActive(false);
        if (carvedFruitVisual != null)
        {
            Transform parentTransform = fruitHoldPoint != null ? fruitHoldPoint : seagullVisual;
            if (parentTransform != null)
            {
                carvedFruitVisual.transform.SetParent(parentTransform);
                carvedFruitVisual.transform.localPosition = Vector3.zero;
                carvedFruitVisual.transform.localRotation = Quaternion.identity;
            }
            carvedFruitVisual.SetActive(true);
        }
        if (seagullSpriteRenderer != null)
        {
            if (carryingCarvedFruitSprite != null)
            {
                seagullSpriteRenderer.sprite = carryingCarvedFruitSprite;
            }
            else if (carryingFruitSprite != null)
            {
                seagullSpriteRenderer.sprite = carryingFruitSprite;
            }
        }

        seagullVisual.DOMove(flyOutPoint.position, flyDuration).OnComplete(() => {
            onEndingTriggered?.Invoke();
            if (endingCutscenePlayer != null)
            {
                endingCutscenePlayer.PlayCutscene();
            }
        });
    }
}