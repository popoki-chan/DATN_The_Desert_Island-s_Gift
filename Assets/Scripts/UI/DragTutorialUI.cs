using UnityEngine;
using TMPro;
using DG.Tweening;

public class DragTutorialUI : MonoBehaviour
{
    private enum TutorialStep
    {
        Drag,
        Tap,
        Finished
    }

    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI tutorialText;

    [Header("Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float startDelay = 0.5f;
    [SerializeField] private string message = "Drag to navigate around the enviroment";
    [SerializeField] private string tapMessage = "Use the left mouse button to tap on anything on the island.";

    [Header("Hand Animation Settings")]
    [SerializeField] private RectTransform handIcon;
    [SerializeField] private float swipeDistance = 150f;
    [SerializeField] private float swipeDuration = 1.2f;

    [Header("Mouse Animation Settings")]
    [SerializeField] private RectTransform mouseIcon;
    [SerializeField] private Sprite mouseNormalSprite;
    [SerializeField] private Sprite mouseClickSprite;
    [SerializeField] private float normalDuration = 0.6f;
    [SerializeField] private float clickDuration = 0.25f;

    private RoomPanner roomPanner;
    private TutorialStep currentStep = TutorialStep.Drag;
    
    private Tween handSwipeTween;
    private Coroutine mouseClickCoroutine;

    private Vector3 dragStartMousePos;
    private bool isTrackingDrag = false;

    void Start()
    {
        // Only display this tutorial in Chapter1
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Chapter1")
        {
            gameObject.SetActive(false);
            return;
        }

        if (tutorialText != null)
        {
            tutorialText.text = message;
        }

        roomPanner = FindObjectOfType<RoomPanner>();
        
        // Initially hide mouse icon, ensure hand icon is active
        if (mouseIcon != null) mouseIcon.gameObject.SetActive(false);
        if (handIcon != null) handIcon.gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            // Fade in after a small delay once the scene is loaded
            canvasGroup.DOFade(1f, fadeInDuration).SetDelay(startDelay);
        }

        // Start hand sliding animation if assigned
        if (handIcon != null)
        {
            float startX = handIcon.anchoredPosition.x;
            // Set initial position to the left of center
            handIcon.anchoredPosition = new Vector2(startX - swipeDistance / 2f, handIcon.anchoredPosition.y);
            
            // Loop back and forth horizontally
            handSwipeTween = handIcon.DOAnchorPosX(startX + swipeDistance / 2f, swipeDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    void Update()
    {
        if (currentStep == TutorialStep.Finished) return;

        if (currentStep == TutorialStep.Drag)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!IsPointerOverUI())
                {
                    dragStartMousePos = Input.mousePosition;
                    isTrackingDrag = true;
                }
            }

            if (isTrackingDrag && Input.GetMouseButton(0))
            {
                float travel = Vector3.Distance(dragStartMousePos, Input.mousePosition);
                if (travel > 40f)
                {
                    isTrackingDrag = false;
                    TransitionToTapStep();
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                isTrackingDrag = false;
            }
        }
        else if (currentStep == TutorialStep.Tap)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!IsPointerOverUI())
                {
                    FinishTutorial();
                }
            }
        }
    }

    void OnDestroy()
    {
        handSwipeTween?.Kill();
        if (mouseClickCoroutine != null)
        {
            StopCoroutine(mouseClickCoroutine);
        }
    }

    private void TransitionToTapStep()
    {
        currentStep = TutorialStep.Tap;
        handSwipeTween?.Kill();

        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0f, fadeOutDuration).OnComplete(() =>
            {
                if (handIcon != null) handIcon.gameObject.SetActive(false);
                if (mouseIcon != null) mouseIcon.gameObject.SetActive(true);
                
                if (tutorialText != null)
                {
                    tutorialText.text = tapMessage;
                }

                PlayMouseAnimation();
                canvasGroup.DOFade(1f, fadeInDuration);
            });
        }
        else
        {
            if (handIcon != null) handIcon.gameObject.SetActive(false);
            if (mouseIcon != null) mouseIcon.gameObject.SetActive(true);
            if (tutorialText != null) tutorialText.text = tapMessage;
            PlayMouseAnimation();
        }
    }

    private void PlayMouseAnimation()
    {
        if (mouseIcon != null)
        {
            if (mouseClickCoroutine != null)
            {
                StopCoroutine(mouseClickCoroutine);
            }
            mouseClickCoroutine = StartCoroutine(MouseClickAnimationRoutine());
        }
    }

    private System.Collections.IEnumerator MouseClickAnimationRoutine()
    {
        var image = mouseIcon.GetComponent<UnityEngine.UI.Image>();
        if (image == null) yield break;

        while (true)
        {
            image.sprite = mouseNormalSprite;
            mouseIcon.localScale = Vector3.one;
            yield return new WaitForSeconds(normalDuration);
            
            image.sprite = mouseClickSprite;
            mouseIcon.localScale = new Vector3(0.92f, 0.92f, 1f); // Shrink slightly to look clicked
            yield return new WaitForSeconds(clickDuration);
        }
    }

    private void FinishTutorial()
    {
        currentStep = TutorialStep.Finished;
        if (mouseClickCoroutine != null)
        {
            StopCoroutine(mouseClickCoroutine);
            mouseClickCoroutine = null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0f, fadeOutDuration).OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private bool IsPointerOverUI()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null) return false;
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return true;
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                return true;
        }
        return false;
    }
}