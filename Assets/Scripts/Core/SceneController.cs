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

    [Header("Loading Screen Settings")]
    [Tooltip("Tên prefab màn hình chờ đặt trong thư mục Resources")]
    [SerializeField] private string loadingScreenPrefabName = "LoadingScreen";

    // internal
    private Coroutine fadeRoutine;
    private LoadingScreenUI loadingScreenInstance;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            Application.runInBackground = true;
            SetupFadeCanvas();

            // Đảm bảo ban đầu màn hình không bị tối đen (đặc biệt khi chạy thử trong Editor)
            fadeCanvas.alpha = 0f;
            fadeCanvas.blocksRaycasts = false;
        }
    }

    private void Start()
    {
        // Nếu bắt đầu ở scene MainMenu (game startup), hiển thị màn hình chờ giả lập
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            StartCoroutine(ShowStartupLoadingScreen());
        }
    }

    private IEnumerator ShowStartupLoadingScreen()
    {
        Debug.Log("[SceneController] ShowStartupLoadingScreen STARTED");
        CreateLoadingScreen();
        if (loadingScreenInstance != null)
        {
            Debug.Log("[SceneController] Hiding menu buttons and showing loading screen");
            SetMenuButtonsActive(false);
            loadingScreenInstance.Show();
            
            float elapsed = 0f;
            float duration = 1.5f; // Thời gian chờ giả lập 1.5s lúc mở game
            while (elapsed < duration)
            {
                float dt = Time.unscaledDeltaTime;
                if (dt > 0.5f) dt = 0.02f;
                elapsed += dt;
                loadingScreenInstance.SetProgress(elapsed / duration);
                yield return null;
            }
            
            Debug.Log("[SceneController] Loading simulation finished. Waiting 0.2s");
            yield return new WaitForSeconds(0.2f);

            if (fadeCanvas == null)
            {
                SetupFadeCanvas();
            }
            if (fadeCanvas != null)
            {
                fadeCanvas.alpha = 0f;
                fadeCanvas.blocksRaycasts = false;
            }

            Debug.Log("[SceneController] Hiding loading screen");
            loadingScreenInstance.Hide();
            yield return new WaitForSeconds(loadingScreenInstance.fadeDuration);
            
            Debug.Log("[SceneController] Showing menu buttons");
            SetMenuButtonsActive(true);
        }
        Debug.Log("[SceneController] ShowStartupLoadingScreen FINISHED");
    }

    private void CreateLoadingScreen()
    {
        if (loadingScreenInstance != null) return;

        GameObject prefab = Resources.Load<GameObject>(loadingScreenPrefabName);
        if (prefab != null)
        {
            GameObject go = Instantiate(prefab);
            DontDestroyOnLoad(go);
            loadingScreenInstance = go.GetComponent<LoadingScreenUI>();
            if (loadingScreenInstance == null)
            {
                loadingScreenInstance = go.AddComponent<LoadingScreenUI>();
            }
        }
        else
        {
            Debug.LogWarning($"[SceneController] Không tìm thấy prefab '{loadingScreenPrefabName}' trong thư mục Resources. Sử dụng màn hình đen làm dự phòng.");
        }
    }

    private bool IsMainMenuScene(int sceneIndex)
    {
        string path = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
        return path != null && path.Contains("MainMenu");
    }

    private bool IsMainMenuScene(string sceneName)
    {
        return sceneName == "MainMenu" || sceneName.Contains("MainMenu");
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
        bool isMainMenu = IsMainMenuScene(sceneIndex);

        // Luôn fade màn hình sang đen trước khi chuyển scene
        yield return StartCoroutine(Fade(1f));

        if (isMainMenu)
        {
            CreateLoadingScreen();
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneIndex);
        op.allowSceneActivation = true;
        while (!op.isDone)
        {
            yield return null;
        }

        yield return null;

        if (fadeCanvas == null)
        {
            SetupFadeCanvas();
        }

        if (isMainMenu && loadingScreenInstance != null)
        {
            // Tắt các button trước khi hiện bg
            SetMenuButtonsActive(false);
            loadingScreenInstance.Show();

            // Fade về bình thường (hiển thị bg của MainMenu và thanh loading đè lên trên)
            yield return StartCoroutine(Fade(0f));

            float elapsed = 0f;
            float minDuration = 1.5f; // Thời gian tối thiểu hiển thị màn hình chờ (s)
            while (elapsed < minDuration)
            {
                float dt = Time.unscaledDeltaTime;
                if (dt > 0.5f) dt = 0.02f;
                elapsed += dt;
                loadingScreenInstance.SetProgress(elapsed / minDuration);
                yield return null;
            }
            loadingScreenInstance.SetProgress(1f);
            yield return new WaitForSeconds(0.2f);

            loadingScreenInstance.Hide();
            yield return new WaitForSeconds(loadingScreenInstance.fadeDuration);
            
            // Hiện lại các button sau khi load xong hoàn toàn
            SetMenuButtonsActive(true);
        }
        else
        {
            yield return StartCoroutine(Fade(0f));
        }
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
        bool isMainMenu = IsMainMenuScene(sceneName);

        // Luôn fade màn hình sang đen trước khi chuyển scene
        yield return StartCoroutine(Fade(1f));

        if (isMainMenu)
        {
            CreateLoadingScreen();
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = true;
        while (!op.isDone)
        {
            yield return null;
        }

        yield return null;

        if (fadeCanvas == null)
        {
            SetupFadeCanvas();
        }

        if (isMainMenu && loadingScreenInstance != null)
        {
            // Tắt các button trước khi hiện bg
            SetMenuButtonsActive(false);
            loadingScreenInstance.Show();

            // Fade về bình thường (hiển thị bg của MainMenu và thanh loading đè lên trên)
            yield return StartCoroutine(Fade(0f));

            float elapsed = 0f;
            float minDuration = 1.5f; // Thời gian tối thiểu hiển thị màn hình chờ (s)
            while (elapsed < minDuration)
            {
                float dt = Time.unscaledDeltaTime;
                if (dt > 0.5f) dt = 0.02f;
                elapsed += dt;
                loadingScreenInstance.SetProgress(elapsed / minDuration);
                yield return null;
            }
            loadingScreenInstance.SetProgress(1f);
            yield return new WaitForSeconds(0.2f);

            loadingScreenInstance.Hide();
            yield return new WaitForSeconds(loadingScreenInstance.fadeDuration);
            
            // Hiện lại các button sau khi load xong hoàn toàn
            SetMenuButtonsActive(true);
        }
        else
        {
            yield return StartCoroutine(Fade(0f));
        }
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
        canvas.sortingOrder = 999; // Đặt cực cao để che phủ toàn bộ UI và game
        
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();
        
        // 2. Tạo Panel màu đen che phủ toàn bộ màn hình
        GameObject panelGo = new GameObject("FadePanel");
        panelGo.transform.SetParent(canvasGo.transform, false);
        
        var rect = panelGo.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero; // Stretch full screen
        
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

        // Đã gỡ bỏ fallback nguy hiểm tự động tìm bất kỳ CanvasGroup nào (tránh chiếm quyền DragTutorial, Settings...)

        if (fadeCanvas == null)
        {
            CreateDynamicFadeCanvas();
        }
        else
        {
            // Cấu hình fadeCanvas có sẵn thành root Canvas độc lập với sortingOrder = 999 để che phủ toàn bộ UI và game
            GameObject go = fadeCanvas.gameObject;
            go.SetActive(true); // Đảm bảo kích hoạt GameObject để Canvas có thể render
            
            if (go.transform.parent != null)
            {
                go.transform.SetParent(null, false);
            }
            
            Canvas canvas = go.GetComponent<Canvas>();
            if (canvas == null) canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // Che phủ toàn bộ UI
            
            if (go.GetComponent<CanvasScaler>() == null) go.AddComponent<CanvasScaler>();
            if (go.GetComponent<GraphicRaycaster>() == null) go.AddComponent<GraphicRaycaster>();
        }
    }

    private void SetMenuButtonsActive(bool active)
    {
        GameObject btnPlay = GameObject.Find("Button Play");
        GameObject btnSetting = GameObject.Find("Btn Setting");

        // Tìm bằng đường dẫn Canvas nếu không tìm thấy trực tiếp
        if (btnPlay == null || btnSetting == null)
        {
            var canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                if (btnPlay == null)
                {
                    var t = canvas.transform.Find("Start/Button Play");
                    if (t != null) btnPlay = t.gameObject;
                }
                if (btnSetting == null)
                {
                    var t = canvas.transform.Find("Btn Setting");
                    if (t != null) btnSetting = t.gameObject;
                }
            }
        }

        // Tìm bằng Resources.FindObjectsOfTypeAll nếu chúng đang bị ẩn hoàn toàn
        if (btnPlay == null || btnSetting == null)
        {
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in allObjects)
            {
                if (go.scene.name == "MainMenu")
                {
                    if (btnPlay == null && go.name == "Button Play") btnPlay = go;
                    if (btnSetting == null && go.name == "Btn Setting") btnSetting = go;
                }
            }
        }

        if (btnPlay != null)
        {
            btnPlay.SetActive(active);
        }
        if (btnSetting != null)
        {
            btnSetting.SetActive(active);
        }
    }
}
