using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TooltipUI : Singleton<TooltipUI>
{
    public GameObject tooltipRoot;
    public TextMeshProUGUI text;
    public Vector2 offset = new Vector2(10, -10);

    protected override void Awake()
    {
        DontDestroyOnLoadEnabled = false;
        base.Awake();
        if (Instance == this)
        {
            Hide();
        }
    }

    void Update()
    {
        if (tooltipRoot.activeSelf)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)transform, Input.mousePosition, null, out pos);
            tooltipRoot.GetComponent<RectTransform>().anchoredPosition = pos + offset;
        }
    }

    public void Show(string content)
    {
        if (string.IsNullOrEmpty(content)) { Hide(); return; }
        tooltipRoot.SetActive(true);
        text.text = content;
    }

    public void Hide()
    {
        if (tooltipRoot != null) tooltipRoot.SetActive(false);
    }
}
