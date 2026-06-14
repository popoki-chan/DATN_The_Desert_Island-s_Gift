using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("Core Systems (assign prefabs or scene objects)")]
    public Inventory inventory;
    public PuzzleManager puzzleManager;
    public AudioManager audioManager;
    public SceneController sceneController;
    public SaveSystem saveSystem;
    

    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            EnsureReferences();
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 100;
            Debug.Log("[GameManager] Locked game FPS to 100.");
        }
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void EnsureReferences()
    {
        if (inventory == null) inventory = FindObjectOfType<Inventory>();
        if (puzzleManager == null) puzzleManager = FindObjectOfType<PuzzleManager>();
        if (audioManager == null) audioManager = FindObjectOfType<AudioManager>();
        if (sceneController == null) sceneController = FindObjectOfType<SceneController>();
        if (saveSystem == null) saveSystem = FindObjectOfType<SaveSystem>();

        BindSettingsButton();
    }

    private void BindSettingsButton()
    {
        var btnGo = GameObject.Find("Btn Setting");
        if (btnGo != null)
        {
            var btn = btnGo.GetComponent<UnityEngine.UI.Button>();
            var popupController = Resources.FindObjectsOfTypeAll<SettingsPopupController>();
            if (btn != null && popupController.Length > 0)
            {
                var controller = popupController[0];
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(controller.Open);
                Debug.Log("<color=green>[GameManager] Dynamically bound settings button to SettingsPopupController.Open()</color>");
            }
        }
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (Instance != this) return;

        BindSettingsButton();

        string sceneName = scene.name;
        if (sceneName.StartsWith("Chapter"))
        {
            string numStr = sceneName.Replace("Chapter", "");
            if (int.TryParse(numStr, out int chapterNum))
            {
                int currentUnlocked = PlayerPrefs.GetInt("UnlockedChapter", 1);
                if (chapterNum > currentUnlocked)
                {
                    PlayerPrefs.SetInt("UnlockedChapter", chapterNum);
                    PlayerPrefs.Save();
                }
            }
        }
    }
}
