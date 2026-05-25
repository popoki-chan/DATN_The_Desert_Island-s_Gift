using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Các Bảng UI (Panels)")]
    public GameObject startPanel;   // Bảng chứa nút Play ban đầu
    public GameObject chapterPanel; // Bảng chứa 3 nút chọn Chapter

    [Header("Nút bấm các Chapter")]
    [Tooltip("Kéo lần lượt 3 nút Chapter 1, 2, 3 vào đây")]
    public Button[] chapterButtons;

    [Header("Icon Ổ khóa (Tùy chọn)")]
    [Tooltip("Kéo các icon ổ khóa tương ứng với từng nút vào đây để ẩn/hiện")]
    public GameObject[] lockIcons;

    void Start()
    {
        // Khi mới vào game, chắc chắn hiển thị Start Panel, ẩn Chapter Panel
        startPanel.SetActive(true);
        chapterPanel.SetActive(false);
    }

    // Gắn hàm này vào sự kiện OnClick của nút "Play"
    public void OpenChapterSelection()
    {
        startPanel.SetActive(false);
        chapterPanel.SetActive(true);

        CheckUnlockedChapters(); // Bắt đầu kiểm tra ổ khóa
    }

    private void CheckUnlockedChapters()
    {
        // Lấy dữ liệu xem đã mở tới chapter mấy. Mặc định người mới chơi là 1.
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedChapter", 1);

        for (int i = 0; i < chapterButtons.Length; i++)
        {
            // i là index (0, 1, 2). Chapter tương ứng là i + 1 (1, 2, 3)
            if (i + 1 <= unlockedLevel)
            {
                // ĐÃ MỞ KHÓA
                chapterButtons[i].interactable = true;
                if (lockIcons.Length > i && lockIcons[i] != null) lockIcons[i].SetActive(false); // Tắt ổ khóa
            }
            else
            {
                // BỊ KHÓA
                chapterButtons[i].interactable = false;
                if (lockIcons.Length > i && lockIcons[i] != null) lockIcons[i].SetActive(true);  // Hiện ổ khóa
            }
        }
    }

    // Gắn hàm này vào từng nút Chapter, truyền tên Scene của Chapter đó vào
    public void LoadChapter(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    // Nút dùng để test (Xóa dữ liệu chơi lại từ đầu)
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("UnlockedChapter");
        CheckUnlockedChapters();
        Debug.Log("<color=red>Đã xóa toàn bộ dữ liệu Chapter!</color>");
    }

    public void GoBackToMainMenu()
    {
        if (startPanel != null) startPanel.SetActive(true);
        if (chapterPanel != null) chapterPanel.SetActive(false);
    }
}
