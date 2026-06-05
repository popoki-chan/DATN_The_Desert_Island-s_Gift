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

    void Start()
    {
        // Set initial position to hidden
        if (notificationPanel != null)
        {
            Vector2 pos = notificationPanel.anchoredPosition;
            pos.y = hiddenY;
            notificationPanel.anchoredPosition = pos;
            notificationPanel.gameObject.SetActive(false);
        }

        // Subscribe to item added event
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnItemAdded += HandleItemAdded;
        }
    }

    void OnDestroy()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnItemAdded -= HandleItemAdded;
        }
        activeSequence?.Kill();
    }

    private void HandleItemAdded(Item item)
    {
        if (item == null || notificationPanel == null || itemNameText == null) return;

        // Kill any ongoing animation sequence
        activeSequence?.Kill();

        // Update text
        itemNameText.text = $"{item.itemName}";

        // Play SFX when item is added to inventory
        if (defaultNotificationSfx != null)
        {
            AudioManager.Instance?.PlaySFX(defaultNotificationSfx);
        }

        // Start new animation sequence
        notificationPanel.gameObject.SetActive(true);
        
        activeSequence = DOTween.Sequence();
        
        // Slide down
        activeSequence.Append(notificationPanel.DOAnchorPosY(visibleY, slideDuration).SetEase(Ease.OutBack));
        
        // Wait
        activeSequence.AppendInterval(displayDuration);
        
        // Slide up
        activeSequence.Append(notificationPanel.DOAnchorPosY(hiddenY, slideDuration).SetEase(Ease.InQuad));
        
        // Hide GameObject on complete
        activeSequence.OnComplete(() => {
            notificationPanel.gameObject.SetActive(false);
        });
    }
}
