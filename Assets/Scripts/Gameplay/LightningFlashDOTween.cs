using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class LightningFlashDOTween : MonoBehaviour
{
    [Header("Interval between lightning flashes")]
    public float minInterval = 3f;
    public float maxInterval = 8f;

    [Header("Visual settings")]
    [Range(0f, 1f)]
    public float maxAlpha = 1f;

    private RawImage rawImage;
    private Color baseColor;
    private Coroutine flashCoroutine;
    private Sequence flashSequence;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        if (rawImage != null)
        {
            baseColor = rawImage.color;
            // Set initial alpha to 0 so it starts invisible
            SetAlpha(0f);
        }
    }

    private void OnEnable()
    {
        if (rawImage != null)
        {
            SetAlpha(0f);
            flashCoroutine = StartCoroutine(LightningLoop());
        }
    }

    private void OnDisable()
    {
        StopFlash();
    }

    private void OnDestroy()
    {
        StopFlash();
    }

    private void StopFlash()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        if (flashSequence != null)
        {
            flashSequence.Kill();
            flashSequence = null;
        }
        if (rawImage != null)
        {
            SetAlpha(0f);
        }
    }

    private void SetAlpha(float alpha)
    {
        if (rawImage != null)
        {
            Color c = baseColor;
            c.a = alpha * baseColor.a; // Scale against the editor color's alpha
            rawImage.color = c;
        }
    }

    private IEnumerator LightningLoop()
    {
        // Initial delay so it doesn't flash immediately upon entering Cut2
        yield return new WaitForSeconds(Random.Range(1f, 3f));

        while (true)
        {
            TriggerLightningFlash();
            
            // Wait for the next flash interval
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void TriggerLightningFlash()
    {
        if (rawImage == null) return;

        // Kill any ongoing sequence
        if (flashSequence != null)
        {
            flashSequence.Kill();
        }

        flashSequence = DOTween.Sequence();
        
        // Randomize lightning pattern
        int pattern = Random.Range(0, 3);
        
        if (pattern == 0)
        {
            // Pattern 0: Single short flash
            flashSequence.Append(DOTween.To(() => 0f, SetAlpha, maxAlpha, 0.05f).SetEase(Ease.OutQuad));
            flashSequence.Append(DOTween.To(() => maxAlpha, SetAlpha, 0f, 0.25f).SetEase(Ease.InQuad));
        }
        else if (pattern == 1)
        {
            // Pattern 1: Double flash (classic lightning strike)
            flashSequence.Append(DOTween.To(() => 0f, SetAlpha, maxAlpha, 0.04f).SetEase(Ease.OutQuad));
            flashSequence.Append(DOTween.To(() => maxAlpha, SetAlpha, 0.1f, 0.1f).SetEase(Ease.InQuad));
            flashSequence.Append(DOTween.To(() => 0.1f, SetAlpha, maxAlpha * 0.8f, 0.03f).SetEase(Ease.OutQuad));
            flashSequence.Append(DOTween.To(() => maxAlpha * 0.8f, SetAlpha, 0f, 0.4f).SetEase(Ease.OutQuad));
        }
        else
        {
            // Pattern 2: Triple stutter flash (dramatic storm)
            flashSequence.Append(DOTween.To(() => 0f, SetAlpha, maxAlpha * 0.7f, 0.05f).SetEase(Ease.OutQuad));
            flashSequence.Append(DOTween.To(() => maxAlpha * 0.7f, SetAlpha, 0.2f, 0.08f).SetEase(Ease.Linear));
            flashSequence.Append(DOTween.To(() => 0.2f, SetAlpha, maxAlpha, 0.04f).SetEase(Ease.OutQuad));
            flashSequence.Append(DOTween.To(() => maxAlpha, SetAlpha, 0.15f, 0.12f).SetEase(Ease.Linear));
            flashSequence.Append(DOTween.To(() => 0.15f, SetAlpha, maxAlpha * 0.9f, 0.03f).SetEase(Ease.OutQuad));
            flashSequence.Append(DOTween.To(() => maxAlpha * 0.9f, SetAlpha, 0f, 0.55f).SetEase(Ease.OutQuad));
        }
    }
}
