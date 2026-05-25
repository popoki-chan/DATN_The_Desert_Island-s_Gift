using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewManager : MonoBehaviour
{
    public static ViewManager Instance { get; private set; }

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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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

        // Tắt góc nhìn hiện tại
        viewHistory.Peek().SetActive(false);

        // Bật góc nhìn mới và đưa vào ngăn xếp lịch sử
        targetView.SetActive(true);
        viewHistory.Push(targetView);

        // Hiện nút Back
        backButton.gameObject.SetActive(true);

        // Ẩn/Hiện mũi tên trái phải tùy theo góc nhìn mới
        UpdateArrowsVisibility();
    }

    public void GoBack()
    {
        // Chỉ cho phép lùi nếu trong lịch sử có nhiều hơn 1 view (nghĩa là không phải mainView)
        if (viewHistory.Count > 1)
        {
            // Lấy view hiện tại ra khỏi lịch sử và tắt nó đi
            GameObject currentView = viewHistory.Pop();
            currentView.SetActive(false);

            // Bật lại view trước đó
            GameObject previousView = viewHistory.Peek();
            previousView.SetActive(true);

            // Nếu lùi về đến mainView rồi thì giấu nút Back đi
            if (viewHistory.Count == 1)
            {
                backButton.gameObject.SetActive(false);
            }

            // Ẩn/Hiện mũi tên trái phải khi lùi ra
            UpdateArrowsVisibility();
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
}