using UnityEngine;

public class CombinationLock : MonoBehaviour
{
    [Header("Các dải số")]
    public NumberWheel2D[] wheels; // kéo 3 wheel vào inspector

    [Header("Mật mã đúng")]
    public string correctCode = "198"; // Bạn có thể đổi pass trực tiếp trên Inspector

    [Header("Hành động khi mở khóa")]
    public AudioClip unlockSfx;
    public Interactable targetChest; // Kéo cái Rương/Hộp mà ổ khóa này bảo vệ vào đây

    private bool isUnlocked = false;

    // Hàm này sẽ được gọi mỗi khi người chơi vuốt xong 1 số
    public void CheckCode()
    {
        if (wheels == null || wheels.Length < correctCode.Length)
        {
            Debug.LogWarning("[LockController] Wheels chưa gán đủ.");
            return;
        }

        for (int i = 0; i < correctCode.Length; i++)
        {
            if (wheels[i].currentNumber != correctCode[i])
            {
                Debug.Log("Mã sai");
                return;
            }
            Debug.Log("Mở khóa thành công!");
            Unlock();
        }

        void Unlock()
        {
            isUnlocked = true;
            Debug.Log("Mật mã ĐÚNG! Khóa đã được mở.");

            // 1. Phát âm thanh "Cạch"
            AudioManager.Instance?.PlaySFX(unlockSfx);

            // 2. Chuyển trạng thái cái rương từ Khóa -> Mở
            if (targetChest != null)
            {
                targetChest.isLocked = false;
                // Lúc này người chơi quay lại phòng chính, click vào rương là nó sẽ mở ra!
            }

            // 3. Tự động lùi về góc nhìn trước đó (thoát khỏi màn hình cận cảnh ổ khóa)
            // Nếu có hiệu ứng trễ 0.5s rồi mới lùi thì càng đẹp, mình dùng Invoke nhé:
            Invoke(nameof(ExitZoom), 0.8f);
        }

        void ExitZoom()
        {
            if (ViewManager.Instance != null)
            {
                ViewManager.Instance.GoBack();
            }
        }
    }
}