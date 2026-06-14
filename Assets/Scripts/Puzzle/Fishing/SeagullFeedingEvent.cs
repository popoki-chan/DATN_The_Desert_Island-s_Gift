using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Interactable))]
public class SeagullFeedingEvent : MonoBehaviour
{
    [Header("1. Cấu hình Vật phẩm")]
    public string fishItemId = "fish";

    [Header("2. Các Object Đồ họa tham chiếu")]
    public GameObject fishOnRockVisual;
    public Transform seagullTransform;
    [Tooltip("Điểm neo ở mỏ hải âu để cắp cá đi. Nếu để trống sẽ cắp ở tâm.")]
    public Transform fishHoldPoint;
    [Tooltip("Sprite Hải âu khi bay")]
    public Sprite flyingSprite;

    [Header("3. Cài đặt Tọa độ Di chuyển (DOTween)")]
    public Transform eatPoint;
    public Transform flyAwayPoint;

    [Header("4. Cài đặt Thời gian (Giây)")]
    public float flyDownDuration = 1.5f;
    public float eatDuration = 2.0f;
    public float flyAwayDuration = 1.5f;

    [Header("5. Cấu hình Cục Cứt (Giấu ngoài Camera)")]
    [Tooltip("Kéo cục cứt nhỏ ĐANG ĐẶT SẴN TRÊN SCENE (ngoài rìa cam) vào đây")]
    public GameObject mapPoopObject;
    public Transform poopDropPoint;
    public float poopFallDuration = 0.4f;
    [Tooltip("Âm thanh poop chạm đất")]
    public AudioClip poopSFX;

    private Interactable coreLogic;
    private SpriteRenderer seagullSpriteRenderer;
    private Sprite originalSprite;
    private Vector3 originalWorldScale = Vector3.one;
    private Sequence seagullSeq;

    void Awake()
    {
        coreLogic = GetComponent<Interactable>();
        if (seagullTransform != null)
        {
            seagullSpriteRenderer = seagullTransform.GetComponent<SpriteRenderer>();
            if (seagullSpriteRenderer != null)
            {
                originalSprite = seagullSpriteRenderer.sprite;
            }
        }
    }

    void Start()
    {
        if (seagullTransform != null) seagullTransform.gameObject.SetActive(false);
        if (fishOnRockVisual != null)
        {
            originalWorldScale = fishOnRockVisual.transform.lossyScale;
            fishOnRockVisual.SetActive(false);
        }

        coreLogic.requiredItemId = fishItemId;
        coreLogic.isLocked = true;
        coreLogic.OnDefaultInteract += HandleFishPlaced;
    }

    private void HandleFishPlaced()
    {
        coreLogic.requiredItemId = "";
        coreLogic.isLocked = false;
        coreLogic.OnDefaultInteract -= HandleFishPlaced;

        // --- DÒNG CHỐT HẠ: TẮT HOÀN TOÀN COLLIDER CỦA HÒN ĐÁ ---
        // Hòn đá "nghỉ hưu", không nhận click chuột nữa để nhường chỗ cho bãi cứt
        if (TryGetComponent<Collider2D>(out var rockCollider))
        {
            rockCollider.enabled = false;
            Debug.Log("<color=yellow>[Feeding]</color> Đã tắt Collider của hòn đá để tránh tranh chấp va chạm!");
        }

        if (fishOnRockVisual != null) fishOnRockVisual.SetActive(true);

        if (seagullTransform != null && eatPoint != null && flyAwayPoint != null)
        {
            // Bắt đầu bay xuống: chuyển sang sprite bay
            if (seagullSpriteRenderer != null && flyingSprite != null)
            {
                seagullSpriteRenderer.sprite = flyingSprite;
            }

            seagullTransform.gameObject.SetActive(true);
            seagullSeq?.Kill();
            seagullSeq = DOTween.Sequence();
            Sequence seagullSequence = seagullSeq;

            seagullSequence.Append(seagullTransform.DOMove(eatPoint.position, flyDownDuration).SetEase(Ease.OutQuad));

            // Đáp xuống ăn: chuyển sang sprite đứng
            seagullSequence.AppendCallback(() => {
                if (seagullSpriteRenderer != null)
                {
                    seagullSpriteRenderer.sprite = originalSprite;
                }
                seagullTransform.DOShakePosition(eatDuration, new Vector3(0.05f, 0.1f, 0f), 5, 90f);
            });
            seagullSequence.AppendInterval(eatDuration);

            // Bắt đầu cắp cá bay đi: chuyển sang sprite bay
            seagullSequence.AppendCallback(() => {
                if (seagullSpriteRenderer != null && flyingSprite != null)
                {
                    seagullSpriteRenderer.sprite = flyingSprite;
                }
                if (fishOnRockVisual != null)
                {
                    Transform parentTransform = fishHoldPoint != null ? fishHoldPoint : seagullTransform;
                    if (parentTransform != null)
                    {
                        fishOnRockVisual.transform.SetParent(parentTransform);
                        fishOnRockVisual.transform.localPosition = Vector3.zero;
                        fishOnRockVisual.transform.localRotation = Quaternion.identity;
                        
                        // Preserve the original world scale under the scaled parentTransform
                        Vector3 parentScale = parentTransform.lossyScale;
                        fishOnRockVisual.transform.localScale = new Vector3(
                            parentScale.x != 0 ? originalWorldScale.x / parentScale.x : originalWorldScale.x,
                            parentScale.y != 0 ? originalWorldScale.y / parentScale.y : originalWorldScale.y,
                            parentScale.z != 0 ? originalWorldScale.z / parentScale.z : originalWorldScale.z
                        );
                    }
                }
            });

            seagullSequence.Append(seagullTransform.DOMove(flyAwayPoint.position, flyAwayDuration).SetEase(Ease.InQuad));

            // Hoàn thành: trả lại sprite đứng mặc định và ẩn đi
            seagullSequence.OnComplete(() => {
                if (seagullSpriteRenderer != null)
                {
                    seagullSpriteRenderer.sprite = originalSprite;
                }
                seagullTransform.gameObject.SetActive(false);

                if (mapPoopObject != null && eatPoint != null)
                {
                    Vector3 dropStartPos = poopDropPoint != null ? poopDropPoint.position : eatPoint.position + new Vector3(0, 6f, 0);

                    // Dịch chuyển bãi cứt về vị trí trên trời rồi cho rơi xuống
                    mapPoopObject.transform.position = dropStartPos;

                    mapPoopObject.transform.DOMove(eatPoint.position, poopFallDuration)
                        .SetEase(Ease.InQuad)
                        .OnComplete(() => {
                            mapPoopObject.transform.DOPunchScale(new Vector3(0.4f, -0.2f, 0f), 0.2f, 1);
                            if (poopSFX != null && AudioManager.Instance != null)
                            {
                                AudioManager.Instance.PlaySFX(poopSFX);
                            }
                            Debug.Log("<color=brown>[Feeding]</color> Cứt đã hạ cánh. Lúc này hòn đá đã mất collider nên click vào cứt sẽ ăn ăn ăn ăn ngay!");
                        });
                }
            });
        }
    }

    void OnDestroy()
    {
        if (coreLogic != null)
        {
            coreLogic.OnDefaultInteract -= HandleFishPlaced;
        }
        seagullSeq?.Kill();
        seagullSeq = null;
        if (seagullTransform != null)
        {
            seagullTransform.DOKill();
        }
        if (mapPoopObject != null)
        {
            mapPoopObject.transform.DOKill();
        }
    }
}