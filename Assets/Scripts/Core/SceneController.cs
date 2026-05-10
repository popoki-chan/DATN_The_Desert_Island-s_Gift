using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    [Header("References")]
    [Tooltip("CanvasGroup dùng để fade màn hình (kéo Image/Panel full-screen có CanvasGroup vào đây)")]
    public CanvasGroup fadeCanvas;

    [Header("Settings")]
    public float fadeDuration = 0.5f;

    // internal
    private Coroutine fadeRoutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // nếu chưa gán trong Inspector, cố gắng tìm CanvasGroup trong scene
            if (fadeCanvas == null)
            {
                fadeCanvas = FindObjectOfType<CanvasGroup>();
            }
            // nếu vẫn null, tạo 1 CanvasGroup tạm (không có visual)
            if (fadeCanvas == null)
            {
                GameObject go = new GameObject("SceneController_FadeCanvas");
                go.transform.SetParent(transform);
                fadeCanvas = go.AddComponent<CanvasGroup>();
                // bạn có thể gắn thêm Image nếu muốn hiển thị màu nền
            }

            // đảm bảo alpha khởi tạo
            fadeCanvas.alpha = Mathf.Clamp01(fadeCanvas.alpha);
            fadeCanvas.blocksRaycasts = fadeCanvas.alpha > 0f;
        }
        else
        {
            Destroy(gameObject);
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
            fadeCanvas = FindObjectOfType<CanvasGroup>();
            if (fadeCanvas == null)
            {
                // tạo tạm nếu không tìm thấy
                GameObject go = new GameObject("SceneController_FadeCanvas");
                go.transform.SetParent(transform);
                fadeCanvas = go.AddComponent<CanvasGroup>();
            }
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
}
