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
        Debug.Log($"[Inventory] Selected item: {(item == null ? "null" : item.itemId)}");
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
    public bool TryUseOn(Item item, Interactable target)
    {
        Debug.Log($"[TryUseOn] Enter target:{(target == null ? "null" : target.id)} item:{(item == null ? "null" : item.itemId)}");

        if (target == null) return false;

        if (item == null)
        {
            Debug.Log("[TryUseOn] No item selected");
            TooltipUI.Instance?.Show("Bạn đang tay không...");
            return false;
        }

        Debug.Log($"[TryUseOn] target.requiredItemId = {target.requiredItemId}");

        if (!string.IsNullOrEmpty(target.requiredItemId))
        {
            string selectedId = item.itemId;
            Debug.Log($"[TryUseOn] comparing selectedId:{selectedId} with required:{target.requiredItemId}");
            if (selectedId == target.requiredItemId)
            {
                Debug.Log("[TryUseOn] Match! executing success flow");
                // gọi Interact trước khi remove nếu Interact cần item
                target.isLocked = false;
                PuzzleManager.Instance?.SetState(target.id + "_used", true);
                AudioManager.Instance?.PlaySFX(target.onClickSfx);
                target.Interact();
                ConsumeSelectedItem();
                return true;
            }
            else
            {
                Debug.Log("[TryUseOn] Item does not match required");
                TooltipUI.Instance?.Show("Không đúng chìa khóa rồi...");
                return false;
            }
        }

        if (PuzzleManager.Instance != null)
        {
            bool solved = PuzzleManager.Instance.TrySolveCombination(target.id, item.itemId);
            Debug.Log("[TryUseOn] TrySolveCombination returned: " + solved);
            return solved;
        }

        Debug.Log("[TryUseOn] No requiredItemId and no PuzzleManager");
        return false;
    }


    // Helper lấy id từ Item, sửa nếu Item dùng tên khác
    private string GetItemId(Item item)
    {
        if (item == null) return null;
        return item.itemId; // nếu class Item của bạn dùng tên khác, đổi ở đây
    }
}