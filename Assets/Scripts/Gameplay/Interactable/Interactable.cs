using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class Interactable : MonoBehaviour
{
    [Header("Thông tin cơ bản")]
    public string id;
    [TextArea] public string description;

    [Header("Trạng thái")]
    public bool isPickable = false;
    public bool isLocked = false;
    public string requiredItemId;

    [Header("Âm thanh")]
    public AudioClip onClickSfx;
    public AudioClip pickupSfx;
    public AudioClip failUseSfx;

    [Header("Chuyển cảnh Zoom In")]
    public bool isZoomable = false;
    public GameObject targetView;

    [Header("Đồng bộ trạng thái (Sync Target)")]
    [Tooltip("GameObject ở view khác cần đồng bộ ẩn/phá hủy theo object này")]
    public GameObject syncTarget;

    public event Action<Interactable> OnClicked; 
    public event Action OnDefaultInteract;      

    public void RaiseClicked()
    {
        OnClicked?.Invoke(this);
    }

    void OnMouseDown()
    {

        if (SettingsPopupController.IsOpen) return;
        if (IsPointerOverUI()) return;
        if (Camera.main != null)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject != gameObject && !hit.collider.transform.IsChildOf(transform))
            {
                return;
            }
        }

        OnClicked?.Invoke(this);

        if (isPickable)
        {
            Pickup();
            return;
        }

        if (isLocked)
        {
            if (Inventory.Instance != null)
            {
                var sel = Inventory.Instance.currentSelectedItem;
                bool success = Inventory.Instance.TryUseOn(sel, this);
                if (!success)
                {
                    if (sel == null)
                    {
                        // Khi click tay không vào vật thể bị khóa, phát âm thanh click bình thường để nhận biết phản hồi
                        AudioClip clipToPlay = onClickSfx != null ? onClickSfx : failUseSfx;
                        if (clipToPlay != null)
                        {
                            AudioManager.Instance?.PlaySFX(clipToPlay);
                        }
                    }
                    // Nếu dùng sai vật phẩm (sel != null), Inventory.TryUseOn đã tự phát failUseSfx nên ta không phát thêm ở đây

                    if (TryGetComponent<PopupBubble>(out var bubble))
                    {
                        bubble.PlayAnimation();
                    }
                }
            }
            return;
        }

        if (isZoomable && targetView != null)
        {
            AudioManager.Instance?.PlaySFX(onClickSfx);
            ViewManager.Instance?.ChangeView(targetView);
            return;
        }

        Interact();
    }

    public virtual void Interact()
    {
        AudioManager.Instance?.PlaySFX(onClickSfx);
        OnDefaultInteract?.Invoke();
    }

    public virtual void Pickup()
    {
        if (!isPickable) return;

        Item asset = Resources.Load<Item>($"Items/{id}");
        if (asset == null) return;

        var clipToPlay = pickupSfx != null ? pickupSfx : onClickSfx;
        if (clipToPlay != null)
        {
            AudioManager.Instance?.PlaySFX(clipToPlay);
        }

        if (TryGetComponent<InteractableAnimation>(out var feedback))
        {
            feedback.PlayPickupAnimation(() => {
                Inventory.Instance?.AddItem(asset);
                if (syncTarget != null) Destroy(syncTarget);
                Destroy(gameObject);
            });
        }
        else
        {
            Inventory.Instance?.AddItem(asset);
            if (syncTarget != null) Destroy(syncTarget);
            Destroy(gameObject);
        }
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        if (EventSystem.current.IsPointerOverGameObject()) return true;
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                return true;
        }
        return false;
    }

    protected virtual void OnDestroy()
    {
        if (syncTarget != null && syncTarget.gameObject != null)
        {
            Destroy(syncTarget);
        }
    }
}