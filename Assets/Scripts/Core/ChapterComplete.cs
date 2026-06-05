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

    public void FinishChapter()
    {

        UnlockNextChapter();

        if (endingCutscenePlayer != null)
        {
            endingCutscenePlayer.loadNextSceneOnComplete = true;
            
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

        if (nextChapterToUnlock > currentUnlocked)
        {
            PlayerPrefs.SetInt("UnlockedChapter", nextChapterToUnlock);
            PlayerPrefs.Save();
            Debug.Log($"<color=green>[ChapterComplete] Đã mở khóa Chapter {nextChapterToUnlock}</color>");
        }
    }
}