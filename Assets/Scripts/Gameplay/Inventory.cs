using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }
    public List<Item> items = new List<Item>();

    public event Action<Item> OnItemAdded;
    public event Action<Item> OnItemRemoved;
    public event Action OnInventoryChanged;
    public event Action<Item> OnItemSelected;

    // Lưu giữ vật phẩm đang sử dụng
    public Item currentSelectedItem { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SelectItem(Item item)
    {
        currentSelectedItem = item;
        OnItemSelected?.Invoke(item);
    }

 
    public void ConsumeSelectedItem()
    {
        if (currentSelectedItem != null)
        {
            Item itemToConsume = currentSelectedItem;
            currentSelectedItem = null; // Xóa data đang cầm trên tay

            // Xóa viền vàng highlight trên UI
            if (InventoryUI.Instance != null) InventoryUI.Instance.Deselect();

            // Xóa khỏi túi đồ
            RemoveItem(itemToConsume);
        }
    }

    public void AddItem(Item item, bool notify = true)
    {
        if (item == null) return;
        items.Add(item);
        if (notify) OnItemAdded?.Invoke(item);
        OnInventoryChanged?.Invoke();
    }

    public void RemoveItem(Item item)
    {
        if (items.Remove(item))
        {
            OnItemRemoved?.Invoke(item);
            OnInventoryChanged?.Invoke();
        }
    }

    public bool HasItem(string id) => items.Exists(i => i.itemId == id);
    public Item GetItemById(string id) => items.Find(i => i.itemId == id);

    public void ClearAll()
    {
        items.Clear();
        OnInventoryChanged?.Invoke();
    }

    // --- HỆ THỐNG SỬ DỤNG ĐỒ ---
    public void TryUseOn(Item item, Interactable target)
    {
        if (target == null) return;

        // Nếu người chơi KHÔNG cầm gì trên tay nhưng click vào vật bị khóa
        if (item == null)
        {
            Debug.Log($"[Khóa] {target.id} cần có {target.requiredItemId} để mở, nhưng bạn đang tay không!");
            TooltipUI.Instance?.Show("Nó bị khóa rồi...");
            // AudioManager.Instance?.PlaySFX(lockedSfx);
            return;
        }

        // Nếu vật thể yêu cầu một item cụ thể (isLocked = true)
        if (!string.IsNullOrEmpty(target.requiredItemId))
        {
            if (item.itemId == target.requiredItemId)
            {
                // THÀNH CÔNG
                Debug.Log($"Đã dùng {item.itemId} lên {target.id} thành công.");

                // Mở khóa
                target.isLocked = false;

                // Kích hoạt Puzzle Manager nếu có
                PuzzleManager.Instance?.SetState(target.id + "_used", true);

                // Phát âm thanh
                AudioManager.Instance?.PlaySFX(target.onClickSfx);

                // Gọi hàm mới để trừ đồ và dọn dẹp UI
                ConsumeSelectedItem();

                // (Tùy chọn) Gọi hàm Interact của vật thể để nó tiếp tục hiển thị đồ bên trong
                target.Interact();
            }
            else
            {
                // SAI ĐỒ
                Debug.Log("Vật phẩm này không dùng ở đây được.");
                TooltipUI.Instance?.Show("Không đúng chìa khóa rồi...");
                // AudioManager.Instance?.PlaySFX(wrongItemSfx);
            }
            return;
        }

        // Fallback: Nếu không có requiredItemId, thử ghép nối với PuzzleManager
        PuzzleManager.Instance?.TrySolveCombination(target.id, item.itemId);
    }
}