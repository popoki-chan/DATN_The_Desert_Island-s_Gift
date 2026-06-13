using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Điều khiển cutscene dựa trên Unity Timeline (PlayableDirector).
/// Ẩn UI gameplay khi phát, hiện nút "Bỏ qua" sau một khoảng trễ,
/// và khôi phục UI khi Timeline kết thúc hoặc người chơi bỏ qua.
/// </summary>
public class TimelineCutsceneController : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    //  INSPECTOR FIELDS
    // ─────────────────────────────────────────────────────────────

    [Header("Timeline")]
    [Tooltip("PlayableDirector chứa Timeline asset của cutscene. Nếu để trống sẽ tự tìm trên GameObject này hoặc con.")]
    public PlayableDirector director;

    [Tooltip("Tự động phát cutscene khi scene được load.")]
    public bool playOnStart = true;

    [Header("Fade Settings")]
    [Tooltip("CanvasGroup của Panel đen dùng để fade-in / fade-out.")]
    public CanvasGroup fadeOverlay;
    public float fadeDuration = 1.0f;

    [Header("Nút Bỏ Qua")]
    [Tooltip("Kéo Button 'Bỏ Qua' từ scene vào đây.")]
    public Button skipButton;

    [Tooltip("Sau bao nhiêu giây thì nút Bỏ Qua mới xuất hiện (tránh skip ngay đầu).")]
    public float skipButtonDelay = 1.5f;

    [Header("Sự kiện hoàn thành")]
    public UnityEvent onCutsceneComplete;

    [Header("Chuyển cảnh sau khi hoàn thành")]
    [Tooltip("Nếu tích, sẽ load scene mới sau khi cutscene kết thúc.")]
    public bool loadNextSceneOnComplete = false;
    [Tooltip("Tên scene cần load (VD: Chapter2).")]
    public string nextSceneName;

    // ─────────────────────────────────────────────────────────────
    //  PRIVATE STATE
    // ─────────────────────────────────────────────────────────────

    private bool isPlaying = false;
    private Coroutine skipButtonCoroutine;

    // Gameplay UI elements cần ẩn khi cutscene phát
    private GameObject cachedInventoryPanel;
    private GameObject cachedBtnSetting;
    private GameObject cachedButtonParent;
    private GameObject cachedBorder;
    private GameObject cachedDragTutorial;
    private GameObject cachedTooltipRoot;
    private GameObject cachedItemPickupNotification;

    // View 3D gameplay cần ẩn (lấy từ ViewManager)
    private GameObject cachedMainView;

    // ─────────────────────────────────────────────────────────────
    //  UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────

    void Awake()
    {
        // Tự tìm PlayableDirector nếu chưa gán
        if (director == null)
        {
            director = GetComponent<PlayableDirector>();
            if (director == null)
                director = GetComponentInChildren<PlayableDirector>();
        }
    }

    void Start()
    {
        // Ẩn nút Bỏ Qua khi mới vào (sẽ hiện sau skipButtonDelay)
        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        if (playOnStart)
            PlayCutscene();
    }

    void OnDestroy()
    {
        UnregisterDirectorEvent();
    }

    // ─────────────────────────────────────────────────────────────
    //  PUBLIC API
    // ─────────────────────────────────────────────────────────────

    /// <summary>Bắt đầu phát cutscene. Có thể gọi từ ngoài script (UnityEvent, script khác).</summary>
    public void PlayCutscene()
    {
        if (director == null)
        {
            Debug.LogError("[TimelineCutsceneController] Không tìm thấy PlayableDirector! Hãy kéo vào Inspector.");
            return;
        }

        // ── Ẩn toàn bộ UI gameplay ──────────────────────────────
        cachedInventoryPanel        = GameObject.Find("InventoryPanel");
        cachedBtnSetting            = GameObject.Find("Btn Setting");
        cachedButtonParent          = GameObject.Find("Button");
        cachedBorder                = GameObject.Find("Border");
        cachedDragTutorial          = GameObject.Find("DragTutorial");
        cachedTooltipRoot           = GameObject.Find("TooltipRoot");
        cachedItemPickupNotification = GameObject.Find("ItemPickupNotification");

        if (cachedInventoryPanel         != null) cachedInventoryPanel.SetActive(false);
        if (cachedBtnSetting             != null) cachedBtnSetting.SetActive(false);
        if (cachedButtonParent           != null) cachedButtonParent.SetActive(false);
        if (cachedBorder                 != null) cachedBorder.SetActive(false);
        if (cachedDragTutorial           != null) cachedDragTutorial.SetActive(false);
        if (cachedTooltipRoot            != null) cachedTooltipRoot.SetActive(false);
        if (cachedItemPickupNotification != null) cachedItemPickupNotification.SetActive(false);

        // ── Ẩn View 3D gameplay (View Main Room) qua ViewManager ─
        if (ViewManager.Instance != null)
        {
            cachedMainView = ViewManager.Instance.mainView;
            if (cachedMainView != null)
                cachedMainView.SetActive(false);

            // Ẩn nút Back
            if (ViewManager.Instance.backButton != null)
                ViewManager.Instance.backButton.gameObject.SetActive(false);
        }

        // ── Đăng ký sự kiện & phát Timeline ─────────────────────
        RegisterDirectorEvent();

        // Gán listener cho nút Bỏ Qua
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(false);
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(SkipCutscene);
        }

        // Cấu hình ban đầu cho fadeOverlay và tự động Fade In
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.alpha = 1f; // Bắt đầu ở màu đen che phủ
            fadeOverlay.blocksRaycasts = true;
            fadeOverlay.DOFade(0f, fadeDuration).SetEase(Ease.InOutSine).OnComplete(() => {
                fadeOverlay.blocksRaycasts = false; // Tắt chặn click khi fade xong
            });
        }

        isPlaying = true;
        director.Play();

        // Hiện nút Bỏ Qua sau một khoảng trễ
        if (skipButton != null)
        {
            if (skipButtonCoroutine != null) StopCoroutine(skipButtonCoroutine);
            skipButtonCoroutine = StartCoroutine(ShowSkipButtonAfterDelay());
        }
    }

    /// <summary>Bỏ qua cutscene ngay lập tức (gán vào onClick của nút Bỏ Qua).</summary>
    public void SkipCutscene()
    {
        if (!isPlaying) return;

        UnregisterDirectorEvent();

        if (director != null)
            director.Stop();

        EndCutscene();
    }

    // ─────────────────────────────────────────────────────────────
    //  PRIVATE HELPERS
    // ─────────────────────────────────────────────────────────────

    private void RegisterDirectorEvent()
    {
        if (director != null)
            director.stopped += OnDirectorStopped;
    }

    private void UnregisterDirectorEvent()
    {
        if (director != null)
            director.stopped -= OnDirectorStopped;
    }

    private void OnDirectorStopped(PlayableDirector d)
    {
        UnregisterDirectorEvent();
        EndCutscene();
    }

    private IEnumerator ShowSkipButtonAfterDelay()
    {
        yield return new WaitForSeconds(skipButtonDelay);
        if (isPlaying && skipButton != null)
            skipButton.gameObject.SetActive(true);
    }

    private void EndCutscene()
    {
        isPlaying = false;

        // Dừng coroutine hiện nút Bỏ Qua nếu đang chờ
        if (skipButtonCoroutine != null)
        {
            StopCoroutine(skipButtonCoroutine);
            skipButtonCoroutine = null;
        }

        // Ẩn nút Bỏ Qua
        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        // Fade Out màn hình trước khi load màn chơi
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.blocksRaycasts = true;
            fadeOverlay.DOFade(1f, fadeDuration).SetEase(Ease.InOutSine).OnComplete(FinishAndLoadNextScene);
        }
        else
        {
            FinishAndLoadNextScene();
        }
    }

    private void FinishAndLoadNextScene()
    {
        // ── Ẩn Cutscene Canvas (Cut1, Cut2) khi kết thúc ─────────
        var cutsceneCanvas = GetComponentInChildren<Canvas>(true);
        if (cutsceneCanvas != null)
            cutsceneCanvas.gameObject.SetActive(false);

        // ── Khôi phục View 3D gameplay ───────────────────────────
        if (cachedMainView != null)
            cachedMainView.SetActive(true);

        // ── Khôi phục UI gameplay ────────────────────────────────
        if (cachedInventoryPanel         != null) cachedInventoryPanel.SetActive(true);
        if (cachedBtnSetting             != null) cachedBtnSetting.SetActive(true);
        if (cachedButtonParent           != null) cachedButtonParent.SetActive(true);
        if (cachedBorder                 != null) cachedBorder.SetActive(true);
        if (cachedDragTutorial           != null) cachedDragTutorial.SetActive(true);
        if (cachedTooltipRoot            != null) cachedTooltipRoot.SetActive(true);
        if (cachedItemPickupNotification != null) cachedItemPickupNotification.SetActive(true);

        // Trả camera về view gameplay
        if (ViewManager.Instance != null)
            ViewManager.Instance.GoBack();

        onCutsceneComplete?.Invoke();

        // Chuyển scene nếu cần
        if (loadNextSceneOnComplete && !string.IsNullOrEmpty(nextSceneName))
        {
            if (SceneController.Instance != null)
                SceneController.Instance.LoadScene(nextSceneName);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
}
