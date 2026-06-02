using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : Singleton<Inventory>
{
    public List<Item> items = new List<Item>();

    public event Action<Item> OnItemAdded;
    public event Action<Item> OnItemRemoved;
    public event Action OnInventoryChanged;
    public event Action<Item> OnItemSelected;

    private Dictionary<Item, int> itemDurability = new Dictionary<Item, int>();
    public Item currentSelectedItem { get; private set; }

    protected override void Awake()
    {
        DontDestroyOnLoadEnabled = false;
        base.Awake();
    }

    public void SelectItem(Item item)
    {
        currentSelectedItem = item;
        Debug.Log($"[Inventory] Selected item: {(item == null ? "null" : item.itemId)}");
        OnItemSelected?.Invoke(item);
    }

    // --- ĐÃ CẬP NHẬT LOGIC ĐỘ BỀN ---
    public void ConsumeSelectedItem()
    {
        if (currentSelectedItem != null)
        {
            Item itemToConsume = currentSelectedItem;
            currentSelectedItem = null; // Bỏ đồ đang cầm trên tay ra

            // Xóa viền vàng highlight trên UI
            if (InventoryUI.Instance != null) InventoryUI.Instance.Deselect();

            // Kiểm tra và trừ độ bền
            if (itemDurability.ContainsKey(itemToConsume))
            {
                itemDurability[itemToConsume]--; // Trừ 1 lần dùng
                Debug.Log($"[Inventory] Đã dùng {itemToConsume.itemId}. Lượt còn lại: {itemDurability[itemToConsume]}");

                // Nếu hết độ bền -> Xóa hẳn khỏi túi đồ
                if (itemDurability[itemToConsume] <= 0)
                {
                    RemoveItem(itemToConsume);
                    Debug.Log($"<color=red>[Inventory] Vật phẩm {itemToConsume.itemId} đã hỏng/biến mất!</color>");
                }
            }
            else
            {
                // Dự phòng: Nếu item không có trong sổ (do lỗi cũ), xóa luôn theo logic gốc
                RemoveItem(itemToConsume);
            }
        }
    }

    // --- ĐÃ CẬP NHẬT THÊM THAM SỐ GHI ĐÈ ĐỘ BỀN ---
    public void AddItem(Item item, bool notify = true, int customDurability = -1)
    {
        if (item == null) return;
        items.Add(item);

        // Ghi vào sổ: Xác định số lần dùng
        // (Nếu customDurability > 0 thì lấy số custom, không thì lấy mặc định trong file Item)
        int usesToSet = (customDurability > 0) ? customDurability : item.maxUses;

        if (!itemDurability.ContainsKey(item))
        {
            itemDurability.Add(item, usesToSet);
        }
        else
        {
            itemDurability[item] = usesToSet; // Reset lại độ bền nếu nhặt lại đồ đã đánh rơi
        }

        if (notify) OnItemAdded?.Invoke(item);
        OnInventoryChanged?.Invoke();
    }

    // --- ĐÃ CẬP NHẬT DỌN SỔ ---
    public void RemoveItem(Item item)
    {
        if (items.Remove(item))
        {
            // Dọn dẹp quyển sổ để tránh rác bộ nhớ
            if (itemDurability.ContainsKey(item))
            {
                itemDurability.Remove(item);
            }

            OnItemRemoved?.Invoke(item);
            OnInventoryChanged?.Invoke();
        }
    }

    public bool HasItem(string id) => items.Exists(i => i.itemId == id);
    public Item GetItemById(string id) => items.Find(i => i.itemId == id);

    public void ClearAll()
    {
        items.Clear();
        itemDurability.Clear(); // Dọn sạch quyển sổ
        OnInventoryChanged?.Invoke();
    }

    // --- HỆ THỐNG SỬ DỤNG ĐỒ (Giữ nguyên logic cực chuẩn của bạn) ---
    public bool TryUseOn(Item item, Interactable target)
    {
        Debug.Log($"[TryUseOn] Enter target:{(target == null ? "null" : target.id)} item:{(item == null ? "null" : item.itemId)}");

        if (target == null) return false;

        if (item == null)
        {
            TooltipUI.Instance?.Show("Bạn đang tay không...");
            return false;
        }

        if (!string.IsNullOrEmpty(target.requiredItemId))
        {
            string selectedId = item.itemId;
            Debug.Log($"[TryUseOn] comparing selectedId:{selectedId} with required:{target.requiredItemId}");
            if (selectedId == target.requiredItemId)
            {
                Debug.Log("[TryUseOn] Match! executing success flow");
                target.isLocked = false;
                PuzzleManager.Instance?.SetState(target.id + "_used", true);
                AudioManager.Instance?.PlaySFX(target.onClickSfx);
                target.Interact();

                // Hàm này sẽ tự động lo việc kiểm tra độ bền và xóa (hoặc giữ lại) vật phẩm!
                ConsumeSelectedItem();
                return true;
            }
            else
            {
                TooltipUI.Instance?.Show("Không đúng chìa khóa rồi...");
                return false;
            }
        }

        if (PuzzleManager.Instance != null)
        {
            bool solved = PuzzleManager.Instance.TrySolveCombination(target.id, item.itemId);
            return solved;
        }

        return false;
    }

    // Helper lấy id từ Item, sửa nếu Item dùng tên khác
    private string GetItemId(Item item)
    {
        if (item == null) return null;
        return item.itemId;
    }
}