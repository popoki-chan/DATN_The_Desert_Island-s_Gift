using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Interactable))]
public class PoopDiggingMinigame : MonoBehaviour
{
    [Header("1. Cài đặt trò chơi")]
    public int requiredClicks = 3;

    [Header("2. Hình ảnh & Vật phẩm")]
    public Sprite openedPoopSprite;
    [Tooltip("Kéo Object Hạt giống bên trong View vào đây")]
    public GameObject seedVisual;
    public Item seedItem;

    [Header("3. Tham chiếu Cục cứt nhỏ ngoài bãi biển")]
    public GameObject mapPoopObject;

    private int currentClicks = 0;
    private bool isRevealed = false;
    private bool isCollected = false; // Chặn click liên tục khi đang bay
    private Sprite originalSprite;
    private SpriteRenderer spriteRenderer;
    private Interactable poopInteract;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSprite = spriteRenderer != null ? spriteRenderer.sprite : null;
        poopInteract = GetComponent<Interactable>();
    }

    void OnEnable()
    {
        currentClicks = 0;
        isRevealed = false;
        isCollected = false;

        if (spriteRenderer != null) spriteRenderer.sprite = originalSprite;

        if (seedVisual != null)
        {
            seedVisual.SetActive(false);
            // Hủy lắng nghe cũ nếu có để tránh trùng lặp sự kiện
            if (seedVisual.TryGetComponent<Interactable>(out var seedInteract))
            {
                seedInteract.OnDefaultInteract -= CollectSeed;
            }
        }

        if (poopInteract != null)
        {
            poopInteract.OnDefaultInteract += HandlePoopClicked;
        }
    }

    void OnDisable()
    {
        if (poopInteract != null) poopInteract.OnDefaultInteract -= HandlePoopClicked;
        if (seedVisual != null && seedVisual.TryGetComponent<Interactable>(out var seedInteract))
        {
            seedInteract.OnDefaultInteract -= CollectSeed;
        }
    }

    // --- CLICK VÀO CỤC CỨT TO ---
    private void HandlePoopClicked()
    {
        if (isRevealed) return; // Đã lòi hạt giống thì không bấm vào cứt nữa

        currentClicks++;

        // Hiệu ứng chọc chọc bẹp bẹp cứt
        transform.DOKill(true);
        transform.DOPunchScale(new Vector3(0.2f, -0.1f, 0), 0.2f, 2);

        if (currentClicks >= requiredClicks)
        {
            RevealSeed();
        }
    }

    // --- HÉ LỘ HẠT GIỐNG ---
    private void RevealSeed()
    {
        isRevealed = true;

        if (openedPoopSprite != null) spriteRenderer.sprite = openedPoopSprite;

        if (seedVisual != null)
        {
            seedVisual.SetActive(true);
            // Hiệu ứng nảy ra từ tâm bãi cứt
            seedVisual.transform.DOScale(Vector3.zero, 0.4f).From().SetEase(Ease.OutBack);

            // ĐĂNG KÝ: Khi người chơi click vào Hạt Giống, gọi hàm CollectSeed dưới đây
            if (seedVisual.TryGetComponent<Interactable>(out var seedInteract))
            {
                seedInteract.OnDefaultInteract += CollectSeed;
            }
        }
    }

    private void CollectSeed()
    {
        if (isCollected) return;
        isCollected = true;

        if (seedVisual != null && seedVisual.TryGetComponent<InteractableAnimation>(out var anim))
        {
            anim.PlayPickupAnimation(() => {

                if (seedItem != null && Inventory.Instance != null)
                {
                    Inventory.Instance.AddItem(seedItem);
                }

                if (mapPoopObject != null)
                {
                    if (mapPoopObject.TryGetComponent<Collider2D>(out var poopCollider))
                    {
                        poopCollider.enabled = false;
                        Debug.Log("<color=cyan>[PoopMinigame]</color> Đã tắt tương tác bãi cứt ngoài map. Giữ lại làm kỷ niệm!");
                    }
                }

                if (ViewManager.Instance != null)
                {
                   ViewManager.Instance.GoBack();
                }
            });
        }
        else
        {
            if (seedItem != null && Inventory.Instance != null) Inventory.Instance.AddItem(seedItem);
            if (mapPoopObject != null && mapPoopObject.TryGetComponent<Collider2D>(out var poopCol))
            {
                poopCol.enabled = false;
            }

            if (ViewManager.Instance != null) ViewManager.Instance.GoBack();
        }
    }
}