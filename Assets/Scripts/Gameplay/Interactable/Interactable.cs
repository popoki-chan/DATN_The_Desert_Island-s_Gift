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

    [Header("Chuyển cảnh Zoom In")]
    public bool isZoomable = false;
    public GameObject targetView;

    // --- SỰ KIỆN CHO CÁC HỆ THỐNG KHÁC LẮNG NGHE ---
    public event Action<Interactable> OnClicked; // Trả lại cho PlayerCursor
    public event Action OnDefaultInteract;       // Dành cho InteractableAnimation (DOTween)

    public void RaiseClicked()
    {
        OnClicked?.Invoke(this);
    }

    void OnMouseDown()
    {
        // --- THÊM KHIÊN CHỐNG XUYÊN CLICK VÀO NGAY ĐÂY ---
        // Nếu chuột đang nằm trên UI (như nút bấm, ảnh nền UI), thì cấm không cho chạy tiếp code bên dưới!
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // Báo hiệu click chuột
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
                Inventory.Instance.TryUseOn(sel, this);
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
        OnDefaultInteract?.Invoke(); // Gọi DOTween
    }

    public virtual void Pickup()
    {
        if (!isPickable) return;

        Item asset = Resources.Load<Item>($"Items/{id}");
        if (asset == null) return;

        if (TryGetComponent<InteractableAnimation>(out var feedback))
        {
            feedback.PlayPickupAnimation(() => {
                Inventory.Instance?.AddItem(asset);
                Destroy(gameObject);
            });
        }
        else
        {
            Inventory.Instance?.AddItem(asset);
            Destroy(gameObject);
        }
    }
}