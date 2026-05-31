using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(NumberWheel2D))]
public class WheelInputWorld2D : MonoBehaviour
{
    public float dragThresholdPixels = 20f; // ngưỡng vuốt tính bằng pixel
    private Vector2 pointerStart;
    private bool isDragging = false;
    private NumberWheel2D wheel;
    private Camera mainCam;

    void Awake()
    {
        wheel = GetComponent<NumberWheel2D>();
        mainCam = Camera.main;
    }

    // MOUSE (Editor / PC)
    void OnMouseDown()
    {
        if (SettingsPopupController.IsOpen) return;
        if (IsPointerOverUI()) return;
        isDragging = true;
        pointerStart = Input.mousePosition;
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        Vector2 delta = (Vector2)Input.mousePosition - pointerStart;
        HandleSwipe(delta);
        isDragging = false;
    }

    // TOUCH (Mobile)
    void Update()
    {
        if (SettingsPopupController.IsOpen)
        {
            isDragging = false;
            return;
        }

        if (Input.touchCount == 0) return;

        // Tìm touch liên quan đến object này bằng raycast
        foreach (Touch t in Input.touches)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId)) continue;
            Vector2 worldPos = mainCam.ScreenToWorldPoint(t.position);
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit == null || hit.gameObject != gameObject) continue;

            if (t.phase == TouchPhase.Began)
            {
                isDragging = true;
                pointerStart = t.position;
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                if (!isDragging) continue;
                Vector2 delta = t.position - pointerStart;
                HandleSwipe(delta);
                isDragging = false;
            }
        }
    }

    void HandleSwipe(Vector2 delta)
    {
        // Chỉ xét theo chiều dọc nếu đủ ngưỡng
        if (Mathf.Abs(delta.y) >= dragThresholdPixels && Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
        {
            if (delta.y > 0)
                wheel.Increment(); // vuốt lên
            else
                wheel.Decrement(); // vuốt xuống
        }
        else
        {
            // Nếu muốn: click ngắn có thể gọi Increment hoặc không làm gì
            // wheel.Increment();
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
