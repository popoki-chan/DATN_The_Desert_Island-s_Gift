using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Core Systems (assign prefabs or scene objects)")]
    public Inventory inventory;
    public PuzzleManager puzzleManager;
    public AudioManager audioManager;
    public SceneController sceneController;
    public SaveSystem saveSystem;
    

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureReferences();
        }
        else
        {
            Destroy(gameObject);
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
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (Instance != this) return;

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
                    Debug.Log($"<color=green>[GameManager] Tự động mở khóa Chapter {chapterNum} khi vào scene!</color>");
                }
            }
        }
    }
}
