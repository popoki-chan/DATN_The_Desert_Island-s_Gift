using UnityEngine;
using TMPro;
using DG.Tweening;

public class ItemPickupNotificationUI : MonoBehaviour
{
    [Header("UI Components")]
    public RectTransform notificationPanel;
    public TextMeshProUGUI itemNameText;

    [Header("Animation Settings")]
    public float slideDuration = 0.4f;
    public float displayDuration = 2.0f;
    public float hiddenY = 100f;
    public float visibleY = -80f;

    [Header("Audio Settings")]
    public AudioClip defaultNotificationSfx;

    private Sequence activeSequence;
    private Inventory cachedInventory;

    void Start()
    {
        if (notificationPanel != null)
        {
            Vector2 pos = notificationPanel.anchoredPosition;
            pos.y = hiddenY;
            notificationPanel.anchoredPosition = pos;
            notificationPanel.gameObject.SetActive(false);
        }
        if (Inventory.Instance != null)
        {
            cachedInventory = Inventory.Instance;
            cachedInventory.OnItemAdded += HandleItemAdded;
        }
    }

    void OnDestroy()
    {
        if (cachedInventory != null)
        {
            cachedInventory.OnItemAdded -= HandleItemAdded;
            cachedInventory = null;
        }
        activeSequence?.Kill();
    }

    private void HandleItemAdded(Item item)
    {
        if (item == null || notificationPanel == null || itemNameText == null) return;
        activeSequence?.Kill();

        itemNameText.text = $"{item.itemName}";

        if (defaultNotificationSfx != null)
        {
            AudioManager.Instance?.PlaySFX(defaultNotificationSfx);
        }

        notificationPanel.gameObject.SetActive(true);
        
        activeSequence = DOTween.Sequence();

        activeSequence.Append(notificationPanel.DOAnchorPosY(visibleY, slideDuration).SetEase(Ease.OutBack));
        activeSequence.AppendInterval(displayDuration);
        activeSequence.Append(notificationPanel.DOAnchorPosY(hiddenY, slideDuration).SetEase(Ease.InQuad));
        
        activeSequence.OnComplete(() => {
            notificationPanel.gameObject.SetActive(false);
        });
    }
}
