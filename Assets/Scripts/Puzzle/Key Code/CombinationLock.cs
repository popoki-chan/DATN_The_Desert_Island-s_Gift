using UnityEngine;

public class CombinationLock : MonoBehaviour
{
    [Header("Các dải số")]
    public NumberWheel2D[] wheels;

    [Header("Mật mã đúng")]
    public string correctCode = "198";

    [Header("Liên kết Rương chính (View Cận cảnh)")]
    public Interactable targetChest;

    [Header("Đổi ảnh rương ở View Cận cảnh")]
    public GameObject chestClosedVisual;
    public GameObject chestOpenVisual;

    [Header("Đồng bộ View Main")]
    [Tooltip("Kéo Rương đang đóng ở View Main Room vào đây")]
    public GameObject mainViewChestClosed;
    [Tooltip("Kéo Rương mở ở View Main Room vào đây (Nhớ tắt tàng hình nó đi nhé)")]
    public GameObject mainViewChestOpen;

    [Header("View Bên Trong Rương")]
    public GameObject viewInChest;

    private bool isUnlocked = false;

    public void CheckCode()
    {
        if (isUnlocked) return;
        if (wheels == null || wheels.Length < correctCode.Length) return;

        for (int i = 0; i < correctCode.Length; i++)
        {
            if (wheels[i].currentNumber.ToString() != correctCode[i].ToString())
            {
                return;
            }
        }
        Unlock();
    }

    private void Unlock()
    {
        isUnlocked = true;
        Debug.Log("<color=green>[CombinationLock]</color> Mật mã ĐÚNG! Đồng bộ rương toàn cục.");

        // 1. Đổi ảnh rương Cận Cảnh
        if (chestClosedVisual != null) chestClosedVisual.SetActive(false);
        if (chestOpenVisual != null) chestOpenVisual.SetActive(true);

        // 1.1 ĐỒNG BỘ: Đổi ảnh rương Ngoài Bãi Biển
        if (mainViewChestClosed != null) mainViewChestClosed.SetActive(false);
        if (mainViewChestOpen != null) mainViewChestOpen.SetActive(true);

        // 2. Mở khóa logic & tráo đường link Zoom
        if (targetChest != null)
        {
            targetChest.isLocked = false;
            targetChest.requiredItemId = "";

            if (viewInChest != null)
            {
                targetChest.targetView = viewInChest;
            }
        }

        // 3. Tự lùi camera ra ngoài sau 0.8s
        Invoke(nameof(ExitZoom), 0.8f);
    }

    private void ExitZoom()
    {
        if (ViewManager.Instance != null)
        {
            ViewManager.Instance.GoBack();
        }
    }
}