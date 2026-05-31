using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

[RequireComponent(typeof(RawImage), typeof(Collider2D))]
public class FruitCarver : MonoBehaviour
{
    [Header("1. Ảnh gốc của Quả (Read/Write Enabled)")]
    public Texture2D originalFruitTexture;

    [Header("2. Cấu hình nét khắc")]
    public Color carvingColor = new Color(0.3f, 0.2f, 0.1f, 1f);
    public int brushSize = 5;

    [Header("3. Cấu hình Tay Khắc (Tùy chọn)")]
    public RectTransform handVisual;
    public float defaultHandRotation = -15f;
    public float carvingHandRotation = -30f;
    public float rotationSmoothTime = 0.05f;

    [Header("4. Tự động nhận đồ")]
    public Item carvedFruitItem;

    [Header("5. Sự kiện bổ sung (Tùy chọn)")]
    public UnityEvent onCarvingFinished;

    private RawImage rawImage;
    private RectTransform rectTransform;
    private Collider2D fruitCollider;
    private Texture2D carvingCopy;
    private Canvas parentCanvas;

    private bool isDrawing = false;
    private Vector2 lastPixelUV;
    private bool isReadyToDraw = false;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
        rectTransform = GetComponent<RawImage>().rectTransform;
        fruitCollider = GetComponent<Collider2D>();
    }

    void OnEnable()
    {
        isReadyToDraw = false;
        isDrawing = false;
    }

    void Start()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null) return;

        if (handVisual != null)
        {
            handVisual.localEulerAngles = new Vector3(0, 0, defaultHandRotation);
        }

        if (originalFruitTexture == null) return;

        carvingCopy = new Texture2D(originalFruitTexture.width, originalFruitTexture.height, originalFruitTexture.format, false);
        Graphics.CopyTexture(originalFruitTexture, carvingCopy);
        rawImage.texture = carvingCopy;
    }

    void Update()
    {
        UpdateHandVisualPosition();

        if (!isReadyToDraw)
        {
            if (Input.GetMouseButtonUp(0)) isReadyToDraw = true;
            return;
        }

        // KHI NGƯỜI CHƠI BẤM CHUỘT XUỐNG
        if (Input.GetMouseButtonDown(0))
        {
            // Trường hợp 1: Bấm trúng cái quả -> Khắc chữ
            if (IsMouseOverFruit(out Vector2 uvPos))
            {
                isDrawing = true;
                lastPixelUV = uvPos;
                DrawBrushAtUV(uvPos);

                if (handVisual != null)
                {
                    handVisual.DOKill();
                    handVisual.DOLocalRotate(new Vector3(0, 0, carvingHandRotation), rotationSmoothTime);
                }
            }
            // Trường hợp 2: Bấm ra ngoài cái quả -> ĐÓNG VIEW
            else
            {
                SubmitCarving();
            }
        }

        // KHI ĐANG GIỮ CHUỘT ĐỂ VẼ
        if (Input.GetMouseButton(0) && isDrawing)
        {
            if (IsMouseOverFruit(out Vector2 uvPos))
            {
                DrawLineBetweenUV(lastPixelUV, uvPos);
                lastPixelUV = uvPos;
            }
            else
            {
                isDrawing = false;
                ReturnHandToDefaultRotation();
            }
        }

        // KHI NHẢ CHUỘT RA
        if (Input.GetMouseButtonUp(0))
        {
            if (isDrawing)
            {
                isDrawing = false;
                lastPixelUV = Vector2.zero;
            }
            ReturnHandToDefaultRotation();
        }
    }

    private void UpdateHandVisualPosition()
    {
        if (handVisual == null) return;
        Camera cam = (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : parentCanvas.worldCamera;
        RectTransform parentRect = handVisual.parent as RectTransform;
        if (parentRect == null) return;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, Input.mousePosition, cam, out localPoint))
        {
            handVisual.anchoredPosition = localPoint;
        }
    }

    private void ReturnHandToDefaultRotation()
    {
        if (handVisual != null)
        {
            handVisual.DOKill();
            handVisual.DOLocalRotate(new Vector3(0, 0, defaultHandRotation), rotationSmoothTime * 2);
        }
    }

    private bool IsMouseOverFruit(out Vector2 uvPos)
    {
        uvPos = Vector2.zero;
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null && hit.collider == fruitCollider)
        {
            Camera cam = (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : parentCanvas.worldCamera;
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, cam, out localPoint))
            {
                float width = rectTransform.rect.width;
                float height = rectTransform.rect.height;
                float u = (localPoint.x - rectTransform.rect.xMin) / width;
                float v = (localPoint.y - rectTransform.rect.yMin) / height;
                uvPos = new Vector2(u, v);
                return true;
            }
        }
        return false;
    }

    private void DrawBrushAtUV(Vector2 uvPos)
    {
        int px = (int)(uvPos.x * carvingCopy.width);
        int py = (int)(uvPos.y * carvingCopy.height);
        for (int x = -brushSize; x <= brushSize; x++)
        {
            for (int y = -brushSize; y <= brushSize; y++)
            {
                if (x * x + y * y <= brushSize * brushSize)
                {
                    int currentX = px + x;
                    int currentY = py + y;
                    if (currentX >= 0 && currentX < carvingCopy.width &&
                        currentY >= 0 && currentY < carvingCopy.height)
                    {
                        carvingCopy.SetPixel(currentX, currentY, carvingColor);
                    }
                }
            }
        }
        carvingCopy.Apply();
    }

    private void DrawLineBetweenUV(Vector2 startUV, Vector2 endUV)
    {
        int dist = (int)(Vector2.Distance(startUV, endUV) * Mathf.Max(carvingCopy.width, carvingCopy.height));
        if (dist > 1)
        {
            for (int i = 0; i <= dist; i++)
            {
                Vector2 interpolatedUV = Vector2.Lerp(startUV, endUV, (float)i / dist);
                DrawBrushAtUV(interpolatedUV);
            }
        }
        else DrawBrushAtUV(endUV);
    }

    public void SubmitCarving()
    {
        isReadyToDraw = false;
        isDrawing = false;

        Debug.Log("<color=green>[FruitCarver]</color> Người chơi click ra ngoài. Tiến hành thoát...");

        // 1. Thử thêm đồ vào túi (Bọc Try-Catch để chống sập game)
        try
        {
            if (Inventory.Instance != null && carvedFruitItem != null)
            {
                Inventory.Instance.AddItem(carvedFruitItem);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("<color=red>[FruitCarver Lỗi]</color> Có vấn đề ở script Túi Đồ của bạn: " + e.Message);
        }

        // 2. Ẩn cái tay đi
        if (handVisual != null)
        {
            handVisual.gameObject.SetActive(false);
        }

        // 3. Tự ẩn quả
        gameObject.SetActive(false);

        // Kích hoạt các sự kiện phụ
        onCarvingFinished?.Invoke();
    }
}