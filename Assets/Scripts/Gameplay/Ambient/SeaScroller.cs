using UnityEngine;

public class SeaScroller : MonoBehaviour
{
    [Header("Tốc độ cuộn sóng")]
    public float speed = 0.5f;

    [Header("Ngưỡng reset (X nhỏ hơn giá trị này sẽ nhảy ra sau)")]
    public float resetThresholdX = -14.6f;

    [Header("Độ đè nhẹ giữa 2 ảnh để tránh khe hở đen")]
    public float overlapCorrection = 0.05f;

    private SeaScroller sibling;
    private float spriteWidth;

    void Start()
    {
        // Tự động lấy chiều rộng của Sprite
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            spriteWidth = sr.bounds.size.x;
        }
        else
        {
            spriteWidth = 17.68f; // Kích thước mặc định của bg_fishing
        }

        // Tự động tìm mảnh biển còn lại cùng cấp
        if (transform.parent != null)
        {
            foreach (Transform child in transform.parent)
            {
                if (child != transform && child.name == transform.name)
                {
                    sibling = child.GetComponent<SeaScroller>();
                    if (sibling == null)
                    {
                        // Thêm SeaScroller cho sibling nếu chưa có (nhằm đảm bảo cả 2 mảnh đều có)
                        sibling = child.gameObject.AddComponent<SeaScroller>();
                        sibling.speed = this.speed;
                        sibling.resetThresholdX = this.resetThresholdX;
                        sibling.overlapCorrection = this.overlapCorrection;
                    }
                    break;
                }
            }
        }
    }

    void Update()
    {
        // Di chuyển sang trái
        transform.localPosition += Vector3.left * speed * Time.deltaTime;

        // Nếu đã đi quá giới hạn bên trái màn hình
        if (transform.localPosition.x <= resetThresholdX)
        {
            if (sibling != null)
            {
                // Nhảy ra phía sau (bên phải) của mảnh kia
                float newX = sibling.transform.localPosition.x + spriteWidth - overlapCorrection;
                transform.localPosition = new Vector3(newX, transform.localPosition.y, transform.localPosition.z);
            }
        }
    }
}
