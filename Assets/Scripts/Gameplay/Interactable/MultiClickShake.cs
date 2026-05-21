using UnityEngine;
using DG.Tweening; // Bắt buộc phải có để dùng DOShake

[RequireComponent(typeof(Interactable))]
public class MultiClickShake : MonoBehaviour
{
    public enum ShakeMode { Rotation, Position }

    [Header("Cài đặt Click")]
    [Tooltip("Số lần click cần thiết để lấy được đồ")]
    public int requiredClicks = 3;
    private int currentClicks = 0;

    [Header("Hiệu ứng Rung (DOTween)")]
    public ShakeMode shakeMode = ShakeMode.Rotation;
    public float shakeDuration = 0.2f;
    [Tooltip("Độ mạnh của cú rung (Góc xoay hoặc khoảng cách giật)")]
    public float shakeStrength = 15f;

    [Header("Kết quả sau khi click đủ")]
    [Tooltip("Vật thể thật sự sẽ rớt ra để nhặt (Kéo object đang tàng hình vào đây)")]
    public GameObject itemToReveal;

    [Tooltip("Tick vào nếu muốn vật thể gốc này biến mất sau khi nhả đồ (như đập vỡ bình)")]
    public bool destroyAfterDone = true;

    private Interactable coreLogic;

    void Awake()
    {
        coreLogic = GetComponent<Interactable>();
    }

    void OnEnable()
    {
        if (coreLogic != null) coreLogic.OnDefaultInteract += HandleClick;
    }

    void OnDisable()
    {
        if (coreLogic != null) coreLogic.OnDefaultInteract -= HandleClick;
    }

    private void HandleClick()
    {
        if (currentClicks >= requiredClicks) return;

        currentClicks++;

        // 1. Dừng ngay hiệu ứng rung cũ nếu người chơi click quá nhanh (spam click)
        transform.DOComplete();

        // 2. Chạy hiệu ứng Rung
        if (shakeMode == ShakeMode.Rotation)
        {
            // Lắc qua lắc lại (Phù hợp rút dao, cạy gạch, nhổ củ cải...)
            transform.DOShakeRotation(shakeDuration, new Vector3(0, 0, shakeStrength), vibrato: 10, randomness: 90);
        }
        else
        {
            // Giật giật vị trí (Phù hợp đập gõ, đập heo đất...)
            transform.DOShakePosition(shakeDuration, strength: shakeStrength / 50f, vibrato: 10, randomness: 90);
        }

        // Âm thanh (nếu có thể thêm sau này)
        // Debug.Log($"<color=orange>[MultiClickShake]</color> Đã click {currentClicks}/{requiredClicks}");

        // 3. Kiểm tra xem đã click đủ chưa
        if (currentClicks >= requiredClicks)
        {
            // Reset lại góc/vị trí cho ngay ngắn trước khi biến mất
            transform.DOComplete();

            if (itemToReveal != null)
            {
                itemToReveal.SetActive(true); // Nhả đồ ra!
            }

            if (destroyAfterDone)
            {
                gameObject.SetActive(false); // Xóa sổ vật thể gốc
            }
            else
            {
                // Nếu không muốn xóa (VD: cục đá bự vẫn nằm đó, chỉ nhả con bọ ra)
                if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;
                this.enabled = false;
            }
        }
    }
}