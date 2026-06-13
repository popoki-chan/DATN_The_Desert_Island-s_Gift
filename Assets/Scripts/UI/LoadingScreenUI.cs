using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreenUI : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup canvasGroup;
    public Slider progressBar;
    public TextMeshProUGUI progressText;

    [Header("Settings")]
    public float fadeDuration = 0.4f;

    private Canvas canvas;

    private void Awake()
    {
        transform.localScale = Vector3.one;
        canvas = GetComponent<Canvas>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
        AssignCamera();
    }

    private void Update()
    {
        // Tự động gán lại camera nếu bị mất (ví dụ khi load xong scene mới)
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
        {
            AssignCamera();
        }
    }

    public void AssignCamera()
    {
        if (canvas == null) canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            var targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = FindObjectOfType<Camera>();
            }

            if (targetCamera != null)
            {
                canvas.worldCamera = targetCamera;
                // Đặt planeDistance ở mức 100f để trùng với canvas chính của MainMenu, tránh bị mờ/DoF hoặc lỗi render
                canvas.planeDistance = 100f;
            }

            // Đảm bảo canvas của Loading Screen luôn nằm trên layer UI và có sorting order cực cao
            canvas.sortingLayerName = "UI";
            canvas.sortingOrder = 999;
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        AssignCamera();
        
        if (canvasGroup != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeRoutine(1f));
            canvasGroup.blocksRaycasts = true;
        }
        
        SetProgress(0f);
    }

    public void Hide()
    {
        if (canvasGroup != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeRoutine(0f, () => {
                canvasGroup.blocksRaycasts = false;
                gameObject.SetActive(false);
            }));
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private System.Collections.IEnumerator FadeRoutine(float targetAlpha, System.Action onComplete = null)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            float dt = Time.unscaledDeltaTime;
            if (dt > 0.5f) dt = 0.02f;
            elapsed += dt;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
        if (onComplete != null)
        {
            onComplete.Invoke();
        }
    }

    public void SetProgress(float value)
    {
        float clampedValue = Mathf.Clamp01(value);
        if (progressBar != null)
        {
            progressBar.value = clampedValue;
        }
        if (progressText != null)
        {
            progressText.text = Mathf.RoundToInt(clampedValue * 100f) + "%";
        }
    }
}
