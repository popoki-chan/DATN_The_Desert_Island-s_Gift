using UnityEngine;

// Tạo ra một khuôn mẫu cho "1 Bước giải đố"
[System.Serializable]
public class PuzzleStep
{
    [Header("Yêu cầu của bước này")]
    [Tooltip("ID vật phẩm cần để qua bước này (VD: coir, lens...)")]
    public string requiredItemId;

    [Header("Vật thể hiện ra (Tùy chọn)")]
    [Tooltip("Vật thể hiển thị ở View Cận Cảnh (Gần)")]
    public GameObject visualNear;

    [Tooltip("Vật thể hiển thị ở View Ngoài (Đồng bộ)")]
    public GameObject visualFar;
}

[RequireComponent(typeof(Interactable))]
public class MultiStepPuzzle : MonoBehaviour
{
    [Header("Danh sách các bước giải đố (Bấm + để thêm bước)")]
    public PuzzleStep[] steps; // Mảng chứa N bước giải đố

    private Interactable coreLogic;
    private int currentStepIndex = 0;

    void Awake()
    {
        coreLogic = GetComponent<Interactable>();

        // Khởi tạo yêu cầu cho bước đầu tiên ngay khi game bắt đầu
        if (steps != null && steps.Length > 0)
        {
            coreLogic.isLocked = true;
            coreLogic.requiredItemId = steps[0].requiredItemId;
        }
    }

    void OnEnable()
    {
        if (coreLogic != null) coreLogic.OnDefaultInteract += HandleStep;
    }

    void OnDisable()
    {
        if (coreLogic != null) coreLogic.OnDefaultInteract -= HandleStep;
    }

    private void HandleStep()
    {
        // Tránh lỗi nếu chưa setup bước nào
        if (steps == null || currentStepIndex >= steps.Length) return;

        // 1. Kích hoạt hình ảnh của bước hiện tại vừa hoàn thành
        PuzzleStep currentStep = steps[currentStepIndex];
        if (currentStep.visualNear != null) currentStep.visualNear.SetActive(true);
        if (currentStep.visualFar != null) currentStep.visualFar.SetActive(true);

        // 2. Tăng chỉ số bước lên
        currentStepIndex++;

        // 3. Cập nhật Logic cho bước TIẾP THEO (nếu còn)
        if (currentStepIndex < steps.Length)
        {
            // Vẫn còn bước -> Cập nhật yêu cầu vật phẩm mới và tiếp tục khóa
            coreLogic.requiredItemId = steps[currentStepIndex].requiredItemId;
            coreLogic.isLocked = true;
            Debug.Log($"<color=yellow>[MultiStepPuzzle]</color> Xong bước {currentStepIndex}. Chuyển sang đòi: {steps[currentStepIndex].requiredItemId}");
        }
        else
        {
            // Đã đi đến bước cuối cùng -> Mở khóa toàn cục rương/cửa/đồ vật
            coreLogic.requiredItemId = "";
            coreLogic.isLocked = false;
            Debug.Log($"<color=green>[MultiStepPuzzle]</color> Đã hoàn thành chuỗi giải đố!");

            // Tắt script này đi vì đã xong nhiệm vụ
            this.enabled = false;
        }
    }
}