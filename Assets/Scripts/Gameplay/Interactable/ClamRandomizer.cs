using UnityEngine;
using System.Collections.Generic;

public class ClamRandomizer : MonoBehaviour
{
    void Start()
    {
        // 1. Tìm tất cả các OpenContainer có tên "Clam" thuộc scene hiện tại (kể cả active/inactive)
        OpenContainer[] allContainers = Resources.FindObjectsOfTypeAll<OpenContainer>();
        List<OpenContainer> clamsList = new List<OpenContainer>();
        
        foreach (var container in allContainers)
        {
            if (container != null && container.gameObject.scene == gameObject.scene && container.gameObject.name == "Clam")
            {
                clamsList.Add(container);
            }
        }

        OpenContainer[] clams = clamsList.ToArray();
        if (clams.Length == 0)
        {
            Debug.LogWarning("[ClamRandomizer] Không tìm thấy vỏ sò (Clam) nào trong scene!");
            return;
        }

        // 2. Tìm kiếm đối tượng oyster_meat dưới các vỏ sò
        GameObject oysterMeat = null;
        foreach (var clam in clams)
        {
            if (clam.openStateObject != null)
            {
                Transform found = clam.openStateObject.transform.Find("oyster_meat");
                if (found != null)
                {
                    oysterMeat = found.gameObject;
                    break;
                }
            }
        }

        if (oysterMeat == null)
        {
            Debug.LogError("[ClamRandomizer] Không tìm thấy oyster_meat dưới các vỏ sò!");
            return;
        }

        // 3. Chọn ngẫu nhiên vỏ sò chứa thịt trong tất cả vỏ sò của scene
        int luckyIndex = Random.Range(0, clams.Length);
        Debug.Log($"[ClamRandomizer] Chọn vỏ sò thứ {luckyIndex} trên tổng số {clams.Length} làm vỏ chứa thịt (Tên path: {GetGameObjectPath(clams[luckyIndex].gameObject)}).");

        for (int i = 0; i < clams.Length; i++)
        {
            var clam = clams[i];
            if (clam == null) continue;

            if (i == luckyIndex)
            {
                // Gán thịt sò vào vỏ sò được chọn
                if (clam.openStateObject != null)
                {
                    oysterMeat.transform.SetParent(clam.openStateObject.transform);
                    oysterMeat.transform.localPosition = Vector3.zero;
                    oysterMeat.transform.localRotation = Quaternion.identity;
                    oysterMeat.transform.localScale = Vector3.one;
                }
                clam.itemToReveal = oysterMeat;
                oysterMeat.SetActive(false); // Ẩn đi ban đầu
            }
            else
            {
                // Các vỏ sò khác không có gì
                clam.itemToReveal = null;
            }
        }
    }

    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
}
