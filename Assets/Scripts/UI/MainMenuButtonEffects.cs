using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class MainMenuButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Dập dềnh (Idle Bobbing)")]
    public float bobSpeed = 2f;
    public float bobRange = 10f;
    public bool enableBobbing = true;

    [Header("Nhấn xuống (Hover / Press Effect)")]
    public float hoverOffset = -10f;
    public float pressOffset = -25f;
    public float transitionDuration = 0.2f;

    private Vector3 startPos;
    private float bobTimer;
    private bool isHovered = false;
    private bool isPressed = false;
    
    // Interactability
    private Button btn;
    private bool wasInteractable = true;
    
    // Tweened parameters
    private float currentInteractionOffset = 0f;
    private float currentScale = 1f;
    private float bobWeight = 1f;
    
    private void Awake()
    {
        startPos = transform.localPosition;
        currentScale = transform.localScale.x;
    }

    private void Start()
    {
        bobTimer = Random.Range(0f, 100f); // Randomize phase for natural look
    }

    private void OnEnable()
    {
        btn = GetComponent<Button>();
        if (btn != null)
        {
            wasInteractable = btn.interactable;
            ApplyInteractableState(wasInteractable, true); // instant update
        }
    }

    private void Update()
    {
        if (btn != null)
        {
            bool isInteractable = btn.interactable;
            if (isInteractable != wasInteractable)
            {
                wasInteractable = isInteractable;
                ApplyInteractableState(isInteractable, false);
            }
        }
        
        if (!wasInteractable) return;
        
        // Calculate bobbing
        float bobOffset = 0f;
        if (enableBobbing)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            bobOffset = Mathf.Sin(bobTimer) * bobRange * bobWeight;
        }
        
        // Update local position and scale
        transform.localPosition = new Vector3(startPos.x, startPos.y + currentInteractionOffset + bobOffset, startPos.z);
        transform.localScale = new Vector3(currentScale, currentScale, currentScale);
    }

    private void ApplyInteractableState(bool isInteractable, bool instant)
    {
        var image = GetComponent<Image>();
        if (image != null)
        {
            Color targetColor = isInteractable ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);
            if (instant)
            {
                image.color = targetColor;
            }
            else
            {
                image.DOColor(targetColor, transitionDuration).SetTarget(this);
            }
        }
        
        if (!isInteractable)
        {
            DOTween.Kill(this);
            isHovered = false;
            isPressed = false;
            
            if (instant)
            {
                currentInteractionOffset = 0f;
                currentScale = 1f;
                bobWeight = 0f;
                transform.localScale = Vector3.one;
                transform.localPosition = startPos;
            }
            else
            {
                DOTween.To(() => currentInteractionOffset, x => currentInteractionOffset = x, 0f, transitionDuration).SetEase(Ease.OutQuad).SetTarget(this);
                DOTween.To(() => currentScale, x => currentScale = x, 1f, transitionDuration).SetEase(Ease.OutQuad).SetTarget(this);
                DOTween.To(() => bobWeight, x => bobWeight = x, 0f, transitionDuration).SetEase(Ease.OutQuad).SetTarget(this);
            }
        }
        else
        {
            if (instant)
            {
                bobWeight = 1f;
            }
            else
            {
                DOTween.To(() => bobWeight, x => bobWeight = x, 1f, transitionDuration).SetEase(Ease.OutQuad).SetTarget(this);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!wasInteractable) return;
        
        isHovered = true;
        
        DOTween.Kill(this);
        DOTween.To(() => currentInteractionOffset, x => currentInteractionOffset = x, hoverOffset, transitionDuration).SetEase(Ease.OutQuad).SetTarget(this);
        DOTween.To(() => currentScale, x => currentScale = x, 0.95f, transitionDuration).SetEase(Ease.OutQuad).SetTarget(this);
        DOTween.To(() => bobWeight, x => bobWeight = x, 0f, transitionDuration).SetEase(Ease.OutQuad).SetTarget(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!wasInteractable) return;
        
        isHovered = false;
        if (!isPressed)
        {
            DOTween.Kill(this);
            DOTween.To(() => currentInteractionOffset, x => currentInteractionOffset = x, 0f, transitionDuration).SetEase(Ease.OutQuad).SetTarget(this);
            DOTween.To(() => currentScale, x => currentScale = x, 1f, transitionDuration).SetEase(Ease.OutQuad).SetTarget(this);
            DOTween.To(() => bobWeight, x => bobWeight = x, 1f, transitionDuration).SetEase(Ease.OutQuad).SetTarget(this);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!wasInteractable) return;
        
        isPressed = true;
        
        DOTween.Kill(this);
        DOTween.To(() => currentInteractionOffset, x => currentInteractionOffset = x, pressOffset, transitionDuration).SetEase(Ease.OutQuad).SetTarget(this);
        DOTween.To(() => currentScale, x => currentScale = x, 0.9f, transitionDuration).SetEase(Ease.OutQuad).SetTarget(this);
        DOTween.To(() => bobWeight, x => bobWeight = x, 0f, transitionDuration).SetEase(Ease.OutQuad).SetTarget(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!wasInteractable) return;
        
        isPressed = false;
        float targetOffset = isHovered ? hoverOffset : 0f;
        float targetScale = isHovered ? 0.95f : 1f;
        float targetBobWeight = isHovered ? 0f : 1f;
        
        DOTween.Kill(this);
        DOTween.To(() => currentInteractionOffset, x => currentInteractionOffset = x, targetOffset, transitionDuration).SetEase(Ease.OutQuad).SetTarget(this);
        DOTween.To(() => currentScale, x => currentScale = x, targetScale, transitionDuration).SetEase(Ease.OutQuad).SetTarget(this);
        DOTween.To(() => bobWeight, x => bobWeight = x, targetBobWeight, transitionDuration).SetEase(Ease.OutQuad).SetTarget(this);
    }

    private void OnDisable()
    {
        DOTween.Kill(this);
        isHovered = false;
        isPressed = false;
        currentInteractionOffset = 0f;
        currentScale = 1f;
        bobWeight = 1f;
        transform.localScale = Vector3.one;
        transform.localPosition = startPos;
    }
}

