using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Interactable : MonoBehaviour
{
    [Header("Thông tin cơ bản")]
    public string id;
    [TextArea] public string description;

    [Header("Trạng thái")]
    public bool isPickable = false;
    public bool isLocked = false;
    public string requiredItemId;

    [Header("Âm thanh")]
    public AudioClip onClickSfx;

    [Header("Chuyển cảnh Zoom In")]
    public bool isZoomable = false;
    public GameObject targetView;

    // Sự kiện để các Manager khác lắng nghe
    public event Action<Interactable> OnClicked;

    // ----- Phần mới: chọn hành động khi click -----
    public enum InteractionAction { None, Shake, Rotate, ShakeAndRotate }
    [Header("Tương tác động")]
    public InteractionAction action = InteractionAction.None;

    [Header("Shake")]
    public float shakeDuration = 0.6f;
    public float shakeAmplitude = 8f;
    public float shakeFrequency = 20f;

    // enum khai báo riêng, không đặt attribute trên enum
    public enum RotateMode { Relative, Toggle, Absolute }

    [Header("Rotate")]
    public RotateMode rotateMode = RotateMode.Relative;
    public float rotateAngle = 90f; // dùng cho Relative/Toggle
    public float targetAngle = 90f; // dùng cho Absolute
    public float rotateDuration = 0.4f;

    [Header("Rotate One Time")]
    public bool rotateOnce = true; // nếu true thì chỉ rotate 1 lần
    public bool allowShakeDuringRotate = true; // cho phép shake ngay cả khi đã rotate
    [Header("Pickup Anim")]
    public float pickupJumpHeight = 2f;      // world units nhảy lên
    public float pickupDuration = 0.45f;       // thời gian animation
    public AnimationCurve pickupCurve = AnimationCurve.EaseInOut(0, 1, 2, 1); // curve cho chuyển động
    public bool fadeOutOnPickup = false;        // có fade sprite khi nhặt không
    public float fadeDuration = 0.05f;         // thời gian fade (phần cuối)
    public AudioClip pickupSfx;

    // internal
    private Quaternion initialRotation;
    private Coroutine rotateCoroutine = null;
    private Coroutine shakeCoroutine = null;
    private bool isRotated = false;
    private bool toggleState = false;

    void Awake()
    {
        initialRotation = transform.rotation;
    }

    public void RaiseClicked()
    {
        OnClicked?.Invoke(this);
    }

    void OnMouseEnter()
    {
        TooltipUI.Instance?.Show(description);
    }

    void OnMouseExit()
    {
        TooltipUI.Instance?.Hide();
    }

    void OnMouseDown()
    {
        OnClicked?.Invoke(this);

        if (isPickable)
        {
            Pickup();
            return;
        }

        if (isLocked)
        {
            if (Inventory.Instance != null)
            {
                Inventory.Instance.TryUseOn(Inventory.Instance.currentSelectedItem, this);
            }
            return;
        }

        if (isZoomable && targetView != null)
        {
            AudioManager.Instance?.PlaySFX(onClickSfx);
            ViewManager.Instance.ChangeView(targetView);
            return;
        }

        Interact();
    }

    public virtual void Interact()
    {
        Debug.Log($"[Interactable] Interact: {id}");
        AudioManager.Instance?.PlaySFX(onClickSfx);

        switch (action)
        {
            case InteractionAction.Shake:
                StartShake();
                break;
            case InteractionAction.Rotate:
                StartRotate();
                break;
            case InteractionAction.ShakeAndRotate:
                StartShakeAndRotate(); // rotate trước rồi shake
                break;
            default:
                break;
        }
    }

    public virtual void Pickup()
    {
        if (!isPickable) return;

        // Load Item asset từ Resources
        Item asset = Resources.Load<Item>($"Items/{id}");

        if (asset == null)
        {
            Debug.LogWarning($"[Pickup] Không tìm thấy Item asset id '{id}' trong Resources/Items/");
            return;
        }

        // Start animation coroutine rồi add item khi animation xong
        StartCoroutine(PickupAnimationAndAddToInventory(asset));
    }

    private IEnumerator PickupAnimationAndAddToInventory(Item asset)
    {
        // Play SFX
        if (pickupSfx != null)
            AudioManager.Instance?.PlaySFX(pickupSfx);
        else
            AudioManager.Instance?.PlaySFX(onClickSfx);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Transform t = transform;
        Vector3 startPos = t.position;
        Vector3 targetPos = startPos + Vector3.up * pickupJumpHeight;
        Vector3 startScale = t.localScale;
        Vector3 targetScale = startScale * 0.85f; // nhỏ lại 1 chút khi nhảy
        float half = pickupDuration * 0.6f; // phần di chuyển, phần fade nằm ở cuối

        float elapsed = 0f;

        // Nếu fade, lưu màu ban đầu
        Color startColor = sr != null ? sr.color : Color.white;

        // Di chuyển + scale theo curve
        while (elapsed < pickupDuration)
        {
            float t01 = Mathf.Clamp01(elapsed / pickupDuration);
            float c = pickupCurve.Evaluate(t01);

            // Di chuyển theo parabolic feel: lên rồi xuống nhẹ (sử dụng curve)
            t.position = Vector3.LerpUnclamped(startPos, targetPos, c);
            t.localScale = Vector3.Lerp(startScale, targetScale, c);

            // Fade ở phần cuối
            if (fadeOutOnPickup && sr != null && t01 > 1f - (fadeDuration / pickupDuration))
            {
                float fadeT = (t01 - (1f - fadeDuration / pickupDuration)) / (fadeDuration / pickupDuration);
                Color col = startColor;
                col.a = Mathf.Lerp(startColor.a, 0f, fadeT);
                sr.color = col;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Đảm bảo trạng thái cuối
        t.position = targetPos;
        t.localScale = targetScale;
        if (fadeOutOnPickup && sr != null)
        {
            Color col = startColor;
            col.a = 0f;
            sr.color = col;
        }

        // Thêm vào inventory
        Inventory.Instance?.AddItem(asset);

        // Xóa object khỏi scene
        Destroy(gameObject);
    }

    // ----- Public helpers (quản lý coroutine an toàn) -----
    public void StartShake()
    {
        if (action != InteractionAction.Shake && action != InteractionAction.ShakeAndRotate) return;
        if (shakeCoroutine != null) return;
        if (action == InteractionAction.ShakeAndRotate && rotateCoroutine != null) return;
        shakeCoroutine = StartCoroutine(DoShake());
    }

    public void StartRotate()
    {
        if (action != InteractionAction.Rotate && action != InteractionAction.ShakeAndRotate) return;
        if (rotateOnce && isRotated) return;
        if (rotateCoroutine != null) return;
        rotateCoroutine = StartCoroutine(DoRotate());
    }

    public void StartShakeAndRotate()
    {
        if (action != InteractionAction.ShakeAndRotate) return;
        if (rotateOnce && isRotated)
        {
            if (allowShakeDuringRotate)
            {
                StartShake();
            }
            return;
        }
        if (rotateCoroutine != null) return;
        if (shakeCoroutine != null) return;

        rotateCoroutine = StartCoroutine(DoRotateThenShake());
    }

    // ----- Coroutines -----
    IEnumerator DoShake()
    {
        float elapsed = 0f;
        Quaternion baseRot = transform.rotation;
        while (elapsed < shakeDuration)
        {
            float t = elapsed * shakeFrequency;
            float angle = Mathf.Sin(t) * shakeAmplitude * (1f - elapsed / shakeDuration);
            transform.rotation = baseRot * Quaternion.Euler(0f, 0f, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = baseRot;
        shakeCoroutine = null;
    }

    IEnumerator DoRotate()
    {
        if (rotateOnce && isRotated)
        {
            rotateCoroutine = null;
            yield break;
        }

        float elapsed = 0f;
        Quaternion start = transform.rotation;
        Quaternion end;

        switch (rotateMode)
        {
            case RotateMode.Relative:
                end = start * Quaternion.Euler(0f, 0f, rotateAngle);
                break;
            case RotateMode.Toggle:
                if (!toggleState)
                    end = start * Quaternion.Euler(0f, 0f, rotateAngle);
                else
                    end = start * Quaternion.Euler(0f, 0f, -rotateAngle);
                toggleState = !toggleState;
                break;
            case RotateMode.Absolute:
                Vector3 euler = transform.eulerAngles;
                end = Quaternion.Euler(euler.x, euler.y, targetAngle);
                break;
            default:
                end = start;
                break;
        }

        while (elapsed < rotateDuration)
        {
            transform.rotation = Quaternion.Slerp(start, end, elapsed / rotateDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = end;

        if (rotateOnce) isRotated = true;

        rotateCoroutine = null;
    }

    IEnumerator DoRotateThenShake()
    {
        // 1) Rotate (tuân thủ rotateOnce)
        if (rotateOnce && isRotated)
        {
            // nếu đã rotate thì bỏ qua rotate, chuyển thẳng sang shake nếu cho phép
            if (allowShakeDuringRotate)
            {
                // nếu đang shake thì không bắt lại
                if (shakeCoroutine == null)
                    shakeCoroutine = StartCoroutine(DoShake());
                // chờ shake hoàn tất
                while (shakeCoroutine != null) yield return null;
            }
            rotateCoroutine = null;
            yield break;
        }

        // Thực hiện rotate và chờ hoàn tất
        yield return StartCoroutine(DoRotate());

        // 2) Sau khi rotate xong, bắt shake (nếu cấu hình cho shake)
        // Nếu action là ShakeAndRotate thì luôn chạy shake sau rotate
        if (shakeCoroutine == null)
        {
            shakeCoroutine = StartCoroutine(DoShake());
            // chờ shake hoàn tất
            while (shakeCoroutine != null) yield return null;
        }

        rotateCoroutine = null;
    }

    // ----- Utility -----
    public void ResetRotation()
    {
        transform.rotation = initialRotation;
        isRotated = false;
        toggleState = false;
    }
}
