using UnityEngine;
using UnityEngine.SceneManagement;

public class ChapterComplete : MonoBehaviour
{
    [Tooltip("Khi qua màn này, sẽ mở khóa Chapter số mấy? (Ví dụ: Qua chap 1 thì điền số 2)")]
    public int nextChapterToUnlock = 2;

    [Tooltip("Tên Scene Menu chính để quay về sau khi thắng")]
    public int menuSceneName = 0;

    // Gọi hàm này khi người chơi giải xong câu đố cuối cùng
    public void FinishChapter()
    {
        int currentUnlocked = PlayerPrefs.GetInt("UnlockedChapter", 1);

        // Chỉ lưu đè nếu chapter chuẩn bị mở lớn hơn kỷ lục hiện tại
        if (nextChapterToUnlock > currentUnlocked)
        {
            PlayerPrefs.SetInt("UnlockedChapter", nextChapterToUnlock);
            PlayerPrefs.Save(); // Lưu thẳng vào ổ cứng máy tính/điện thoại
            Debug.Log($"<color=green>Chúc mừng! Đã mở khóa Chapter {nextChapterToUnlock}</color>");
        }

        // Quay về Menu để người chơi chọn chap mới
        SceneManager.LoadScene(menuSceneName);
    }
}