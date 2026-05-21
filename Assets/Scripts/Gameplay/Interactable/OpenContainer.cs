using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class OpenContainer : MonoBehaviour
{
    [Header("Chuyển đổi trạng thái (GameObject)")]
    [Tooltip("Kéo object vỏ sò đóng (clam_close) vào đây")]
    public GameObject closedStateObject;

    [Tooltip("Kéo object vỏ sò mở (clam_open) vào đây")]
    public GameObject openStateObject;

    [Header("Vật phẩm ẩn bên trong")]
    [Tooltip("Kéo thịt sò (oyster_meat) vào đây")]
    public GameObject itemToReveal;

    [Header("Cấu hình sau khi mở")]
    [Tooltip("Nếu tick, sau khi mở ra sẽ khóa luôn cái vỏ, không cho bấm vào vỏ nữa")]
    public bool disableContainerAfterOpen = true;

    private Interactable coreLogic;

    void Awake()
    {
        coreLogic = GetComponent<Interactable>();
    }

    void OnEnable()
    {
        if (coreLogic != null) coreLogic.OnDefaultInteract += RevealContent;
    }

    void OnDisable()
    {
        if (coreLogic != null) coreLogic.OnDefaultInteract -= RevealContent;
    }

    private void RevealContent()
    {
        // 1. Tắt hình con sò lúc đóng
        if (closedStateObject != null)
        {
            closedStateObject.SetActive(false);
        }

        // 2. Bật hình con sò lúc mở
        if (openStateObject != null)
        {
            openStateObject.SetActive(true);
        }

        // 3. Đảm bảo miếng thịt sò hiện lên (cho dù nó là con của clam_open)
        if (itemToReveal != null)
        {
            itemToReveal.SetActive(true);
            Debug.Log($"<color=yellow>[OpenContainer]</color> Đã mở sò! Lộ ra: {itemToReveal.name}");
        }

        // 4. Khóa tương tác của GameObject tổng (Clam)
        if (disableContainerAfterOpen)
        {
            if (TryGetComponent<Collider2D>(out var col))
            {
                col.enabled = false;
            }
            this.enabled = false;
        }
    }
}