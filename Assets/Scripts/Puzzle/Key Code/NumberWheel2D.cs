using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]

public class NumberWheel2D : MonoBehaviour
{
    private SpriteRenderer numberRenderer;
    private Sprite[] digitSprites;
    [Tooltip("Đường dẫn trong Resources tới file sprite sheet (không có .png)")]
    public string resourcesPath = "Extra_assets/number_lock";
    [HideInInspector]
    public int currentNumber = 0;

    void Awake()
    {
        numberRenderer = GetComponent<SpriteRenderer>();

        // Load tất cả sprite con từ sprite sheet đã slice
        var sprites = Resources.LoadAll<Sprite>(resourcesPath);
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"[LockWheel2D] Không load được sprites từ Resources/{resourcesPath}");
            digitSprites = new Sprite[0];
        }
        else
        {
            // Sort theo tên để đảm bảo thứ tự 0->9 nếu tên là number_lock_0...
            digitSprites = sprites.OrderBy(s => s.name).ToArray();
        }

        UpdateDisplay();
    }

    public void Increment()
    {
        if (digitSprites == null || digitSprites.Length == 0) return;
        currentNumber = (currentNumber + 1) % digitSprites.Length;
        UpdateDisplay();
    }

    public void Decrement()
    {
        if (digitSprites == null || digitSprites.Length == 0) return;
        currentNumber = (currentNumber - 1 + digitSprites.Length) % digitSprites.Length;
        UpdateDisplay();
    }

    public void SetNumber(int num)
    {
        if (digitSprites == null || digitSprites.Length == 0) return;
        currentNumber = Mathf.Clamp(num, 0, digitSprites.Length - 1);
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (numberRenderer == null) return;
        if (digitSprites != null && digitSprites.Length > 0)
            numberRenderer.sprite = digitSprites[currentNumber];
    }
}