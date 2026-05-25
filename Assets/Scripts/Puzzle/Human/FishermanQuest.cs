using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class FishermanQuest : MonoBehaviour
{
    [Header("1. Yêu cầu vật phẩm")]
    [Tooltip("ID của Cành cây nhọn (VD: sharp_bough)")]
    public string spearItemId = "sharp_bough";

    [Header("2. Bật/Tắt Hình ảnh (GameObjects)")]
    public GameObject humanIdleVisual;       // Hình người đứng không
    public GameObject humanWithSpearVisual;  // Hình người cầm lao

    [Header("3. Khu vực Bắt cá")]
    [Tooltip("Kéo Object vùng click để mở View Bắt Cá (đang bị tàng hình) vào đây")]
    public GameObject fishingInteractArea;

    private Interactable coreLogic;

    void Awake()
    {
        coreLogic = GetComponent<Interactable>();
    }

    // --- HÀM NÀY SẼ ĐƯỢC GỌI KHI BƯỚC CHẾ TẠO TRƯỚC ĐÓ HOÀN THÀNH ---
    public void StartFishingPhase()
    {
        // 1. Reset đồ họa: Bật hình ban đầu, tắt các hình khác
        if (humanIdleVisual != null) humanIdleVisual.SetActive(true);
        if (humanWithSpearVisual != null) humanWithSpearVisual.SetActive(false);

        // Đảm bảo khu bắt cá vẫn đang bị khóa (tàng hình)
        if (fishingInteractArea != null) fishingInteractArea.SetActive(false);

        // 2. Bắt đầu đòi người chơi đưa Cành cây nhọn
        coreLogic.requiredItemId = spearItemId;
        coreLogic.isLocked = true;

        // 3. Lắng nghe cú click chuột tiếp theo
        coreLogic.OnDefaultInteract += ReceiveSpear;
    }

    // --- HÀM NÀY CHẠY KHI NGƯỜI CHƠI ĐƯA CÀNH CÂY NHỌN CHO HUMAN ---
    private void ReceiveSpear()
    {
        // 1. Cập nhật đồ họa: Đổi sang hình đang cầm cành cây nhọn
        if (humanIdleVisual != null) humanIdleVisual.SetActive(false);
        if (humanWithSpearVisual != null) humanWithSpearVisual.SetActive(true);

        // 2. MỞ KHÓA VIEW BẮT CÁ (Bật object click lên)
        if (fishingInteractArea != null) fishingInteractArea.SetActive(true);

        // 3. Hoàn thành nhiệm vụ của Human (Không đòi gì nữa, khóa nhân vật lại)
        coreLogic.requiredItemId = "";
        coreLogic.isLocked = false;
        coreLogic.OnDefaultInteract -= ReceiveSpear; // Ngắt lắng nghe để tránh lỗi

        Debug.Log("<color=green>[Fisherman]</color> Đã nhận vũ khí! Mở khóa khu vực bắt cá thành công.");
    }
}