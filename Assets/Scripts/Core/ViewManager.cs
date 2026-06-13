using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewManager : Singleton<ViewManager>
{
    [Header("Cài đặt View (Góc nhìn)")]
    [Tooltip("Góc nhìn tổng của căn phòng lúc mới vào game")]
    public GameObject mainView;

    [Tooltip("Kéo TẤT CẢ các View (kể cả mainView) vào đây để hệ thống tự quản lý tắt/bật")]
    public GameObject[] allViews;

    [Header("UI Button")]
    [Tooltip("Nút mũi tên quay xuống (Back)")]
    public Button backButton;
    [Tooltip("Kéo Object nút Mũi tên Trái vào đây")]
    public GameObject leftArrowButton;
    [Tooltip("Kéo Object nút Mũi tên Phải vào đây")]
    public GameObject rightArrowButton;

    private Stack<GameObject> viewHistory = new Stack<GameObject>();

    protected override void Awake()
    {
        DontDestroyOnLoadEnabled = false;
        base.Awake();
    }

    private void Start()
    {
        if (allViews != null)
        {
            foreach (GameObject view in allViews)
            {
                if (view != null) view.SetActive(false);
            }
        }
        
        if (mainView != null)
        {
            mainView.SetActive(true);
            viewHistory.Push(mainView);
        }

        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(GoBack);
        }

        UpdateArrowsVisibility();
    }

    public void ChangeView(GameObject targetView)
    {
        if (targetView == null) return;
        if (viewHistory.Count > 0 && viewHistory.Peek() == targetView) return;

        StartCoroutine(TransitionToView(targetView));
    }

    public void GoBack()
    {
        if (viewHistory.Count > 1)
        {
            StartCoroutine(TransitionBack());
        }
    }

    private void UpdateArrowsVisibility()
    {
        bool isAtMainRoom = (viewHistory.Count > 0 && viewHistory.Peek() == mainView);

        if (leftArrowButton != null) leftArrowButton.SetActive(isAtMainRoom);
        if (rightArrowButton != null) rightArrowButton.SetActive(isAtMainRoom);
    }

    private IEnumerator TransitionToView(GameObject targetView)
    {
        float duration = 0.15f;
        
        if (SceneController.Instance != null)
        {
            SceneController.Instance.FadeTo(1f, duration);
            yield return new WaitForSeconds(duration);
        }

        if (viewHistory.Count > 0)
        {
            viewHistory.Peek().SetActive(false);
        }

        targetView.SetActive(true);
        viewHistory.Push(targetView);
        backButton.gameObject.SetActive(true);
        UpdateArrowsVisibility();

        yield return null;

        if (SceneController.Instance != null)
        {
            SceneController.Instance.FadeTo(0f, duration);
            yield return new WaitForSeconds(duration);
        }
    }

    private IEnumerator TransitionBack()
    {
        float duration = 0.15f;

        if (SceneController.Instance != null)
        {
            SceneController.Instance.FadeTo(1f, duration);
            yield return new WaitForSeconds(duration);
        }

        GameObject currentView = viewHistory.Pop();
        currentView.SetActive(false);

        GameObject previousView = viewHistory.Peek();
        previousView.SetActive(true);

        if (viewHistory.Count == 1)
        {
            backButton.gameObject.SetActive(false);
        }
        UpdateArrowsVisibility();

        yield return null;

        if (SceneController.Instance != null)
        {
            SceneController.Instance.FadeTo(0f, duration);
            yield return new WaitForSeconds(duration);
        }
    }
}