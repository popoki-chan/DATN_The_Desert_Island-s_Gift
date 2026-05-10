using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }
    public GameObject tooltipRoot;
    public TextMeshProUGUI text;
    public Vector2 offset = new Vector2(10, -10);

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        Hide();
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
