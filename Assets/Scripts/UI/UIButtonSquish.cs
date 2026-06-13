using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class UIButtonSquish : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Scale Settings")]
    [Tooltip("Target scale multiplier when button is pressed.")]
    [SerializeField] private Vector3 pressedScaleMultiplier = new Vector3(0.9f, 0.9f, 1f);
    
    [Tooltip("Duration of the press (shrink) animation.")]
    [SerializeField] private float pressDuration = 0.08f;
    
    [Tooltip("Duration of the release (bounce) animation.")]
    [SerializeField] private float releaseDuration = 0.35f;
    
    [Tooltip("Ease type for pressing down.")]
    [SerializeField] private Ease pressEase = Ease.OutQuad;
    
    [Tooltip("Ease type for releasing (bounce back).")]
    [SerializeField] private Ease releaseEase = Ease.OutBack;

    private Vector3 originalScale;
    private bool isPressed = false;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;
    }

    private void OnDisable()
    {
        ResetScale();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        isPressed = true;
        transform.DOKill(true); // Stop existing tweens and complete them
        
        Vector3 targetScale = Vector3.Scale(originalScale, pressedScaleMultiplier);
        transform.DOScale(targetScale, pressDuration)
            .SetEase(pressEase)
            .SetUpdate(true); // Ensure it plays even if Time.timeScale is 0 (e.g. paused settings popup)
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPressed) return;
        isPressed = false;

        transform.DOKill(true);
        transform.DOScale(originalScale, releaseDuration)
            .SetEase(releaseEase)
            .SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isPressed) return;
        isPressed = false;

        transform.DOKill(true);
        transform.DOScale(originalScale, pressDuration)
            .SetEase(pressEase)
            .SetUpdate(true);
    }

    private void ResetScale()
    {
        transform.DOKill(true);
        transform.localScale = originalScale;
        isPressed = false;
    }
}
