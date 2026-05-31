using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class PuzzleStep
{
    public string requiredItemId;
    public GameObject visualNow;
    public GameObject visualOther;
    public GameObject hideVisualNow;
    public GameObject hideVisualOther;

    [Header("Custom Animation Settings")]
    public bool hasCustomAnimation;
    public UnityEvent onStepActivated;
}

[RequireComponent(typeof(Interactable))]
public class MultiStepPuzzle : MonoBehaviour
{
    [Header("1. Danh sách các bước giải đố")]
    public PuzzleStep[] steps;

    [Header("2. Mở View khi hoàn thành (Tùy chọn)")]
    [Tooltip("Kéo View cần mở (VD: View khắc chữ) vào đây. Nó sẽ mở ra sau khi đưa đủ đồ ở bước cuối.")]
    public GameObject finalViewToOpen;

    [Header("3. Sự kiện khi Hoàn thành (Tùy chọn)")]
    [Tooltip("Kéo các hàm rớt đồ, bật âm thanh, mở cửa... vào đây")]
    public UnityEvent onPuzzleCompleted;
    public CutscenePlayer cutscenePlayer;

    private Interactable coreLogic;
    private int currentStepIndex = 0;

    void Awake()
    {
        coreLogic = GetComponent<Interactable>();

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
        if (steps == null || currentStepIndex >= steps.Length) return;

        PuzzleStep currentStep = steps[currentStepIndex];

        // Nếu có custom animation, gọi event và chờ controller báo hoàn tất qua CompleteCurrentStep()
        if (currentStep.hasCustomAnimation && currentStep.onStepActivated != null)
        {
            currentStep.onStepActivated.Invoke();
            return;
        }

        ExecuteStepVisuals(currentStep);
        AdvanceStep();
    }

    public void CompleteCurrentStep()
    {
        if (steps == null || currentStepIndex >= steps.Length) return;
        PuzzleStep currentStep = steps[currentStepIndex];
        ExecuteStepVisuals(currentStep);
        AdvanceStep();
    }

    private void ExecuteStepVisuals(PuzzleStep currentStep)
    {
        // 1. Cập nhật Đồ họa (Tắt/Bật Visuals)
        if (currentStep.visualNow != null) currentStep.visualNow.SetActive(true);
        if (currentStep.visualOther != null) currentStep.visualOther.SetActive(true);
        if (currentStep.hideVisualNow != null) currentStep.hideVisualNow.SetActive(false);
        if (currentStep.hideVisualOther != null) currentStep.hideVisualOther.SetActive(false);
    }

    private void AdvanceStep()
    {
        currentStepIndex++;

        // 2. Nếu vẫn còn bước tiếp theo -> Cập nhật yêu cầu item mới
        if (currentStepIndex < steps.Length)
        {
            coreLogic.requiredItemId = steps[currentStepIndex].requiredItemId;
            coreLogic.isLocked = true;
            Debug.Log($"<color=yellow>[Puzzle]</color> Đã xong bước {currentStepIndex}. Cần item tiếp: {coreLogic.requiredItemId}");
        }
        // 3. Nếu ĐÃ XONG TẤT CẢ CÁC BƯỚC!
        else
        {
            coreLogic.requiredItemId = "";
            coreLogic.isLocked = false;

            // --- TÍCH HỢP LOGIC MỞ VIEW TỪ GIVEITEM ---
            if (finalViewToOpen != null && ViewManager.Instance != null)
            {
                ViewManager.Instance.ChangeView(finalViewToOpen);

                // Cài đặt để sau này người chơi click tay không vào human vẫn mở lại được view khắc chữ
                coreLogic.isZoomable = true;
                coreLogic.targetView = finalViewToOpen;
            }

            // --- HÉT LÊN QUA LOA LÀ ĐÃ XONG! ---
            onPuzzleCompleted?.Invoke();
            if (cutscenePlayer != null)
            {
                cutscenePlayer.PlayCutscene();
            }

            Debug.Log("<color=green>[Puzzle]</color> Hoàn thành toàn bộ chuỗi giải đố!");
            this.enabled = false; // Tắt script này đi vì đã hoàn thành nhiệm vụ
        }
    }
}