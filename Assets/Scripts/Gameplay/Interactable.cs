using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Interactable : MonoBehaviour
{
    [Header("Thông tin cơ bản")]
    public string id;
    [TextArea] public string description;

    [Header("Trạng thái")]
    public bool isPickable = false;
    public bool isLocked = false;
    public string requiredItemId; // optional: item id required to use

    [Header("Âm thanh")]
    public AudioClip onClickSfx;

    [Header("Chuyển cảnh (Zoom In)")]
    public bool isZoomable = false;
    public GameObject targetView; // Góc nhìn (View) sẽ hiện ra khi click vào

    // Sự kiện để các Manager khác (như PuzzleManager) lắng nghe
    public event Action<Interactable> OnClicked;



    public void RaiseClicked()
    {
        OnClicked?.Invoke(this);
    }

    void OnMouseEnter()
    {
        TooltipUI.Instance?.Show(description);
    }

    void OnMouseExit()
    {
        TooltipUI.Instance?.Hide();
    }

    void OnMouseDown()
    {
        // 1. Phát sự kiện cho các hệ thống khác biết vật thể này vừa bị click
        OnClicked?.Invoke(this);

        // 2. Phân nhánh hành động: Nhặt, Mở Khóa, hoặc Tương tác bình thường
        if (isPickable)
        {
            Pickup();
        }
        else if (isLocked)
        {
            // Nhờ Inventory kiểm tra xem trên tay có đang cầm đúng đồ không.
            // Hàm TryUseOn bên Inventory sẽ tự động lo việc trừ đồ, mở khóa và báo lỗi!
            if (Inventory.Instance != null)
            {
                Inventory.Instance.TryUseOn(Inventory.Instance.currentSelectedItem, this);
            }
        }
        else if (isZoomable && targetView != null)
        {
            AudioManager.Instance?.PlaySFX(onClickSfx); // them sound effect
            ViewManager.Instance.ChangeView(targetView); // Gọi ViewManager chuyển cảnh
        }
        else
        {
            Interact();
        }
    }

    public virtual void Interact()
    {
        Debug.Log($"[Interactable] Đang tương tác với: {id}");
        AudioManager.Instance?.PlaySFX(onClickSfx);
    }

    public virtual void Pickup()
    {
        if (!isPickable) return;

        // Load Item asset từ thư mục Assets/Resources/Items/
        Item asset = Resources.Load<Item>($"Items/{id}");

        if (asset != null)
        {
            Inventory.Instance.AddItem(asset);
            AudioManager.Instance?.PlaySFX(onClickSfx); // Thêm tiếng nhặt đồ
            Destroy(gameObject); // Xóa khỏi màn hình
        }
        else
        {
            Debug.LogWarning($"[Lỗi] Không tìm thấy Item asset có id là '{id}' trong thư mục Resources/Items/");
        }
    }
}