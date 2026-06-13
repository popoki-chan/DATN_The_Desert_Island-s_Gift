using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class CutscenePlayer : MonoBehaviour
{
    [Header("Cấu hình slide ảnh tĩnh")]
    [Tooltip("Kéo danh sách các sprite ảnh cutscene vào đây nếu dùng slide tĩnh")]
    public Sprite[] slides;

    [Tooltip("SpriteRenderer dùng để hiển thị các bức ảnh cutscene. Nếu để trống sẽ tự lấy ở GameObject này hoặc con.")]
    public SpriteRenderer targetRenderer;

    [Header("Cấu hình slide dạng GameObject hoạt họa")]
    [Tooltip("Kéo thả danh sách các GameObject phân cảnh hoạt họa vào đây. Nếu mảng này có phần tử, hệ thống sẽ ẩn/hiện GameObject tương ứng thay vì hoán đổi Sprite.")]
    public GameObject[] animatedSlides;

    [Header("Cấu hình tự động phát")]
    [Tooltip("Tích chọn để tự động phát cutscene khi cảnh chơi được load")]
    public bool playOnStart = false;

    [Header("Cấu hình hiệu ứng")]
    public float fadeDuration = 0.5f;
    [Tooltip("Âm thanh khi chuyển slide")]
    public AudioClip nextSlideSfx;

    [Header("Sự kiện hoàn thành")]
    public UnityEvent onCutsceneComplete;

    [Header("Chuyển cảnh sau khi hoàn thành")]
    [Tooltip("Nếu tích chọn, sẽ tự động chuyển cảnh sau khi hết cutscene")]
    public bool loadNextSceneOnComplete = false;
    [Tooltip("Tên của scene tiếp theo cần load (VD: Chapter2, Chapter3)")]
    public string nextSceneName;

    [Header("Cài đặt trễ khởi động")]
    [Tooltip("Thời gian chờ (giây) sau khi gọi PlayCutscene trước khi cho phép click để bắt đầu")]
    public float startDelay = 2.5f;

    private int currentIndex = 0;
    private bool isTransitioning = false;
    private bool isPlaying = false;
    private float delayTimer = 0f;
    private Tween fadeTween;

    private GameObject cachedInventoryPanel;
    private GameObject cachedBtnSetting;
    private GameObject cachedButtonParent;
    private GameObject cachedBorder;

    void OnDestroy()
    {
        fadeTween?.Kill();
        if (targetRenderer != null)
        {
            targetRenderer.DOKill();
        }
    }

    void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<SpriteRenderer>();
            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }
    }

    void Start()
    {
        if (playOnStart)
        {
            PlayCutscene();
        }
    }

    public void PlayCutscene()
    {
        // Cache references and deactivate gameplay UI elements
        cachedInventoryPanel = GameObject.Find("InventoryPanel");
        cachedBtnSetting = GameObject.Find("Btn Setting");
        cachedButtonParent = GameObject.Find("Button");
        cachedBorder = GameObject.Find("Border");

        if (cachedInventoryPanel != null) cachedInventoryPanel.SetActive(false);
        if (cachedBtnSetting != null) cachedBtnSetting.SetActive(false);
        if (cachedButtonParent != null) cachedButtonParent.SetActive(false);
        if (cachedBorder != null) cachedBorder.SetActive(false);

        bool hasAnimatedSlides = animatedSlides != null && animatedSlides.Length > 0;
        bool hasStaticSlides = slides != null && slides.Length > 0;

        if (!hasAnimatedSlides && !hasStaticSlides)
        {
            Debug.LogWarning("[CutscenePlayer] Không có slide (ảnh hoặc GameObject) nào để chạy!");
            EndCutscene();
            return;
        }

        // Kích hoạt GameObject này để hàm Update có thể chạy
        gameObject.SetActive(true);

        if (hasAnimatedSlides)
        {
            // Ẩn tất cả các slide hoạt họa đi ban đầu
            foreach (var go in animatedSlides)
            {
                if (go != null) go.SetActive(false);
            }
        }
        else if (targetRenderer != null)
        {
            // Ẩn SpriteRenderer trong thời gian chờ để không che màn hình gameplay
            targetRenderer.gameObject.SetActive(false);
        }

        // Bắt đầu trình chiếu các slide ngay lập tức
        StartPlayingSlides();

        // Cài đặt thời gian trễ không cho phép skip ngay
        delayTimer = startDelay;
    }

    private void StartPlayingSlides()
    {
        isPlaying = true;
        currentIndex = 0;
        isTransitioning = false;

        bool hasAnimatedSlides = animatedSlides != null && animatedSlides.Length > 0;

        if (!hasAnimatedSlides && targetRenderer != null)
        {
            // Hiện lại SpriteRenderer để trình chiếu ảnh tĩnh
            targetRenderer.gameObject.SetActive(true);
        }

        // Chuyển góc nhìn camera sang View này
        if (ViewManager.Instance != null)
        {
            ViewManager.Instance.ChangeView(gameObject);
            
            // Ẩn nút Back đi để người chơi không thoát ngang cutscene
            if (ViewManager.Instance.backButton != null)
            {
                ViewManager.Instance.backButton.gameObject.SetActive(false);
            }
        }

        // Hiển thị slide đầu tiên
        ShowSlide(currentIndex, false);
    }

    void Update()
    {
        if (SettingsPopupController.IsOpen) return;

        if (!isPlaying) return;

        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
            return;
        }

        // Click chuột trái hoặc bấm phím Space/Enter để sang slide tiếp theo
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            AdvanceSlide();
        }
    }

    private void AdvanceSlide()
    {
        if (isTransitioning) return;

        currentIndex++;

        int totalSlides = (animatedSlides != null && animatedSlides.Length > 0) ? animatedSlides.Length : (slides != null ? slides.Length : 0);

        if (currentIndex < totalSlides)
        {
            // Phát âm thanh chuyển slide
            if (nextSlideSfx != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(nextSlideSfx);
            }
            ShowSlide(currentIndex, true);
        }
        else
        {
            EndCutscene();
        }
    }

    private void ShowSlide(int index, bool useFade)
    {
        bool hasAnimatedSlides = animatedSlides != null && animatedSlides.Length > 0;

        if (hasAnimatedSlides)
        {
            if (index >= animatedSlides.Length) return;

            // Kích hoạt duy nhất slide hiện tại, tắt toàn bộ slide khác
            for (int i = 0; i < animatedSlides.Length; i++)
            {
                if (animatedSlides[i] != null)
                {
                    animatedSlides[i].SetActive(i == index);
                }
            }
        }
        else
        {
            if (targetRenderer == null || slides == null || index >= slides.Length) return;

            if (useFade)
            {
                isTransitioning = true;
                fadeTween?.Kill();
                // Smooth fade out -> change sprite -> fade in
                fadeTween = targetRenderer.DOFade(0f, fadeDuration).OnComplete(() =>
                {
                    if (targetRenderer != null && slides != null && index < slides.Length)
                    {
                        targetRenderer.sprite = slides[index];
                        fadeTween = targetRenderer.DOFade(1f, fadeDuration).OnComplete(() =>
                        {
                            isTransitioning = false;
                            fadeTween = null;
                        });
                    }
                    else
                    {
                        isTransitioning = false;
                        fadeTween = null;
                    }
                });
            }
            else
            {
                targetRenderer.sprite = slides[index];
                // Đảm bảo alpha là 1
                Color c = targetRenderer.color;
                c.a = 1f;
                targetRenderer.color = c;
            }
        }
    }

    private void EndCutscene()
    {
        isPlaying = false;
        Debug.Log("[CutscenePlayer] Kết thúc cutscene!");

        // Ẩn toàn bộ các slide hoạt họa đi khi hoàn tất
        if (animatedSlides != null && animatedSlides.Length > 0)
        {
            foreach (var go in animatedSlides)
            {
                if (go != null) go.SetActive(false);
            }
        }

        // Reactivate gameplay UI if we are staying in this scene
        if (!loadNextSceneOnComplete || string.IsNullOrEmpty(nextSceneName))
        {
            if (cachedInventoryPanel != null) cachedInventoryPanel.SetActive(true);
            if (cachedBtnSetting != null) cachedBtnSetting.SetActive(true);
            if (cachedButtonParent != null) cachedButtonParent.SetActive(true);
            if (cachedBorder != null) cachedBorder.SetActive(true);

            if (ViewManager.Instance != null)
            {
                ViewManager.Instance.GoBack();
            }
        }

        onCutsceneComplete?.Invoke();

        if (loadNextSceneOnComplete && !string.IsNullOrEmpty(nextSceneName))
        {
            if (SceneController.Instance != null)
            {
                SceneController.Instance.LoadScene(nextSceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}
