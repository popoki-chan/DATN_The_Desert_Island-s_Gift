using UnityEngine;
using DG.Tweening;

public class CampfireIgnitionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MultiStepPuzzle multiStepPuzzle;
    [SerializeField] private SpriteRenderer lensRenderer;
    [SerializeField] private SpriteRenderer sunRayRenderer;
    [SerializeField] private SpriteRenderer focalGlowRenderer;
    [SerializeField] private SpriteRenderer smokeRenderer1;
    [SerializeField] private SpriteRenderer smokeRenderer2;
    [SerializeField] private SpriteRenderer smallFlameRenderer;

    [Header("Positions & Offsets")]
    [SerializeField] private Transform focusTarget; // Target position on the coir
    [SerializeField] private Vector3 lensOffset = new Vector3(0f, 0.6f, 0f);
    [SerializeField] private Vector3 lensStartOffset = new Vector3(1.5f, 1.5f, 0f);

    [Header("Timings")]
    [SerializeField] private float lensSpawnDuration = 0.8f;
    [SerializeField] private float sunRayFadeDuration = 0.8f;
    [SerializeField] private float focusDuration = 1.8f;
    [SerializeField] private float igniteDuration = 0.8f;
    [SerializeField] private float fadeOutDuration = 0.6f;

    [Header("SFX (Optional)")]
    [SerializeField] private AudioClip lensPlaceSfx;
    [SerializeField] private AudioClip sunFocusSfx;
    [SerializeField] private AudioClip fireIgniteSfx;

    private Sprite generatedGlowSprite;
    private Sprite generatedSmokeSprite;
    private Vector3 originalLensScale = Vector3.one;
    private Sequence ignitionSeq;

    private void Awake()
    {
        // Dynamically generate soft glow and smoke sprites to avoid missing asset issues
        generatedGlowSprite = CreateGlowSprite(new Color(1f, 0.95f, 0.7f, 1f));
        generatedSmokeSprite = CreateGlowSprite(new Color(0.9f, 0.9f, 0.9f, 1f));

        if (focalGlowRenderer != null) focalGlowRenderer.sprite = generatedGlowSprite;
        if (smokeRenderer1 != null) smokeRenderer1.sprite = generatedSmokeSprite;
        if (smokeRenderer2 != null) smokeRenderer2.sprite = generatedSmokeSprite;

        // Cache original scale and activate parent to ensure rendering works
        if (lensRenderer != null)
        {
            originalLensScale = lensRenderer.transform.localScale;
            if (lensRenderer.transform.parent != null && lensRenderer.transform.parent != transform)
            {
                lensRenderer.transform.parent.gameObject.SetActive(true);
            }
        }

        // Hide anim objects on start
        ResetVisuals();

        // Wire up ignition animation to the lens step in multiStepPuzzle
        if (multiStepPuzzle != null && multiStepPuzzle.steps != null)
        {
            foreach (var step in multiStepPuzzle.steps)
            {
                if (step.requiredItemId == "lens")
                {
                    step.onStepActivated.RemoveListener(PlayIgnitionAnimation);
                    step.onStepActivated.AddListener(PlayIgnitionAnimation);
                }
            }
        }
    }

    private void ResetVisuals()
    {
        if (lensRenderer != null) lensRenderer.gameObject.SetActive(false);
        if (sunRayRenderer != null) sunRayRenderer.gameObject.SetActive(false);
        if (focalGlowRenderer != null) focalGlowRenderer.gameObject.SetActive(false);
        if (smokeRenderer1 != null) smokeRenderer1.gameObject.SetActive(false);
        if (smokeRenderer2 != null) smokeRenderer2.gameObject.SetActive(false);
        if (smallFlameRenderer != null) smallFlameRenderer.gameObject.SetActive(false);
    }

    public void PlayIgnitionAnimation()
    {
        ResetVisuals();

        // Kill any previous animation to avoid ghost tweens
        ignitionSeq?.Kill();
        ignitionSeq = null;

        // 0. Temporary disable interaction on campfire
        Interactable interactable = null;
        if (multiStepPuzzle != null)
        {
            interactable = multiStepPuzzle.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.isLocked = true;
            }
        }

        ignitionSeq = DOTween.Sequence();
        Sequence seq = ignitionSeq;

        // 1. Spawn Lens (moving & scaling in)
        if (lensRenderer != null && focusTarget != null)
        {
            Vector3 targetLensPos = focusTarget.position + lensOffset;
            Vector3 startLensPos = targetLensPos + lensStartOffset;

            lensRenderer.transform.position = startLensPos;
            lensRenderer.transform.localScale = Vector3.zero;
            lensRenderer.color = new Color(1f, 1f, 1f, 0f);
            lensRenderer.gameObject.SetActive(true);

            seq.Append(lensRenderer.transform.DOMove(targetLensPos, lensSpawnDuration).SetEase(Ease.OutBack));
            seq.Join(lensRenderer.transform.DOScale(originalLensScale, lensSpawnDuration).SetEase(Ease.OutBack));
            seq.Join(lensRenderer.DOFade(1f, lensSpawnDuration));

            if (lensPlaceSfx != null && AudioManager.Instance != null)
            {
                seq.AppendCallback(() => AudioManager.Instance.PlaySFX(lensPlaceSfx));
            }
        }

        // 2. Fade in Sun Ray
        if (sunRayRenderer != null)
        {
            sunRayRenderer.color = new Color(1f, 1f, 1f, 0f);
            sunRayRenderer.gameObject.SetActive(true);

            seq.Append(sunRayRenderer.DOFade(0.75f, sunRayFadeDuration));
            if (sunFocusSfx != null && AudioManager.Instance != null)
            {
                seq.Join(DOVirtual.DelayedCall(0.1f, () => AudioManager.Instance.PlaySFX(sunFocusSfx)));
            }
        }

        // 3. Focal point glow & smoke rising
        if (focalGlowRenderer != null)
        {
            focalGlowRenderer.transform.localScale = Vector3.zero;
            focalGlowRenderer.color = new Color(1f, 0.95f, 0.7f, 0f);
            focalGlowRenderer.gameObject.SetActive(true);

            seq.Append(focalGlowRenderer.transform.DOScale(0.8f, focusDuration).SetEase(Ease.InQuad));
            seq.Join(focalGlowRenderer.DOFade(1f, focusDuration));

            // Pulsate focal glow at the end
            seq.Append(focalGlowRenderer.transform.DOScale(0.5f, 0.15f).SetLoops(6, LoopType.Yoyo));
        }

        // Animate Smoke 1 rising
        if (smokeRenderer1 != null && focusTarget != null)
        {
            smokeRenderer1.color = new Color(1f, 1f, 1f, 0f);
            smokeRenderer1.transform.position = focusTarget.position;
            smokeRenderer1.transform.localScale = Vector3.one * 0.2f;
            smokeRenderer1.gameObject.SetActive(true);

            // Starts halfway through focusDuration
            float smokeStartDelay = lensSpawnDuration + sunRayFadeDuration + (focusDuration * 0.4f);
            seq.Insert(smokeStartDelay, smokeRenderer1.DOFade(0.5f, 0.4f));
            seq.Insert(smokeStartDelay, smokeRenderer1.transform.DOMoveY(focusTarget.position.y + 0.6f, 1.2f).SetEase(Ease.OutQuad));
            seq.Insert(smokeStartDelay, smokeRenderer1.transform.DOMoveX(focusTarget.position.x - 0.2f, 1.2f).SetEase(Ease.OutQuad));
            seq.Insert(smokeStartDelay, smokeRenderer1.transform.DOScale(0.6f, 1.2f));
            seq.Insert(smokeStartDelay + 0.8f, smokeRenderer1.DOFade(0f, 0.4f));
        }

        // Animate Smoke 2 rising (slightly delayed and drifted to another side)
        if (smokeRenderer2 != null && focusTarget != null)
        {
            smokeRenderer2.color = new Color(1f, 1f, 1f, 0f);
            smokeRenderer2.transform.position = focusTarget.position;
            smokeRenderer2.transform.localScale = Vector3.one * 0.2f;
            smokeRenderer2.gameObject.SetActive(true);

            float smoke2StartDelay = lensSpawnDuration + sunRayFadeDuration + (focusDuration * 0.7f);
            seq.Insert(smoke2StartDelay, smokeRenderer2.DOFade(0.5f, 0.4f));
            seq.Insert(smoke2StartDelay, smokeRenderer2.transform.DOMoveY(focusTarget.position.y + 0.7f, 1.2f).SetEase(Ease.OutQuad));
            seq.Insert(smoke2StartDelay, smokeRenderer2.transform.DOMoveX(focusTarget.position.x + 0.2f, 1.2f).SetEase(Ease.OutQuad));
            seq.Insert(smoke2StartDelay, smokeRenderer2.transform.DOScale(0.7f, 1.2f));
            seq.Insert(smoke2StartDelay + 0.8f, smokeRenderer2.DOFade(0f, 0.4f));
        }

        // 4. Ignite small flame on the coir
        if (smallFlameRenderer != null)
        {
            smallFlameRenderer.transform.localScale = Vector3.zero;
            smallFlameRenderer.gameObject.SetActive(true);

            seq.Append(smallFlameRenderer.transform.DOScale(0.5f, igniteDuration).SetEase(Ease.OutElastic));
            if (fireIgniteSfx != null && AudioManager.Instance != null)
            {
                seq.Join(DOVirtual.DelayedCall(0.1f, () => AudioManager.Instance.PlaySFX(fireIgniteSfx)));
            }
        }

        // 5. Complete Step (turns on full fire)
        seq.AppendCallback(() =>
        {
            if (multiStepPuzzle != null)
            {
                multiStepPuzzle.CompleteCurrentStep();
            }
        });

        // 6. Smoothly fade out temporary elements
        Sequence fadeOutSeq = DOTween.Sequence();
        if (lensRenderer != null) fadeOutSeq.Join(lensRenderer.DOFade(0f, fadeOutDuration));
        if (sunRayRenderer != null) fadeOutSeq.Join(sunRayRenderer.DOFade(0f, fadeOutDuration));
        if (focalGlowRenderer != null) fadeOutSeq.Join(focalGlowRenderer.DOFade(0f, fadeOutDuration));
        if (smallFlameRenderer != null) fadeOutSeq.Join(smallFlameRenderer.DOFade(0f, fadeOutDuration));

        fadeOutSeq.OnComplete(() =>
        {
            ResetVisuals();
        });

        seq.Append(fadeOutSeq);
    }

    private Sprite CreateGlowSprite(Color baseColor)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size / 2f;
        float maxDist = size / 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = Mathf.Clamp01(1f - (dist / maxDist));
                // Quadratic falloff for smooth gradient
                alpha = Mathf.Pow(alpha, 2f); 
                
                tex.SetPixel(x, y, new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * alpha));
            }
        }
        tex.Apply();
        
        // Disable texture filtering issues
        tex.wrapMode = TextureWrapMode.Clamp;
        
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private void OnDestroy()
    {
        // Cleanup generated textures to prevent memory leaks
        if (generatedGlowSprite != null && generatedGlowSprite.texture != null)
        {
            Destroy(generatedGlowSprite.texture);
        }
        if (generatedSmokeSprite != null && generatedSmokeSprite.texture != null)
        {
            Destroy(generatedSmokeSprite.texture);
        }

        // Kill running tweens
        ignitionSeq?.Kill();
        ignitionSeq = null;

        // Cleanup listener
        if (multiStepPuzzle != null && multiStepPuzzle.steps != null)
        {
            foreach (var step in multiStepPuzzle.steps)
            {
                if (step.requiredItemId == "lens")
                {
                    step.onStepActivated.RemoveListener(PlayIgnitionAnimation);
                }
            }
        }
    }
}
