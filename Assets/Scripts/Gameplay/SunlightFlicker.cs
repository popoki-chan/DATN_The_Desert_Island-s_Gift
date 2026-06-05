using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SunlightFlicker : MonoBehaviour
{
    [Header("Cấu hình lấp ló")]
    public float minAlpha = 0.15f;
    public float maxAlpha = 0.6f;
    public float pulseSpeed = 1.5f;

    private SpriteRenderer sr;
    private float timer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (sr == null) return;

        timer += Time.deltaTime * pulseSpeed;
        float sinVal = (Mathf.Sin(timer) + 1f) * 0.5f;
        Color c = sr.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, sinVal);
        sr.color = c;
    }
}
