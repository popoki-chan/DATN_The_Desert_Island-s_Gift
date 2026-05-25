using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Interactable))]
public class WaterScoop : MonoBehaviour
{
    [Header("1. Cài đặt Cận cảnh (View Camp Fire)")]
    [Tooltip("Kéo Object cái gáo dừa hoạt hình nằm tại vũng nước vào đây")]
    public GameObject scoopAnimationObject;

    [Tooltip("Nếu gáo dừa có Animator riêng, kéo vào đây để kích hoạt")]
    public Animator coconutAnimator;
    public string animTriggerName = "Scoop";

    [Tooltip("Thời gian chờ gáo dừa múc nước xong xuôi (bằng độ dài animation)")]
    public float scoopDuration = 1.2f;

    [Header("2. Vật phẩm nhận được")]
    [Tooltip("Kéo File Data Item (hoặc Prefab Item) của 'Gáo dừa có nước' vào đây")]
    public Item waterCoconutItem; // SỬA Ở ĐÂY: Nhận trực tiếp class Item của bạn

    private Interactable coreLogic;

    void Awake()
    {
        coreLogic = GetComponent<Interactable>();
    }

    void OnEnable()
    {
        if (coreLogic != null) coreLogic.OnDefaultInteract += StartScooping;
    }

    void OnDisable()
    {
        if (coreLogic != null) coreLogic.OnDefaultInteract -= StartScooping;
    }

    private void StartScooping()
    {
        coreLogic.isLocked = true;
        StartCoroutine(ScoopRoutine());
    }

    private IEnumerator ScoopRoutine()
    {
        // 1. CHẠY ANIMATION MÚC NƯỚC
        if (scoopAnimationObject != null) scoopAnimationObject.SetActive(true);
        if (coconutAnimator != null) coconutAnimator.SetTrigger(animTriggerName);

        Debug.Log("<color=cyan>[WaterScoop]</color> Đang múc nước...");

        // 2. CHỜ HOẠT ẢNH XONG
        yield return new WaitForSeconds(scoopDuration);

        // 3. GỌI TRỰC TIẾP HỆ THỐNG INVENTORY CỦA BẠN ĐỂ THÊM ĐỒ
        if (Inventory.Instance != null && waterCoconutItem != null)
        {
            Inventory.Instance.AddItem(waterCoconutItem);
            Debug.Log($"<color=green>[WaterScoop]</color> Đã tự động thêm {waterCoconutItem.itemId} vào túi!");
        }
        else
        {
            Debug.LogWarning("[WaterScoop] Không tìm thấy túi đồ hoặc bạn chưa kéo Water Coconut Item vào Inspector!");
        }

        // 4. DỌN DẸP
        if (scoopAnimationObject != null) scoopAnimationObject.SetActive(false);
        coreLogic.isLocked = false;
    }
}