using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class UIButtonAudio : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("UI SFX Clips")]
    [SerializeField] private AudioClip hoverSfx;
    [SerializeField] private AudioClip clickSfx;

    [Header("Custom Volume Settings")]
    [Range(0f, 1f)] [SerializeField] private float volumeScale = 1f;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Chỉ phát âm thanh hover nếu nút có thể tương tác (interactable)
        if (button != null && button.interactable && hoverSfx != null)
        {
            AudioManager.Instance?.PlaySFX(hoverSfx, volumeScale);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (button != null && button.interactable && clickSfx != null)
        {
            AudioManager.Instance?.PlaySFX(clickSfx, volumeScale);
        }
    }
}
