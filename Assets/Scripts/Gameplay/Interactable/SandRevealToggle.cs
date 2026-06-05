using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class SandRevealToggle : MonoBehaviour
{
    [Header("Các object sẽ hiện ra/ẩn đi")]
    public GameObject[] revealObjects; // kéo các object nằm dưới vào đây (có thể inactive)

    [Header("Hiệu ứng")]
    public float fadeDuration = 0.35f;
    public bool useJump = false;
    public float jumpHeight = 0.15f;
    public float jumpDuration = 0.18f;

    [Header("Âm thanh")]
    public AudioClip sandSfx;
    public float sfxVolume = 1f;

    [Header("Hành vi")]
    public bool startHidden = false;
    public bool disableColliderWhenHidden = true;

    SpriteRenderer sr;
    Collider2D col;
    bool isHidden = false;
    Color originalColor;
    Vector3 originalPos;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        if (sr != null) originalColor = sr.color;
        originalPos = transform.position;

        if (startHidden)
            SetHiddenState(true, instant: true);
        else
            SetHiddenState(false, instant: true);
    }

    void OnMouseDown()
    {
        if (SettingsPopupController.IsOpen) return;
        if (IsPointerOverUI()) return;
        ToggleReveal();
    }

    void Update()
    {
        if (SettingsPopupController.IsOpen) return;
        if (Input.touchCount > 0)
        {
            foreach (Touch t in Input.touches)
            {
                if (t.phase == TouchPhase.Began)
                {
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId)) continue;
                    Vector2 wp = Camera.main.ScreenToWorldPoint(t.position);
                    Collider2D hit = Physics2D.OverlapPoint(wp);
                    if (hit == col) ToggleReveal();
                }
            }
        }
    }

    public void ToggleReveal()
    {
        if (sandSfx != null)
            AudioSource.PlayClipAtPoint(sandSfx, Camera.main.transform.position, sfxVolume);
        if (isHidden)
            StartCoroutine(FadeInAndHideReveals());
        else
            StartCoroutine(FadeOutAndShowReveals());
    }

    void SetHiddenState(bool hidden, bool instant = false)
    {
        isHidden = hidden;
        if (sr != null)
        {
            Color c = originalColor;
            c.a = hidden ? 0f : originalColor.a;
            sr.color = c;
            sr.enabled = !hidden || !instant ? sr.enabled : !hidden;
        }
        if (col != null)
            col.enabled = !(hidden && disableColliderWhenHidden);

        foreach (var go in revealObjects)
        {
            if (go == null) continue;
            foreach (var c in go.GetComponentsInChildren<Collider2D>(true))
            {
                c.enabled = hidden;
            }
            foreach (var interact in go.GetComponentsInChildren<Interactable>(true))
            {
                interact.enabled = hidden;
            }
        }
    }

    IEnumerator FadeOutAndShowReveals()
    {
        if (useJump)
            yield return StartCoroutine(JumpCoroutine(jumpHeight, jumpDuration));
        if (sr != null)
        {
            float elapsed = 0f;
            Color start = sr.color;
            while (elapsed < fadeDuration)
            {
                float t = elapsed / fadeDuration;
                Color c = start;
                c.a = Mathf.Lerp(start.a, 0f, t);
                sr.color = c;
                elapsed += Time.deltaTime;
                yield return null;
            }
            Color end = start;
            end.a = 0f;
            sr.color = end;
        }
        if (col != null && disableColliderWhenHidden) col.enabled = false;
        foreach (var go in revealObjects)
        {
            if (go == null) continue;
            foreach (var c in go.GetComponentsInChildren<Collider2D>(true))
            {
                c.enabled = true;
            }
            foreach (var interact in go.GetComponentsInChildren<Interactable>(true))
            {
                interact.enabled = true;
            }
        }

        isHidden = true;
    }

    IEnumerator FadeInAndHideReveals()
    {
        foreach (var go in revealObjects)
        {
            if (go == null) continue;
            foreach (var c in go.GetComponentsInChildren<Collider2D>(true))
            {
                c.enabled = false;
            }
            foreach (var interact in go.GetComponentsInChildren<Interactable>(true))
            {
                interact.enabled = false;
            }
        }
        if (col != null) col.enabled = true;
        if (sr != null)
        {
            float elapsed = 0f;
            Color start = sr.color;
            float startAlpha = start.a;
            while (elapsed < fadeDuration)
            {
                float t = elapsed / fadeDuration;
                Color c = start;
                c.a = Mathf.Lerp(startAlpha, originalColor.a, t);
                sr.color = c;
                elapsed += Time.deltaTime;
                yield return null;
            }
            Color end = originalColor;
            end.a = originalColor.a;
            sr.color = end;
        }

        isHidden = false;
    }

    IEnumerator JumpCoroutine(float height, float duration)
    {
        Vector3 start = originalPos;
        Vector3 peak = start + Vector3.up * height;
        float half = duration * 0.5f;
        float t = 0f;
        while (t < half)
        {
            float p = t / half;
            transform.position = Vector3.Lerp(start, peak, Mathf.Sin(p * Mathf.PI * 0.5f));
            t += Time.deltaTime;
            yield return null;
        }
        t = 0f;
        while (t < half)
        {
            float p = t / half;
            transform.position = Vector3.Lerp(peak, start, 1f - Mathf.Cos(p * Mathf.PI * 0.5f));
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = start;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        if (EventSystem.current.IsPointerOverGameObject()) return true;
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                return true;
        }
        return false;
    }
}
