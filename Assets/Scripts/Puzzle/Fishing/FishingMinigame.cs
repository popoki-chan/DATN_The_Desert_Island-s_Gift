using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class FishingMinigame : MonoBehaviour
{
    [Header("1. Cài đặt Vật phẩm nhận được")]
    public Item fishItem;

    [Header("2. Hiệu ứng cây lao (Tùy chọn)")]
    public Transform spearVisual;
    public float thrustDuration = 0.2f;

    [Header("3. Lớp va chạm của cá")]
    public LayerMask fishLayer;

    [Header("4. CẤU HÌNH LIÊN KẾT NHIỆM VỤ")]
    public FishermanQuest fishermanQuestScript;

    [Header("5. Cấu hình thời gian rút lui")]
    public float autoReturnDelay = 1.5f;

    // Đổi canShoot thành false lúc đầu để chặn cú click thừa khi vừa chuyển cảnh
    private bool canShoot = false;
    private bool isGameFinished = false;
    private Vector3 spearOriginalPos;
    private Tween delayedCallTween;

    void Start()
    {
        if (spearVisual != null) spearOriginalPos = spearVisual.position;
    }

    // --- SỬA LẠI HÀM NÀY ĐỂ CHẶN CLICK THỪA TỪ VIEW TRƯỚC ---
    void OnEnable()
    {
        isGameFinished = false;
        canShoot = false; // Khóa bắn tạm thời

        delayedCallTween?.Kill();
        // Dùng DOTween để delay khoảng 0.15 giây (hoặc qua vài khung hình) 
        // Sau khi cú click đưa lao của người chơi đã trôi qua hoàn toàn, mới mở khóa canShoot
        delayedCallTween = DOVirtual.DelayedCall(0.15f, () =>
        {
            canShoot = true;
            Debug.Log("<color=yellow>[FishingMinigame]</color> Hệ thống phóng lao đã sẵn sàng nhận lệnh click mới!");
        });
    }

    void OnDisable()
    {
        delayedCallTween?.Kill();
        if (spearVisual != null)
        {
            spearVisual.DOKill();
        }
    }

    void OnDestroy()
    {
        delayedCallTween?.Kill();
        if (spearVisual != null)
        {
            spearVisual.DOKill();
        }
    }

    void Update()
    {
        if (SettingsPopupController.IsOpen) return;

        // Hệ thống chỉ nhận lệnh nếu canShoot đã được mở khóa ở hàm OnEnable trên
        if (Input.GetMouseButtonDown(0) && canShoot && !isGameFinished)
        {
            if (IsPointerOverUI()) return;
            HandleFishingThrust();
        }
    }

    private void HandleFishingThrust()
    {
        canShoot = false;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 touchPoint = new Vector2(mousePos.x, mousePos.y);

        if (spearVisual != null)
        {
            spearVisual.DOMove(new Vector3(mousePos.x, mousePos.y, spearOriginalPos.z), thrustDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    Collider2D hit = Physics2D.OverlapPoint(touchPoint, fishLayer);

                    if (hit != null && !isGameFinished)
                    {
                        CatchFishWithSmoothReturn(hit.gameObject);
                    }
                    else
                    {
                        SmoothSpearReturn();
                    }
                });
        }
        else
        {
            Collider2D hit = Physics2D.OverlapPoint(touchPoint, fishLayer);
            if (hit != null && !isGameFinished) CatchFishWithSmoothReturn(hit.gameObject);
            else canShoot = true;
        }
    }

    private void CatchFishWithSmoothReturn(GameObject fishObj)
    {
        isGameFinished = true;
        Debug.Log("<color=green>[Fishing]</color> Đâm trúng cá! Tiến hành khóa logic vĩnh viễn.");

        if (fishermanQuestScript != null)
        {
            fishermanQuestScript.CompleteQuestAndLockPermanently();
        }

        fishObj.transform.DOKill();
        fishObj.transform.SetParent(spearVisual);
        fishObj.transform.DOShakeRotation(0.5f, 10f, 10, 90f);

        FishSpawner spawner = GetComponent<FishSpawner>();
        if (spawner != null) spawner.StopFutureWaves();

        SmoothSpearReturn(() => {
            Destroy(fishObj);

            if (fishItem != null && Inventory.Instance != null)
            {
                Inventory.Instance.AddItem(fishItem);
            }

            if (ViewManager.Instance != null)
            {
                ViewManager.Instance.GoBack();
            }
        });
    }

    private void SmoothSpearReturn(System.Action onComplete = null)
    {
        if (spearVisual != null)
        {
            spearVisual.DOMove(spearOriginalPos, thrustDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    // Chỉ cho phép đâm tiếp nếu trò chơi chưa kết thúc hoàn toàn
                    if (!isGameFinished) canShoot = true;
                    onComplete?.Invoke();
                });
        }
        else
        {
            if (!isGameFinished) canShoot = true;
            onComplete?.Invoke();
        }
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        if (EventSystem.current.IsPointerOverGameObject()) return true;
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                return true;
        }
        return false;
    }
}