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
}

[RequireComponent(typeof(Interactable))]
public class MultiStepPuzzle : MonoBehaviour
{
    [Header("1. Danh sách các bước giải đố")]
    public PuzzleStep[] steps;

    [Header("2. Sự kiện khi Hoàn thành (Nối dây tùy ý)")]
    [Tooltip("Kéo các hàm rớt đồ, bật âm thanh, mở cửa... vào đây")]
    public UnityEvent onPuzzleCompleted; // CÁI LOA THÔNG BÁO

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

        if (currentStep.visualNow != null) currentStep.visualNow.SetActive(true);
        if (currentStep.visualOther != null) currentStep.visualOther.SetActive(true);
        if (currentStep.hideVisualNow != null) currentStep.hideVisualNow.SetActive(false);
        if (currentStep.hideVisualOther != null) currentStep.hideVisualOther.SetActive(false);

        currentStepIndex++;

        if (currentStepIndex < steps.Length)
        {
            coreLogic.requiredItemId = steps[currentStepIndex].requiredItemId;
            coreLogic.isLocked = true;
        }
        else
        {
            coreLogic.requiredItemId = "";
            coreLogic.isLocked = false;

            // --- HÉT LÊN QUA LOA LÀ ĐÃ XONG! ---
            onPuzzleCompleted?.Invoke();

            this.enabled = false;
        }
    }
}