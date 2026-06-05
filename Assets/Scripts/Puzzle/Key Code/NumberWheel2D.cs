using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class NumberWheel2D : MonoBehaviour
{
    [Header("Liên kết với ổ khóa")]
    public CombinationLock lockManager; // THÊM CÁI NÀY: Để báo cho ổ khóa mỗi khi xoay

    [Tooltip("Đường dẫn trong Resources tới file sprite sheet (không có .png)")]
    public string resourcesPath = "Extra_assets/number_lock";

    [HideInInspector]
    public int currentNumber = 0;

    private SpriteRenderer numberRenderer;
    private Sprite[] digitSprites;

    void Awake()
    {
        numberRenderer = GetComponent<SpriteRenderer>();
        var sprites = Resources.LoadAll<Sprite>(resourcesPath);

        if (sprites != null && sprites.Length > 0)
        {
            digitSprites = sprites.OrderBy(s => s.name).ToArray();
        }
        UpdateDisplay();
    }

    public void Increment()
    {
        if (digitSprites == null || digitSprites.Length == 0) return;
        currentNumber = (currentNumber + 1) % digitSprites.Length;
        UpdateDisplay();
        if (lockManager != null) lockManager.CheckCode();
    }

    public void Decrement()
    {
        if (digitSprites == null || digitSprites.Length == 0) return;
        currentNumber = (currentNumber - 1 + digitSprites.Length) % digitSprites.Length;
        UpdateDisplay();
        if (lockManager != null) lockManager.CheckCode();
    }

    void UpdateDisplay()
    {
        if (numberRenderer != null && digitSprites != null && digitSprites.Length > 0)
        {
            numberRenderer.sprite = digitSprites[currentNumber];
        }
    }
}