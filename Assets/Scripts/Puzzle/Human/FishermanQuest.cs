using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class FishermanQuest : MonoBehaviour
{
    [Header("1. Yêu cầu vật phẩm")]
    public string spearItemId = "sharp_bough";

    [Header("2. Bật/Tắt Hình ảnh Nhân vật (Góc nhìn cũ)")]
    public GameObject humanIdleVisual;
    public GameObject humanWithSpearVisual;

    [Header("3. Chuyển thẳng sang View Bắt Cá")]
    public GameObject fishingView;

    private Interactable coreLogic;
    private bool isQuestCompleted = false; // BÚA TẠ KHÓA LLogic: Đã hoàn thành bắt cá hoàn toàn chưa?

    void Awake()
    {
        coreLogic = GetComponent<Interactable>();
    }

    void Start()
    {
        if (Inventory.Instance != null && Inventory.Instance.HasItem("fish"))
        {
            CompleteQuestAndLockPermanently();
        }
    }

    public void StartFishingPhase()
    {
        isQuestCompleted = false; // Reset trạng thái khi bắt đầu pha đòi lao

        if (humanIdleVisual != null) humanIdleVisual.SetActive(true);
        if (humanWithSpearVisual != null) humanWithSpearVisual.SetActive(false);

        coreLogic.requiredItemId = spearItemId;
        coreLogic.isLocked = true;

        coreLogic.OnDefaultInteract += ReceiveSpearAndSwitchView;
    }

    private void ReceiveSpearAndSwitchView()
    {
        if (humanIdleVisual != null) humanIdleVisual.SetActive(false);
        if (humanWithSpearVisual != null) humanWithSpearVisual.SetActive(true);

        SwitchToFishingView();

        coreLogic.requiredItemId = "";
        coreLogic.isLocked = false;

        coreLogic.OnDefaultInteract -= ReceiveSpearAndSwitchView;
        coreLogic.OnDefaultInteract += ReEnterFishingView;

        Debug.Log("<color=green>[FishermanQuest]</color> Đã nhận lao! Mở cổng vào View bắt cá.");
    }

    private void ReEnterFishingView()
    {
        // BIỆN PHÁP CHẶN ĐỨNG: Nếu đã bắt được cá rồi, chặn đứng không cho chuyển view nữa!
        if (isQuestCompleted)
        {
            Debug.Log("[FishermanQuest] Nhiệm vụ đã hoàn thành vĩnh viễn. Chặn click vào lại.");
            return;
        }

        Debug.Log("<color=cyan>[FishermanQuest]</color> Vào lại View câu cá do chưa bắt được cá.");
        SwitchToFishingView();
    }

    private void SwitchToFishingView()
    {
        if (fishingView != null && ViewManager.Instance != null)
        {
            ViewManager.Instance.ChangeView(fishingView);
        }
    }

    // --- HÀM CHỐT HẠ: ĐƯỢC GỌI KHI BẮT ĐƯỢC CÁ ---
    public void CompleteQuestAndLockPermanently()
    {
        isQuestCompleted = true; // Kích hoạt khóa vĩnh viễn

        if (coreLogic != null)
        {
            coreLogic.requiredItemId = "";
            coreLogic.isLocked = true; // Khóa cứng Interactable gốc của Human lại luôn

            // Hủy đăng ký tất cả các hàm tương tác để ông này hoàn toàn trơ ra
            coreLogic.OnDefaultInteract -= ReceiveSpearAndSwitchView;
            coreLogic.OnDefaultInteract -= ReEnterFishingView;
        }

        // Tắt bong bóng popup của Human
        var popup = GetComponent<PopupBubble>();
        if (popup != null)
        {
            popup.Hide();
            popup.enabled = false;
        }

        Debug.Log("<color=red>[FishermanQuest]</color> CHỐT HẠ: Đã bắt được cá, Human bị khóa tương tác mãi mãi!");
    }
}