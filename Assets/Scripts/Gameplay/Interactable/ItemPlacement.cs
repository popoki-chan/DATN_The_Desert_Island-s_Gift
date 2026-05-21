using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class ItemPlacement : MonoBehaviour
{
    [Header("Vật thể đặt View hiện tại")]
    public GameObject visualToActivate;

    [Header("Đồng bộ View khác")]
    [Tooltip("Kéo cái Xơ dừa ở View Main (hoặc các view khác) vào đây để nó cùng hiện lên")]
    public GameObject[] syncedVisuals;

    [Header("Cài đặt sau khi đặt")]
    [Tooltip("Khóa luôn vùng kích hoạt này, không cho bấm vào nữa")]
    public bool disableInteractionAfterPlace = true;

    private Interactable coreLogic;

    void Awake()
    {
        coreLogic = GetComponent<Interactable>();
    }

    void OnEnable()
    {
        if (coreLogic != null) coreLogic.OnDefaultInteract += ActivateObject;
    }

    void OnDisable()
    {
        if (coreLogic != null) coreLogic.OnDefaultInteract -= ActivateObject;
    }

    private void ActivateObject()
    {
        // 1. Bật vật thể Xơ dừa ở View hiện tại (View Camp)
        if (visualToActivate != null)
        {
            visualToActivate.SetActive(true);
        }

        // 2. Tự động tìm và bật TẤT CẢ các Xơ dừa ở các View khác (View Main)
        if (syncedVisuals != null && syncedVisuals.Length > 0)
        {
            foreach (GameObject obj in syncedVisuals)
            {
                if (obj != null) obj.SetActive(true);
            }
        }

        Debug.Log($"<color=green>[ItemPlacement]</color> Đã đặt đồ! Cập nhật đồng bộ trên tất cả các View.");

        // 3. Tắt vùng tương tác
        if (disableInteractionAfterPlace)
        {
            if (TryGetComponent<Collider2D>(out var col))
            {
                col.enabled = false;
            }
            this.enabled = false;
        }
    }
}