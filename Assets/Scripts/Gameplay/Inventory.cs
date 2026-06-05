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
        if (currentSelectedItem == item) return;
        currentSelectedItem = item;
        OnItemSelected?.Invoke(item);
    }

    public void ConsumeSelectedItem()
    {
        if (currentSelectedItem != null)
        {
            Item itemToConsume = currentSelectedItem;
            if (InventoryUI.Instance != null) InventoryUI.Instance.Deselect();

            currentSelectedItem = null;
            if (itemDurability.ContainsKey(itemToConsume))
            {
                itemDurability[itemToConsume]--;
                if (itemDurability[itemToConsume] <= 0)
                {
                    RemoveItem(itemToConsume);
                }
            }
            else
            {
                RemoveItem(itemToConsume);
            }
        }
    }

    public void AddItem(Item item, bool notify = true, int customDurability = -1)
    {
        if (item == null) return;
        items.Add(item);
        int usesToSet = (customDurability > 0) ? customDurability : item.maxUses;

        if (!itemDurability.ContainsKey(item))
        {
            itemDurability.Add(item, usesToSet);
        }
        else
        {
            itemDurability[item] = usesToSet;
        }

        if (notify) OnItemAdded?.Invoke(item);
        OnInventoryChanged?.Invoke();
    }
    public void RemoveItem(Item item)
    {
        if (items.Remove(item))
        {
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
        itemDurability.Clear();
        OnInventoryChanged?.Invoke();
    }

    public bool TryUseOn(Item item, Interactable target)
    {
        if (target == null) return false;

        if (item == null)
        {
            TooltipUI.Instance?.Show("Bạn đang tay không...");
            return false;
        }

        if (!string.IsNullOrEmpty(target.requiredItemId))
        {
            string selectedId = item.itemId;
            if (selectedId == target.requiredItemId)
            {
                target.isLocked = false;
                PuzzleManager.Instance?.SetState(target.id + "_used", true);
                target.Interact();
                ConsumeSelectedItem();
                return true;
            }
            else
            {
                TooltipUI.Instance?.Show("Không đúng chìa khóa rồi...");
                if (target.failUseSfx != null)
                {
                    AudioManager.Instance?.PlaySFX(target.failUseSfx);
                }
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

    private string GetItemId(Item item)
    {
        if (item == null) return null;
        return item.itemId;
    }
}