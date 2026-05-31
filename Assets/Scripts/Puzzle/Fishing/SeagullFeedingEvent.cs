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

    private Interactable coreLogic;

    void Awake()
    {
        coreLogic = GetComponent<Interactable>();
    }

    void Start()
    {
        if (seagullTransform != null) seagullTransform.gameObject.SetActive(false);
        if (fishOnRockVisual != null) fishOnRockVisual.SetActive(false);

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
            seagullTransform.gameObject.SetActive(true);
            Sequence seagullSequence = DOTween.Sequence();

            seagullSequence.Append(seagullTransform.DOMove(eatPoint.position, flyDownDuration).SetEase(Ease.OutQuad));

            seagullSequence.AppendCallback(() => {
                seagullTransform.DOShakePosition(eatDuration, new Vector3(0.05f, 0.1f, 0f), 5, 90f);
            });
            seagullSequence.AppendInterval(eatDuration);

            seagullSequence.AppendCallback(() => {
                if (fishOnRockVisual != null)
                {
                    fishOnRockVisual.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() => fishOnRockVisual.SetActive(false));
                }
            });

            seagullSequence.Append(seagullTransform.DOMove(flyAwayPoint.position, flyAwayDuration).SetEase(Ease.InQuad));

            // BƯỚC THẢ CỨT
            seagullSequence.OnComplete(() => {
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
                            Debug.Log("<color=brown>[Feeding]</color> Cứt đã hạ cánh. Lúc này hòn đá đã mất collider nên click vào cứt sẽ ăn ăn ăn ăn ngay!");
                        });
                }
            });
        }
    }
}