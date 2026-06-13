using UnityEngine;
using UnityEngine.UI;

public class StormRainDOTween : MonoBehaviour
{
    [Header("Rain Scrolling Speed")]
    [Tooltip("Tốc độ cuộn ngang (giả lập gió thổi nghiêng).")]
    public float scrollSpeedX = -0.6f;
    
    [Tooltip("Tốc độ cuộn dọc (giả lập mưa rơi xuống rất nhanh).")]
    public float scrollSpeedY = -4.0f;

    private RawImage rawImage;
    private Rect initialUvRect;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        if (rawImage != null)
        {
            initialUvRect = rawImage.uvRect;
            
            // Thiết lập shader Additive để lọc nền đen của vân mưa
            var additiveShader = Shader.Find("Legacy Shaders/Particles/Additive") 
                              ?? Shader.Find("Mobile/Particles/Additive")
                              ?? Shader.Find("UI/Default");
            
            if (additiveShader != null)
            {
                rawImage.material = new Material(additiveShader);
            }
        }
    }

    private void OnEnable()
    {
        if (rawImage != null)
        {
            rawImage.uvRect = initialUvRect;
        }
    }

    private void Update()
    {
        if (rawImage != null)
        {
            // Cuộn UV chéo nhanh xuống góc dưới bên trái
            var uv = rawImage.uvRect;
            uv.x += scrollSpeedX * Time.deltaTime;
            uv.y += scrollSpeedY * Time.deltaTime;
            rawImage.uvRect = uv;
        }
    }

    private void OnDisable()
    {
        if (rawImage != null)
        {
            rawImage.uvRect = initialUvRect;
        }
    }
}
