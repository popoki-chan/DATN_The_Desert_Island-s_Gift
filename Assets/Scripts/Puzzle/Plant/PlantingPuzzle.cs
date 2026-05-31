using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Interactable), typeof(SpriteRenderer))]
public class PlantingPuzzle : MonoBehaviour
{
    [Header("1. Cấu hình ID Vật phẩm")]
    public string digToolId = "sharp_rock_ch2";
    public string seedItemId = "seed";
    public string waterItemId = "water_coconut";

    [Header("2. Đồ Họa (Visuals)")]
    public GameObject dirtMoundVisual;
    public GameObject toolDiggingVisual;

    [Tooltip("Hạt giống nằm trên mặt đất (Trước khi tưới nước)")]
    public GameObject placedSeedVisual;

    [Tooltip("Object Mầm cây con (Mọc lên SAU KHI tưới nước)")]
    public GameObject seedlingVisual;

    [Header("3. Tinh chỉnh Hoạt ảnh Đào (Offsets)")]
    public float leftOffset = 0.3f;
    public float downOffset = 0.2f;
    public float rightOffset = 0.3f;
    public float dragHeightOffset = -0.1f;

    [Header("4. Tinh chỉnh Góc xoay (Angles)")]
    public float stabAngle = -30f;
    public float scoopAngle = 10f;

    [Header("5. Thời gian Diễn hoạt (Duration)")]
    public float stabDuration = 0.15f;
    public float dragDuration = 0.3f;
    public float resetDuration = 0.2f;

    private Interactable interactable;
    private int puzzleStep = 0; // 0: Cát phẳng, 1: Ụ đất, 2: Hạt nằm trên đất, 3: Nảy mầm

    void Awake()
    {
        interactable = GetComponent<Interactable>();
    }

    void Start()
    {
        if (dirtMoundVisual != null) dirtMoundVisual.transform.localScale = Vector3.zero;
        if (toolDiggingVisual != null) toolDiggingVisual.SetActive(false);
        if (placedSeedVisual != null) placedSeedVisual.SetActive(false);
        if (seedlingVisual != null) seedlingVisual.SetActive(false);

        interactable.requiredItemId = digToolId;
        interactable.isLocked = true;
        interactable.description = "Bãi cát chỗ này trông có vẻ mềm và dễ xới hơn bình thường...";

        interactable.OnDefaultInteract += HandleInteraction;
    }

    private void HandleInteraction()
    {
        // --- BƯỚC 1: ĐÀO ĐẤT ---
        if (puzzleStep == 0)
        {
            interactable.isLocked = true;
            Sequence digSequence = DOTween.Sequence();

            if (toolDiggingVisual != null)
            {
                toolDiggingVisual.SetActive(true);
                Vector3 toolStartPos = toolDiggingVisual.transform.localPosition;
                Vector3 toolStartRot = toolDiggingVisual.transform.localEulerAngles;

                for (int i = 0; i < 3; i++)
                {
                    float currentDirtScale = (i + 1) / 3f;

                    Vector3 stabPos = toolStartPos + new Vector3(-leftOffset, -downOffset, 0);
                    digSequence.Append(toolDiggingVisual.transform.DOLocalMove(stabPos, stabDuration).SetEase(Ease.InQuad));
                    digSequence.Join(toolDiggingVisual.transform.DOLocalRotate(new Vector3(0, 0, stabAngle), stabDuration));
                    digSequence.AppendCallback(() => { transform.DOPunchScale(new Vector3(0.05f, -0.02f, 0), 0.1f, 1); });

                    Vector3 dragPos = toolStartPos + new Vector3(rightOffset, dragHeightOffset, 0);
                    digSequence.Append(toolDiggingVisual.transform.DOLocalMove(dragPos, dragDuration).SetEase(Ease.Linear));
                    digSequence.Join(toolDiggingVisual.transform.DOLocalRotate(new Vector3(0, 0, scoopAngle), dragDuration));

                    if (dirtMoundVisual != null)
                    {
                        digSequence.Join(dirtMoundVisual.transform.DOScale(new Vector3(currentDirtScale, currentDirtScale, 1f), dragDuration).SetEase(Ease.OutBack));
                    }

                    digSequence.Append(toolDiggingVisual.transform.DOLocalMove(toolStartPos, resetDuration).SetEase(Ease.OutQuad));
                    digSequence.Join(toolDiggingVisual.transform.DOLocalRotate(toolStartRot, resetDuration));
                    digSequence.AppendInterval(0.1f);
                }
                digSequence.AppendCallback(() => toolDiggingVisual.SetActive(false));
            }

            digSequence.OnComplete(() => {
                interactable.requiredItemId = seedItemId;
                interactable.isLocked = true;
                interactable.description = "Một ụ đất tơi xốp vừa được vun lên. Giờ chỉ cần hạt giống...";
                puzzleStep = 1;
            });
        }

        // --- BƯỚC 2: GIEO HẠT (CHỈ HIỆN HẠT GIỐNG) ---
        else if (puzzleStep == 1)
        {
            if (placedSeedVisual != null)
            {
                placedSeedVisual.SetActive(true);
                // Hiệu ứng hạt giống rơi nhẹ xuống đất (Dùng OutBounce cho nảy nảy)
                placedSeedVisual.transform.DOLocalMoveY(0.2f, 0.4f).From().SetRelative(true).SetEase(Ease.OutBounce);
            }

            interactable.requiredItemId = waterItemId;
            interactable.isLocked = true;
            interactable.description = "Hạt giống đã nằm gọn trong đất. Giờ nó cần một ít nước ngọt để thức tỉnh...";

            puzzleStep = 2; // Chuyển sang chờ tưới nước
        }

        // --- BƯỚC 3: TƯỚI NƯỚC (NẢY MẦM) ---
        else if (puzzleStep == 2)
        {
            Sequence growSequence = DOTween.Sequence();

            // 1. Nếu có ảnh hạt giống, cho nó teo nhỏ lại và biến mất
            if (placedSeedVisual != null)
            {
                growSequence.Append(placedSeedVisual.transform.DOScale(Vector3.zero, 0.2f));
                growSequence.AppendCallback(() => placedSeedVisual.SetActive(false));
            }

            // 2. Mầm non búng lên từ dưới đất
            if (seedlingVisual != null)
            {
                growSequence.AppendCallback(() => seedlingVisual.SetActive(true));
                growSequence.Append(seedlingVisual.transform.DOScale(Vector3.zero, 0.5f).From().SetEase(Ease.OutBack));
            }

            growSequence.OnComplete(() => {
                Debug.Log("<color=cyan>[Planting]</color> Đã tưới nước! Cây đã nảy mầm!");
                interactable.requiredItemId = "";
                interactable.isLocked = false;
                interactable.description = "Một mầm non xanh tươi! Phép màu của sự sống là đây.";
                interactable.OnDefaultInteract -= HandleInteraction;
            });
        }
    }

    void OnDisable()
    {
        if (toolDiggingVisual != null) toolDiggingVisual.transform.DOKill();
        if (dirtMoundVisual != null) dirtMoundVisual.transform.DOKill();
        if (placedSeedVisual != null) placedSeedVisual.transform.DOKill();
        if (seedlingVisual != null) seedlingVisual.transform.DOKill();
    }
}