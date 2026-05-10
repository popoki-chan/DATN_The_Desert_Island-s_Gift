using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class SandRevealToggle : MonoBehaviour
{
    [Header("Các object sẽ hiện ra/ẩn đi")]
    public GameObject[] revealObjects; // kéo các object nằm dưới vào đây (có thể inactive)

    [Header("Hiệu ứng")]
    public float fadeDuration = 0.35f;
    public bool useJump = false; // nhún nhẹ trước khi fade
    public float jumpHeight = 0.15f;
    public float jumpDuration = 0.18f;

    [Header("Âm thanh")]
    public AudioClip sandSfx;
    public float sfxVolume = 1f;

    [Header("Hành vi")]
    public bool startHidden = false; // nếu true thì cát bắt đầu ẩn (vật dưới hiện)
    public bool disableColliderWhenHidden = true;

    // internal
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

        // khởi tạo trạng thái ban đầu
        if (startHidden)
            SetHiddenState(true, instant: true);
        else
            SetHiddenState(false, instant: true);
    }

    void OnMouseDown()
    {
        ToggleReveal();
    }

    void Update()
    {
        // Touch support
        if (Input.touchCount > 0)
        {
            foreach (Touch t in Input.touches)
            {
                if (t.phase == TouchPhase.Began)
                {
                    Vector2 wp = Camera.main.ScreenToWorldPoint(t.position);
                    Collider2D hit = Physics2D.OverlapPoint(wp);
                    if (hit == col) ToggleReveal();
                }
            }
        }
    }

    public void ToggleReveal()
    {
        // play sfx
        if (sandSfx != null)
            AudioSource.PlayClipAtPoint(sandSfx, Camera.main.transform.position, sfxVolume);

        // nếu đang ẩn thì hiện lại, ngược lại ẩn
        if (isHidden)
            StartCoroutine(FadeInAndHideReveals());
        else
            StartCoroutine(FadeOutAndShowReveals());
    }

    // Set trạng thái ngay lập tức (dùng ở Awake hoặc reset)
    void SetHiddenState(bool hidden, bool instant = false)
    {
        isHidden = hidden;
        if (sr != null)
        {
            Color c = originalColor;
            c.a = hidden ? 0f : originalColor.a;
            sr.color = c;
            sr.enabled = !hidden || !instant ? sr.enabled : !hidden; // đảm bảo visible nếu not hidden
        }
        if (col != null)
            col.enabled = !(hidden && disableColliderWhenHidden);

        foreach (var go in revealObjects)
        {
            if (go == null) continue;
            go.SetActive(hidden ? true : false);
        }
    }

    IEnumerator FadeOutAndShowReveals()
    {
        // optional jump feedback
        if (useJump)
            yield return StartCoroutine(JumpCoroutine(jumpHeight, jumpDuration));

        // fade out cát
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

        // disable collider nếu cần
        if (col != null && disableColliderWhenHidden) col.enabled = false;

        // bật các object nằm dưới
        foreach (var go in revealObjects)
        {
            if (go == null) continue;
            go.SetActive(true);
        }

        isHidden = true;
    }

    IEnumerator FadeInAndHideReveals()
    {
        // tắt các object nằm dưới trước khi fade in cát (nếu muốn)
        foreach (var go in revealObjects)
        {
            if (go == null) continue;
            go.SetActive(false);
        }

        // enable collider trước khi fade in (để có thể click nếu cần)
        if (col != null) col.enabled = true;

        // fade in cát
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
}
