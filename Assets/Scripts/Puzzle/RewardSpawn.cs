using UnityEngine;

public class RewardSpawn : MonoBehaviour
{
    [Header("Vật phẩm thưởng")]
    [Tooltip("Kéo file dữ liệu Item (VD: sharp_bough) muốn tự động cho vào túi đồ vào đây")]
    public Item rewardItem;

    // Hàm này sẽ được gọi từ cái loa UnityEvent của MultiStepPuzzle
    public void GiveRewardToInventory()
    {
        if (rewardItem != null && Inventory.Instance != null)
        {
            // Gọi thẳng hệ thống túi đồ để thêm vật phẩm
            Inventory.Instance.AddItem(rewardItem);
            Debug.Log($"<color=green>[RewardGiver]</color> Đã thêm thành công {rewardItem.itemId} vào thẳng túi đồ!");
        }
        else
        {
            Debug.LogWarning("[RewardGiver] Thiếu file Item hoặc không tìm thấy Inventory Instance!");
        }
    }
}