using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class IgniteCampfire : MonoBehaviour
{
    [Header("View cận cảnh")]
    [Tooltip("Kéo object ngọn lửa ở view cận cảnh vào đây")]
    public GameObject fireVisual;
    [Tooltip("Kéo object củi khô ở view cận cảnh vào đây để ẩn đi")]
    public GameObject unlitVisual;

    [Header("View Main Room")]
    [Tooltip("Kéo object ngọn lửa ở bãi biển đằng xa vào đây (đang tàng hình)")]
    public GameObject mainViewFireVisual;
    [Tooltip("Kéo object củi khô ở bãi biển đằng xa vào đây")]
    public GameObject mainViewUnlitVisual;

    [Header("Đồng bộ Logic")]
    [Tooltip("Nếu đống củi ngoài bãi biển cũng có script Interactable, kéo nó vào đây để mở khóa luôn")]
    public Interactable mainViewInteractable;

    private Interactable coreLogic;

    void Awake()
    {
        coreLogic = GetComponent<Interactable>();
    }

    void OnEnable()
    {
        if (coreLogic != null) coreLogic.OnDefaultInteract += LightTheFire;
    }

    void OnDisable()
    {
        if (coreLogic != null) coreLogic.OnDefaultInteract -= LightTheFire;
    }

    private void LightTheFire()
    {
        // --- ĐỒNG BỘ VIEW CẬN CẢNH ---
        if (unlitVisual != null) unlitVisual.SetActive(false);
        if (fireVisual != null) fireVisual.SetActive(true);

        // --- ĐỒNG BỘ VIEW NGOÀI BÃI BIỂN ---
        if (mainViewUnlitVisual != null) mainViewUnlitVisual.SetActive(false);
        if (mainViewFireVisual != null) mainViewFireVisual.SetActive(true);

        // --- MỞ KHÓA LOGIC TOÀN CỤC ---
        // Mở khóa ở view hiện tại
        coreLogic.isLocked = false;
        coreLogic.requiredItemId = "";

        // Mở khóa ở view main (nếu có gắn script)
        if (mainViewInteractable != null)
        {
            mainViewInteractable.isLocked = false;
            mainViewInteractable.requiredItemId = "";
        }

        Debug.Log("<color=orange>[IgniteCampfire]</color> Đã nhóm lửa và đồng bộ tất cả các View!");
        this.enabled = false;
    }
}