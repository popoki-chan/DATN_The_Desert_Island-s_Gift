using System.Collections.Generic;
using UnityEngine;

public class SceneStartingItems : MonoBehaviour
{
    [Header("Danh sách vật phẩm cấp sẵn khi vào Scene")]
    public List<Item> startingItems = new List<Item>();

    void Start()
    {
        // Chờ một khung hình để đảm bảo Inventory Instance đã được khởi tạo xong
        Invoke(nameof(GiveItems), 0.1f);
    }

    void GiveItems()
    {
        if (Inventory.Instance == null) return;

        foreach (Item item in startingItems)
        {
            // Kiểm tra xem túi đồ đã có món này chưa để tránh bị nhân đôi đồ bậy bạ
            if (!Inventory.Instance.HasItem(item.itemId))
            {
                // Thêm thẳng vào túi đồ (độ bền lấy mặc định từ file ScriptableObject)
                Inventory.Instance.AddItem(item, true);
                Debug.Log($"<color=lime>[StartingItems]</color> Đã tự động thêm {item.itemId} vào túi khi vào Scene!");
            }
        }
    }
}