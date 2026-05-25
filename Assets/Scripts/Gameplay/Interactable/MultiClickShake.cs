using UnityEngine;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(Interactable))]
public class MultiClickShake : MonoBehaviour
{
    public enum ShakeMode { Rotation, Position }

    [Header("Click Settings")]
    public int requiredClicks = 3;
    private int currentClicks = 0;

    [Header("Shake Effect")]
    public ShakeMode shakeMode = ShakeMode.Rotation;
    public float shakeDuration = 0.2f;
    public float shakeStrength = 15f;

    [Header("Drop Item")]

    [Tooltip("CÁCH 1: Nhặt thẳng vật thể này vào túi (Kéo file Data Item của nó vào đây)")]
    public Item directPickupItem; // TÍNH NĂNG MỚI NẰM Ở ĐÂY

    [Tooltip("CÁCH 2: Rớt ra một vật thể khác (Kéo object đang tàng hình vào đây)")]
    public GameObject itemToReveal;

    [Tooltip("Tick vào nếu muốn vật thể gốc này biến mất sau khi nhặt xong")]
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
        transform.DOComplete();

        // Nếu đã đủ số click -> Chạy cú rung cuối cùng rồi mới nhặt
        if (currentClicks >= requiredClicks)
        {
            StartCoroutine(FinalClickRoutine());
        }
        else
        {
            // Nếu chưa đủ thì cứ rung bình thường
            DoShake();
        }
    }

    private void DoShake()
    {
        if (shakeMode == ShakeMode.Rotation)
        {
            transform.DOShakeRotation(shakeDuration, new Vector3(0, 0, shakeStrength), vibrato: 10, randomness: 90);
        }
        else
        {
            transform.DOShakePosition(shakeDuration, strength: shakeStrength / 50f, vibrato: 10, randomness: 90);
        }
    }

    private IEnumerator FinalClickRoutine()
    {
        // 1. Tạm khóa click để tránh spam trong lúc đang diễn cú rung chót
        coreLogic.isLocked = true;

        // 2. Rung cú chót bần bật
        DoShake();

        // NÍN THỞ CHỜ RUNG XONG (Cảm giác game cực đã nằm ở dòng này)
        yield return new WaitForSeconds(shakeDuration);

        // 3. XỬ LÝ PHẦN THƯỞNG
        transform.DOComplete();

        // CÁCH 1: Tự động nhặt thẳng vào túi
        if (directPickupItem != null && Inventory.Instance != null)
        {
            Inventory.Instance.AddItem(directPickupItem);
            Debug.Log($"<color=green>[MultiClickShake]</color> Đã tự động nhặt {directPickupItem.itemId} vào túi!");
        }

        // CÁCH 2: Nhả object khác (Dùng cho các câu đố cũ)
        if (itemToReveal != null)
        {
            itemToReveal.SetActive(true);
        }

        // 4. DỌN DẸP
        if (destroyAfterDone)
        {
            gameObject.SetActive(false);
        }
        else
        {
            if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;
            this.enabled = false;
        }
    }
}