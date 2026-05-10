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

    void EnsureReferences()
    {
        if (inventory == null) inventory = FindObjectOfType<Inventory>();
        if (puzzleManager == null) puzzleManager = FindObjectOfType<PuzzleManager>();
        if (audioManager == null) audioManager = FindObjectOfType<AudioManager>();
        if (sceneController == null) sceneController = FindObjectOfType<SceneController>();
        if (saveSystem == null) saveSystem = FindObjectOfType<SaveSystem>();
    }

    
}
