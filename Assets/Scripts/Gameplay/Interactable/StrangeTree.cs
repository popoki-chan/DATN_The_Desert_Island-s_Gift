using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

[RequireComponent(typeof(Interactable))]
public class StrangeTree : MonoBehaviour
{
    [Header("Danh sách quả trên cây")]
    public List<GameObject> fruits;

    [Header("Số lần click để mọc lớn hoàn toàn")]
    public int maxClicks = 3;

    private int clickCount = 0;
    private List<Vector3> targetScales = new List<Vector3>();
    private Interactable interactable;

    void Awake()
    {
        interactable = GetComponent<Interactable>();
    }

    void Start()
    {
        // Lưu scale đích ban đầu và ẩn quả đi
        foreach (var fruit in fruits)
        {
            if (fruit != null)
            {
                targetScales.Add(fruit.transform.localScale);
                fruit.transform.localScale = Vector3.zero;

                // Tạm thời khóa tương tác nhặt quả
                var col = fruit.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;

                var fruitInteract = fruit.GetComponent<Interactable>();
                if (fruitInteract != null)
                {
                    fruitInteract.isLocked = true;
                }
            }
        }

        // Đăng ký sự kiện click cây
        if (interactable != null)
        {
            interactable.OnDefaultInteract += GrowFruits;
        }
    }

    void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.OnDefaultInteract -= GrowFruits;
        }
    }

    private void GrowFruits()
    {
        if (clickCount >= maxClicks) return;

        clickCount++;
        float ratio = (float)clickCount / maxClicks;

        // Rung cây nhẹ
        transform.DOShakePosition(0.3f, 0.1f, 10, 90f);

        // Quả mọc lớn lên từ từ
        for (int i = 0; i < fruits.Count; i++)
        {
            if (i < targetScales.Count && fruits[i] != null)
            {
                Vector3 nextScale = targetScales[i] * ratio;
                fruits[i].transform.DOKill();
                fruits[i].transform.DOScale(nextScale, 0.5f).SetEase(Ease.OutBack);
            }
        }

        Debug.Log($"[StrangeTree] Click {clickCount}/{maxClicks}. Quả lớn {ratio * 100}%.");

        // Khi quả đã chín hoàn toàn (lần click thứ 3)
        if (clickCount >= maxClicks)
        {
            UnlockFruits();
        }
    }

    private void UnlockFruits()
    {
        foreach (var fruit in fruits)
        {
            if (fruit != null)
            {
                // Bật va chạm để người chơi click hái được
                var col = fruit.GetComponent<Collider2D>();
                if (col != null) col.enabled = true;

                // Mở khóa tương tác hái quả
                var fruitInteract = fruit.GetComponent<Interactable>();
                if (fruitInteract != null)
                {
                    fruitInteract.isLocked = false;
                }
            }
        }

        // Khóa cây lại không cho tương tác tiếp
        if (interactable != null)
        {
            interactable.isLocked = true;
        }

        Debug.Log("[StrangeTree] Quả đã chín! Hãy hái quả.");
    }
}
