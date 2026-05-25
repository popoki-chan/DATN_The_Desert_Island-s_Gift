using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class ViewTrigger : MonoBehaviour
{
    [Header("Cài đặt Chuyển View")]
    [Tooltip("Kéo cái View Bắt Cá (Cận cảnh) vào đây")]
    public GameObject targetView;

    private Interactable coreLogic;

    void Awake()
    {
        coreLogic = GetComponent<Interactable>();
    }

    void OnEnable()
    {
        if (coreLogic != null) coreLogic.OnDefaultInteract += TriggerChange;
    }

    void OnDisable()
    {
        if (coreLogic != null) coreLogic.OnDefaultInteract -= TriggerChange;
    }

    private void TriggerChange()
    {
        if (targetView != null && ViewManager.Instance != null)
        {
            // GỌI VIEW MANAGER ĐỂ CHUYỂN CẢNH ĐÚNG CÁCH
            // Nó sẽ tự động tắt View Main hiện tại và hiện View Bắt Cá lên
            ViewManager.Instance.ChangeView(targetView);

            Debug.Log($"<color=cyan>[ViewTrigger]</color> Đang chuyển sang: {targetView.name}");
        }
    }
}