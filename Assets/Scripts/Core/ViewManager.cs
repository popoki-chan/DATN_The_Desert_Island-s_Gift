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

    // Dùng Stack (ngăn xếp) để nhớ lịch sử zoom. Bấm Back sẽ lùi lại đúng 1 bước.
    private Stack<GameObject> viewHistory = new Stack<GameObject>();

    protected override void Awake()
    {
        DontDestroyOnLoadEnabled = false;
        base.Awake();
    }

    private void Start()
    {
        // 1. Tắt hết tất cả các góc nhìn
        foreach (GameObject view in allViews)
        {
            if (view != null) view.SetActive(false);
        }

        // 2. Bật góc nhìn chính lên và lưu vào lịch sử
        if (mainView != null)
        {
            mainView.SetActive(true);
            viewHistory.Push(mainView);
        }

        // 3. Setup nút Back
        backButton.gameObject.SetActive(false); // Ẩn nút Back lúc ở phòng chính
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(GoBack);

        // 4. Setup trạng thái ban đầu cho 2 mũi tên trái/phải
        UpdateArrowsVisibility();
    }

    // Hàm này sẽ được gọi khi bạn click vào đồ vật muốn Zoom
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

    // --- HÀM MỚI: TỰ ĐỘNG KIỂM TRA ĐỂ BẬT/TẮT MŨI TÊN ---
    private void UpdateArrowsVisibility()
    {
        // Kiểm tra xem view trên cùng của ngăn xếp có khớp với mainView không
        bool isAtMainRoom = (viewHistory.Count > 0 && viewHistory.Peek() == mainView);

        // Nếu đang ở Main View -> Hiện mũi tên. Bất kỳ View nào khác -> Ẩn mũi tên.
        if (leftArrowButton != null) leftArrowButton.SetActive(isAtMainRoom);
        if (rightArrowButton != null) rightArrowButton.SetActive(isAtMainRoom);
    }

    private IEnumerator TransitionToView(GameObject targetView)
    {
        float duration = 0.15f;
        
        // 1. Fade out (làm tối màn hình)
        if (SceneController.Instance != null)
        {
            SceneController.Instance.FadeTo(1f, duration);
            yield return new WaitForSeconds(duration);
        }

        // 2. Chuyển đổi trạng thái View khi màn hình tối
        if (viewHistory.Count > 0)
        {
            viewHistory.Peek().SetActive(false);
        }

        targetView.SetActive(true);
        viewHistory.Push(targetView);
        backButton.gameObject.SetActive(true);
        UpdateArrowsVisibility();

        // 3. Đợi 1 frame để render xong view mới
        yield return null;

        // 4. Fade in (làm sáng màn hình)
        if (SceneController.Instance != null)
        {
            SceneController.Instance.FadeTo(0f, duration);
            yield return new WaitForSeconds(duration);
        }
    }

    private IEnumerator TransitionBack()
    {
        float duration = 0.15f;

        // 1. Fade out (làm tối màn hình)
        if (SceneController.Instance != null)
        {
            SceneController.Instance.FadeTo(1f, duration);
            yield return new WaitForSeconds(duration);
        }

        // 2. Chuyển đổi trạng thái View khi màn hình tối
        GameObject currentView = viewHistory.Pop();
        currentView.SetActive(false);

        GameObject previousView = viewHistory.Peek();
        previousView.SetActive(true);

        if (viewHistory.Count == 1)
        {
            backButton.gameObject.SetActive(false);
        }
        UpdateArrowsVisibility();

        // 3. Đợi 1 frame để render xong view mới
        yield return null;

        // 4. Fade in (làm sáng màn hình)
        if (SceneController.Instance != null)
        {
            SceneController.Instance.FadeTo(0f, duration);
            yield return new WaitForSeconds(duration);
        }
    }
}