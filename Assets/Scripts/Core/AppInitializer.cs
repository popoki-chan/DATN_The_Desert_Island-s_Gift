using UnityEngine;

public static class AppInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // 1. Nếu GameManager đã tồn tại trong scene (ví dụ: do load từ trước), bỏ qua
        if (GameManager.Instance != null) return;

        // 2. Load prefab "Game Manager Roots" từ thư mục Resources
        var prefab = Resources.Load<GameObject>("Game Manager Roots");
        if (prefab != null)
        {
            var instance = Object.Instantiate(prefab);
            instance.name = "Game Manager Roots";
            Object.DontDestroyOnLoad(instance);
            Debug.Log("<color=green>[AppInitializer] Đã tự động tạo Game Manager Roots trước khi load Scene!</color>");
        }
        else
        {
            Debug.LogError("[AppInitializer] Không tìm thấy prefab Game Manager Roots trong Resources!");
        }
    }
}
