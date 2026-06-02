using UnityEngine;
using UnityEngine.SceneManagement;

public class ChapterComplete : MonoBehaviour
{
    [Tooltip("Khi qua màn này, sẽ mở khóa Chapter số mấy? (Ví dụ: Qua chap 1 thì điền số 2)")]
    public int nextChapterToUnlock = 2;

    [Tooltip("Tên Scene Menu chính để quay về sau khi thắng")]
    public string menuSceneName = "MainMenu";

    [Header("Cutscene & Next Chapter Config")]
    [Tooltip("Cutscene Player chạy khi kết thúc chapter này")]
    public CutscenePlayer endingCutscenePlayer;

    [Tooltip("Tên Scene của Chapter tiếp theo cần chuyển tới sau khi cutscene kết thúc")]
    public string nextChapterSceneName;

    // Gọi hàm này khi người chơi giải xong câu đố cuối cùng
    public void FinishChapter()
    {
        // 1. Thực hiện mở khóa Chapter tiếp theo ngay lập tức để lưu tiến trình phòng trường hợp người chơi tắt game giữa chừng khi đang xem cutscene
        UnlockNextChapter();

        // 2. Chạy cutscene nếu có
        if (endingCutscenePlayer != null)
        {
            endingCutscenePlayer.loadNextSceneOnComplete = true;
            
            // Nếu có chapter tiếp theo thì chuyển tới chapter đó, không thì quay về Menu
            if (!string.IsNullOrEmpty(nextChapterSceneName))
            {
                endingCutscenePlayer.nextSceneName = nextChapterSceneName;
            }
            else
            {
                endingCutscenePlayer.nextSceneName = menuSceneName;
            }
            
            endingCutscenePlayer.PlayCutscene();
        }
        else
        {
            // Nếu không có cutscene kết thúc, chuyển thẳng sang scene tiếp theo
            string targetScene = !string.IsNullOrEmpty(nextChapterSceneName) ? nextChapterSceneName : menuSceneName;
            if (SceneController.Instance != null)
            {
                SceneController.Instance.LoadScene(targetScene);
            }
            else
            {
                SceneManager.LoadScene(targetScene);
            }
        }
    }

    public void UnlockNextChapter()
    {
        int currentUnlocked = PlayerPrefs.GetInt("UnlockedChapter", 1);

        // Chỉ lưu đè nếu chapter chuẩn bị mở lớn hơn kỷ lục hiện tại
        if (nextChapterToUnlock > currentUnlocked)
        {
            PlayerPrefs.SetInt("UnlockedChapter", nextChapterToUnlock);
            PlayerPrefs.Save(); // Lưu thẳng vào ổ cứng
            Debug.Log($"<color=green>[ChapterComplete] Đã mở khóa Chapter {nextChapterToUnlock}</color>");
        }
    }
}