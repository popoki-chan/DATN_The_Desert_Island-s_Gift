using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : Singleton<SceneController>
{
    [Header("References")]
    [Tooltip("CanvasGroup dùng để fade màn hình (kéo Image/Panel full-screen có CanvasGroup vào đây)")]
    public CanvasGroup fadeCanvas;

    [Header("Settings")]
    public float fadeDuration = 0.5f;

    // internal
    private Coroutine fadeRoutine;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            SetupFadeCanvas();

            // Đảm bảo ban đầu màn hình không bị tối đen (đặc biệt khi chạy thử trong Editor)
            fadeCanvas.alpha = 0f;
            fadeCanvas.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// Load scene theo index với fade out -> load -> fade in
    /// </summary>
    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(DoLoad(sceneIndex));
    }

    IEnumerator DoLoad(int sceneIndex)
    {
        // fade out (màn tối)
        yield return StartCoroutine(Fade(1f));

        // bắt đầu load scene bất đồng bộ
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneIndex);
        // đảm bảo scene không bị paused
        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            yield return null;
        }

        // chờ 1 frame để scene mới khởi tạo UI nếu cần
        yield return null;

        // nếu fadeCanvas nằm trong scene cũ và bị destroyed, cố gắng tìm lại
        if (fadeCanvas == null)
        {
            SetupFadeCanvas();
        }

        // fade in (màn hiện)
        yield return StartCoroutine(Fade(0f));
    }

    /// <summary>
    /// Load scene theo tên với fade out -> load -> fade in
    /// </summary>
    public void LoadScene(string sceneName)
    {
        StartCoroutine(DoLoad(sceneName));
    }

    IEnumerator DoLoad(string sceneName)
    {
        // fade out (màn tối)
        yield return StartCoroutine(Fade(1f));

        // bắt đầu load scene bất đồng bộ
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        // đảm bảo scene không bị paused
        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            yield return null;
        }

        // chờ 1 frame để scene mới khởi tạo UI nếu cần
        yield return null;

        // nếu fadeCanvas nằm trong scene cũ và bị destroyed, cố gắng tìm lại
        if (fadeCanvas == null)
        {
            SetupFadeCanvas();
        }

        // fade in (màn hiện)
        yield return StartCoroutine(Fade(0f));
    }

    /// <summary>
    /// Fade tới alpha mục tiêu. Nếu đang có fade khác, dừng nó trước.
    /// </summary>
    IEnumerator Fade(float target)
    {
        // bảo đảm có fadeCanvas
        if (fadeCanvas == null)
        {
            yield break;
        }

        // dừng coroutine cũ nếu có
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        // nếu duration <= 0 thì set ngay
        if (fadeDuration <= 0f)
        {
            fadeCanvas.alpha = target;
            fadeCanvas.blocksRaycasts = target > 0f;
            yield break;
        }

        float start = fadeCanvas.alpha;
        float elapsed = 0f;

        // khi alpha > 0 thì chặn input
        fadeCanvas.blocksRaycasts = true;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            fadeCanvas.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }

        fadeCanvas.alpha = target;
        fadeCanvas.blocksRaycasts = target > 0f;

        fadeRoutine = null;
    }

    /// <summary>
    /// Hỗ trợ gọi fade độc lập (ngoài LoadScene)
    /// </summary>
    public void FadeTo(float targetAlpha, float duration = -1f)
    {
        if (duration > 0f) StartCoroutine(FadeToRoutine(targetAlpha, duration));
        else StartCoroutine(FadeToRoutine(targetAlpha, fadeDuration));
    }

    IEnumerator FadeToRoutine(float target, float duration)
    {
        if (fadeCanvas == null) yield break;

        // dừng fade hiện tại
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        // tạm set duration
        float prevDuration = fadeDuration;
        fadeDuration = Mathf.Max(0.0001f, duration);

        // chạy fade
        yield return StartCoroutine(Fade(target));

        // phục hồi
        fadeDuration = prevDuration;
    }

    private void CreateDynamicFadeCanvas()
    {
        // 1. Tạo Canvas độc lập cho Fade
        GameObject canvasGo = new GameObject("SceneController_FadeCanvas");
        DontDestroyOnLoad(canvasGo);
        
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -1; // Đặt dưới UI chính (0) nhưng trên Game World 2D
        
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();
        
        // 2. Tạo Panel màu đen che phủ toàn bộ màn hình
        GameObject panelGo = new GameObject("FadePanel");
        panelGo.transform.SetParent(canvasGo.transform, false);
        
        var rect = panelGo.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.one;
        
        var img = panelGo.AddComponent<Image>();
        img.color = Color.black;
        
        fadeCanvas = panelGo.AddComponent<CanvasGroup>();
    }

    private void SetupFadeCanvas()
    {
        if (fadeCanvas == null)
        {
            // 1. Try to find the specific FadeCanvas GameObject first (active or inactive)
            GameObject go = GameObject.Find("FadeCanvas") ?? GameObject.Find("SceneController_FadeCanvas");
            if (go == null)
            {
                var allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var candidate in allGameObjects)
                {
                    if (candidate.name == "FadeCanvas" || candidate.name == "SceneController_FadeCanvas")
                    {
                        if (candidate.scene.IsValid())
                        {
                            go = candidate;
                            break;
                        }
                    }
                }
            }

            if (go != null)
            {
                fadeCanvas = go.GetComponent<CanvasGroup>();
            }
        }

        if (fadeCanvas == null)
        {
            // 2. Safe fallback: Find any CanvasGroup but filter out common UI panel names
            var candidates = FindObjectsOfType<CanvasGroup>();
            foreach (var candidate in candidates)
            {
                string name = candidate.gameObject.name.ToLower();
                if (!name.Contains("setting") && !name.Contains("popup") && !name.Contains("inventory") && !name.Contains("tooltip"))
                {
                    fadeCanvas = candidate;
                    break;
                }
            }
        }

        if (fadeCanvas == null)
        {
            CreateDynamicFadeCanvas();
        }
        else
        {
            // Cấu hình fadeCanvas có sẵn thành root Canvas độc lập với sortingOrder = -1 để chỉ che màn chơi, không che UI
            GameObject go = fadeCanvas.gameObject;
            if (go.transform.parent != null)
            {
                go.transform.SetParent(null, false);
            }
            
            Canvas canvas = go.GetComponent<Canvas>();
            if (canvas == null) canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = -1; // Dưới Canvas chính (0) nhưng trên camera game
            
            if (go.GetComponent<CanvasScaler>() == null) go.AddComponent<CanvasScaler>();
            if (go.GetComponent<GraphicRaycaster>() == null) go.AddComponent<GraphicRaycaster>();
        }
    }
}
