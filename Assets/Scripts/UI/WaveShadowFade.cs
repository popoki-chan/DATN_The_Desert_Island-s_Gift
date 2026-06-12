using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class WaveShadowFade : MonoBehaviour
{
    [Header("Tham chiếu tới Sóng chính")]
    public RectTransform mainWave;

    [Header("Tốc độ hiện rõ dần (Alpha tăng mỗi giây)")]
    public float fadeInSpeed = 1.5f;

    [Header("Tốc độ mờ dần (Alpha giảm mỗi giây)")]
    public float fadeSpeed = 0.5f;

    [Header("Độ hiển thị tối đa của bóng sóng")]
    public float maxAlpha = 0.44f;

    private RawImage rawImage;
    private float lastWaveY;
    private bool hasInitialized = false;

    private RectTransform rectTransform;
    private Vector2 shadowToWaveOffset;
    private Vector2 peakAnchoredPos;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();

        if (mainWave != null && rectTransform != null)
        {
            // Tính toán khoảng cách (offset) ban đầu giữa shadow và wave tại Awake
            // để đảm bảo không bị ảnh hưởng bởi thứ tự chạy Start() của các script khác.
            shadowToWaveOffset = rectTransform.anchoredPosition - mainWave.anchoredPosition;
        }
    }

    void Start()
    {
        if (mainWave != null && rectTransform != null)
        {
            lastWaveY = mainWave.anchoredPosition.y;
            peakAnchoredPos = rectTransform.anchoredPosition;
            hasInitialized = true;
        }
    }

    void Update()
    {
        if (!hasInitialized || mainWave == null || rawImage == null || rectTransform == null) return;

        float currentWaveY = mainWave.anchoredPosition.y;
        float deltaY = currentWaveY - lastWaveY;

        // Lưu ý về hướng của sóng trong màn hình Menu:
        // - Biển ở phía trên (Y cao), bờ cát ở phía dưới (Y thấp).
        // - Khi sóng dâng lên bờ (Sóng lên): wave di chuyển xuống dưới => deltaY < -0.0001f.
        // - Khi sóng rút về biển (Sóng lùi): wave di chuyển lên trên => deltaY > 0.0001f.

        if (deltaY < -0.0001f)
        {
            // Sóng đang dâng lên bờ: bóng sóng di chuyển tịnh tiến đi theo cùng sóng chính
            rectTransform.anchoredPosition = mainWave.anchoredPosition + shadowToWaveOffset;
            peakAnchoredPos = rectTransform.anchoredPosition; // Cập nhật vị trí tiến xa nhất trên bờ cát

            // Hiện rõ dần về mức maxAlpha khi sóng dâng lên
            Color color = rawImage.color;
            if (color.a < maxAlpha)
            {
                color.a = Mathf.MoveTowards(color.a, maxAlpha, fadeInSpeed * Time.deltaTime);
                rawImage.color = color;
            }
        }
        else if (deltaY > 0.0001f)
        {
            // Sóng đang rút về biển: giữ bóng sóng đứng yên tại vị trí tiến xa nhất
            rectTransform.anchoredPosition = peakAnchoredPos;

            // Tự động mờ dần về 0
            Color color = rawImage.color;
            if (color.a > 0f)
            {
                color.a = Mathf.MoveTowards(color.a, 0f, fadeSpeed * Time.deltaTime);
                rawImage.color = color;
            }
        }
        else
        {
            // Khi sóng đứng im hoặc đạt đỉnh đứng im tạm thời:
            // Giữ nguyên vị trí cao nhất đã đạt được
            rectTransform.anchoredPosition = peakAnchoredPos;
        }

        lastWaveY = currentWaveY;
    }
}
