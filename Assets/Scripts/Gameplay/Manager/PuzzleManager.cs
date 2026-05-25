using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }
    private Dictionary<string, bool> puzzleStates = new Dictionary<string, bool>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetState(string key, bool value)
    {
        puzzleStates[key] = value;
        Debug.Log($"[PuzzleManager] {key} = {value}");
    }

    public bool GetState(string key)
    {
        if (puzzleStates.TryGetValue(key, out bool val)) return val;
        return false;
    }

    public void ClearAllStates() => puzzleStates.Clear();

    public List<string> GetSolvedKeys()
    {
        List<string> solved = new List<string>();
        foreach (var kv in puzzleStates)
            if (kv.Value) solved.Add(kv.Key);
        return solved;
    }

    // Chỉnh: trả về bool để caller biết có thành công hay không
    public bool TrySolveCombination(string puzzleId, params string[] requiredItemIds)
    {
        // kiểm tra đủ item trong inventory
        foreach (var id in requiredItemIds)
        {
            if (!Inventory.Instance.HasItem(id))
            {
                Debug.Log($"[PuzzleManager] Missing required item {id} for puzzle {puzzleId}");
                return false;
            }
        }

        // đánh dấu đã giải
        SetState(puzzleId, true);

        // Remove used items
        foreach (var id in requiredItemIds)
        {
            Item it = Inventory.Instance.GetItemById(id);
            if (it != null) Inventory.Instance.RemoveItem(it);
        }

        // Trigger puzzle solved effects: find interactable with puzzleId and call method
        Interactable target = FindInteractableById(puzzleId);
        if (target != null)
        {
            Debug.Log($"[PuzzleManager] Puzzle {puzzleId} solved, calling Interact on {target.name}");
            target.Interact();
        }
        else
        {
            Debug.Log($"[PuzzleManager] Puzzle {puzzleId} solved but no Interactable found with id {puzzleId}");
        }

        return true;
    }

    Interactable FindInteractableById(string id)
    {
        Interactable[] all = FindObjectsOfType<Interactable>();
        foreach (var a in all) if (a.id == id) return a;
        return null;
    }
}
