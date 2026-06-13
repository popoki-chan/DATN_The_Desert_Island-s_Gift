using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
public class FireFlickerDOTween : MonoBehaviour
{
    [Header("Scale Flicker Settings")]
    public float scaleSpeed = 0.1f; // Duration of each scale change
    public Vector3 minScale = new Vector3(0.9f, 0.95f, 1f);
    public Vector3 maxScale = new Vector3(1.1f, 1.15f, 1f);

    [Header("Color/Alpha Flicker Settings")]
    public float colorSpeed = 0.08f; // Duration of each color/alpha change
    public float minAlpha = 0.82f;
    public float maxAlpha = 1.0f;
    public Color fireColor1 = new Color(1f, 1f, 1f, 1f); // Base white sprite color
    public Color fireColor2 = new Color(1f, 0.92f, 0.8f, 1f); // Warm flame tint

    [Header("Position Jitter Settings")]
    public float jitterSpeed = 0.07f; // Duration of each position change
    public float jitterAmountX = 0.03f;
    public float jitterAmountY = 0.02f;

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Sequence flickerSequence;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        StartFlicker();
    }

    private void OnDisable()
    {
        StopFlicker();
    }

    private void OnDestroy()
    {
        StopFlicker();
    }

    private void StartFlicker()
    {
        StopFlicker();

        flickerSequence = DOTween.Sequence();

        // 1. Scale animation loop
        flickerSequence.Join(
            transform.DOScale(MultiplyVectors(originalScale, maxScale), scaleSpeed)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
        );

        // 2. Position jitter animation loop
        flickerSequence.Join(
            transform.DOLocalMove(originalPosition + new Vector3(jitterAmountX, jitterAmountY, 0f), jitterSpeed)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
        );

        // 3. Color/Alpha animation loop
        if (spriteRenderer != null)
        {
            // Flickering alpha and color tint
            flickerSequence.Join(
                DOTween.To(() => minAlpha, SetAlphaAndColor, maxAlpha, colorSpeed)
                    .SetEase(Ease.InOutQuad)
                    .SetLoops(-1, LoopType.Yoyo)
            );
        }
    }

    private void StopFlicker()
    {
        if (flickerSequence != null)
        {
            flickerSequence.Kill();
            flickerSequence = null;
        }

        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    private void SetAlphaAndColor(float alphaValue)
    {
        if (spriteRenderer == null) return;
        
        // Interpolate between fireColor1 and fireColor2 based on current alpha wave
        float t = Mathf.InverseLerp(minAlpha, maxAlpha, alphaValue);
        Color targetColor = Color.Lerp(fireColor2, fireColor1, t);
        targetColor.a = alphaValue;
        
        spriteRenderer.color = targetColor;
    }

    private Vector3 MultiplyVectors(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }
}
