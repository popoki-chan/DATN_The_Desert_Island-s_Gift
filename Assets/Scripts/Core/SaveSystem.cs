using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

[Serializable]
public class SaveData
{
    public string version = "1";
    public string currentScene;
    public List<string> inventoryIds = new List<string>();
    public List<string> solvedPuzzles = new List<string>();
}

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }
    private string saveFile => Path.Combine(Application.persistentDataPath, "save.json");

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();
        data.currentScene = SceneManager.GetActiveScene().name;

        if (Inventory.Instance != null)
        {
            foreach (var it in Inventory.Instance.items)
                data.inventoryIds.Add(it.itemId);
        }

        if (PuzzleManager.Instance != null)
            data.solvedPuzzles = PuzzleManager.Instance.GetSolvedKeys();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFile, json);
        Debug.Log("[SaveSystem] Saved to " + saveFile);
    }

    public void LoadGame()
    {
        if (!File.Exists(saveFile))
        {
            Debug.LogWarning("[SaveSystem] No save file found.");
            return;
        }

        string json = File.ReadAllText(saveFile);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // Load scene first, then restore other states after scene loaded
        StartCoroutine(LoadAndRestore(data));
    }

    System.Collections.IEnumerator LoadAndRestore(SaveData data)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(data.currentScene);
        while (!op.isDone) yield return null;
        yield return null; // wait one frame for objects to initialize

        // Restore inventory
        if (Inventory.Instance != null)
        {
            Inventory.Instance.ClearAll();
            foreach (var id in data.inventoryIds)
            {
                Item asset = Resources.Load<Item>($"Items/{id}");
                if (asset != null) Inventory.Instance.AddItem(asset, false);
                else Debug.LogWarning($"[SaveSystem] Item asset not found in Resources/Items: {id}");
            }
        }

        // Restore puzzles
        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.ClearAllStates();
            foreach (var key in data.solvedPuzzles)
                PuzzleManager.Instance.SetState(key, true);
        }

        Debug.Log("[SaveSystem] Load complete.");
    }

    public bool HasSave() => File.Exists(saveFile);
}
