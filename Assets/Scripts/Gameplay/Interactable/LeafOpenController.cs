using UnityEngine;
using System.Collections;

public class LeafOpenController : MonoBehaviour
{
    [Tooltip("Collider của code_2 để chuyển sang View Coconut Tree")]
    public Collider2D code2Collider;

    private Interactable leafInteractable;
    private InteractableAnimation leafAnim;

    void Awake()
    {
        leafInteractable = GetComponent<Interactable>();
        leafAnim = GetComponent<InteractableAnimation>();
    }

    void Start()
    {
        // Kiểm tra nếu lá chưa mở thì mới khóa collider của code_2
        if (leafAnim != null && !leafAnim.IsRotated)
        {
            if (code2Collider != null)
            {
                code2Collider.enabled = false;
            }
        }
        else
        {
            // Nếu đã mở rồi, đảm bảo code_2 collider được mở và leaf collider bị khóa để tránh click tiếp
            if (code2Collider != null)
            {
                code2Collider.enabled = true;
            }
            var leafCol = GetComponent<Collider2D>();
            if (leafCol != null)
            {
                leafCol.enabled = false;
            }
        }
    }

    void OnEnable()
    {
        if (leafInteractable != null)
        {
            leafInteractable.OnDefaultInteract += HandleLeafClicked;
        }
    }

    void OnDisable()
    {
        if (leafInteractable != null)
        {
            leafInteractable.OnDefaultInteract -= HandleLeafClicked;
        }
    }

    private void HandleLeafClicked()
    {
        StartCoroutine(WaitAndEnableCode2());
    }

    private IEnumerator WaitAndEnableCode2()
    {
        // Khóa collider của lá để tránh người chơi bấm tiếp khi đang hoặc sau khi mở
        var leafCol = GetComponent<Collider2D>();
        if (leafCol != null)
        {
            leafCol.enabled = false;
        }

        // Đợi hoạt ảnh mở lá hoàn tất (quay + rung lắc)
        float waitTime = 0.8f; // Mặc định
        if (leafAnim != null)
        {
            waitTime = leafAnim.rotateDuration + leafAnim.shakeDuration;
        }

        yield return new WaitForSeconds(waitTime);

        // Mở collider của code_2 để cho phép chuyển sang View Coconut Tree
        if (code2Collider != null)
        {
            code2Collider.enabled = true;
            Debug.Log("[LeafOpenController] Đã mở collider của code_2 sau khi lá mở xong.");
        }
    }
}
