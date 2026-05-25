using UnityEngine;
using System.Collections;
// using Spine.Unity; // BỎ DẤU // NẾU BẠN SỬ DỤNG SPINE 2D CHO CON CHIM/NHÂN VẬT

[RequireComponent(typeof(Interactable))]
public class PuzzleAnimationTrigger : MonoBehaviour
{
    [Header("1. Cài đặt Animation")]
    [Tooltip("Dành cho Unity Animator thường")]
    public Animator unityAnimator;
    public string animationTriggerName = "PlayAction";

    // [Header("Dành cho Spine 2D (Tùy chọn)")]
    // public SkeletonAnimation spineAnimator;
    // [SpineAnimation] public string spineAnimName;

    [Header("2. Cài đặt Thời gian chờ")]
    [Tooltip("Đợi bao nhiêu giây cho animation chạy xong rồi mới rớt đồ?")]
    public float delayBeforeSpawn = 1.5f;

    [Header("3. Vật phẩm rớt ra")]
    [Tooltip("Kéo hạt giống / gáo dừa đầy nước (đang tàng hình) vào đây")]
    public GameObject rewardItem;

    [Header("4. Tùy chọn sau khi xong")]
    [Tooltip("Bật nếu muốn con hải âu bay mất tiêu sau khi ị xong")]
    public bool destroyAfterDone = false;

    private Interactable coreLogic;

    void Awake()
    {
        coreLogic = GetComponent<Interactable>();
    }

    void OnEnable()
    {
        if (coreLogic != null) coreLogic.OnDefaultInteract += TriggerAction;
    }

    void OnDisable()
    {
        if (coreLogic != null) coreLogic.OnDefaultInteract -= TriggerAction;
    }

    private void TriggerAction()
    {
        // Khóa không cho click spam nữa
        coreLogic.isLocked = true;

        // Bắt đầu đếm ngược thời gian chờ Animation
        StartCoroutine(PlayAnimationAndSpawnRoutine());
    }

    private IEnumerator PlayAnimationAndSpawnRoutine()
    {
        // 1. CHẠY ANIMATION
        if (unityAnimator != null)
        {
            unityAnimator.SetTrigger(animationTriggerName);
        }

        // Nếu dùng Spine 2D thì bật đoạn code này lên:
        // if (spineAnimator != null && !string.IsNullOrEmpty(spineAnimName))
        // {
        //     spineAnimator.AnimationState.SetAnimation(0, spineAnimName, false);
        // }

        Debug.Log("<color=cyan>[AnimTrigger]</color> Đang chạy Animation... Chờ đợi rớt đồ.");

        // 2. CHỜ ĐỢI (Nín thở chờ hải âu rặn / chờ tay múc nước)
        yield return new WaitForSeconds(delayBeforeSpawn);

        // 3. NHẢ ĐỒ RA
        if (rewardItem != null)
        {
            rewardItem.SetActive(true);
            Debug.Log("<color=green>[AnimTrigger]</color> Đã nhả vật phẩm!");
        }

        // 4. XỬ LÝ VẬT THỂ GỐC
        if (destroyAfterDone)
        {
            gameObject.SetActive(false); // Biến mất
        }
        else
        {
            // Chỉ xóa script này để không gọi lại được nữa
            if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;
            this.enabled = false;
        }
    }
}