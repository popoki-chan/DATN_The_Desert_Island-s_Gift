using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class CutscenePlayer : MonoBehaviour
{
    [Header("Cấu hình slide ảnh")]
    [Tooltip("Kéo danh sách các sprite ảnh cutscene vào đây")]
    public Sprite[] slides;

    [Tooltip("SpriteRenderer dùng để hiển thị các bức ảnh cutscene. Nếu để trống sẽ tự lấy ở GameObject này hoặc con.")]
    public SpriteRenderer targetRenderer;

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
    private bool isWaitingForStartClick = false;
    private float delayTimer = 0f;
    private Tween fadeTween;

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

    public void PlayCutscene()
    {
        if (slides == null || slides.Length == 0)
        {
            Debug.LogWarning("[CutscenePlayer] Không có slide nào để chạy!");
            // Nếu không có slide, vẫn gọi hoàn thành để tránh kẹt game
            EndCutscene();
            return;
        }

        // Kích hoạt GameObject này để hàm Update có thể chạy
        gameObject.SetActive(true);

        // Ẩn SpriteRenderer trong thời gian chờ để không che màn hình gameplay
        if (targetRenderer != null)
        {
            targetRenderer.gameObject.SetActive(false);
        }

        isWaitingForStartClick = true;
        delayTimer = startDelay;
        isPlaying = false;
    }

    private void StartPlayingSlides()
    {
        isPlaying = true;
        currentIndex = 0;
        isTransitioning = false;

        // Hiện lại SpriteRenderer để trình chiếu
        if (targetRenderer != null)
        {
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

        if (isWaitingForStartClick)
        {
            if (delayTimer > 0f)
            {
                delayTimer -= Time.deltaTime;
                return;
            }

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                isWaitingForStartClick = false;
                StartPlayingSlides();
            }
            return;
        }

        if (!isPlaying) return;

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

        if (currentIndex < slides.Length)
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

    private void EndCutscene()
    {
        isPlaying = false;
        Debug.Log("[CutscenePlayer] Kết thúc cutscene!");
        onCutsceneComplete?.Invoke();

        if (loadNextSceneOnComplete && !string.IsNullOrEmpty(nextSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
}
