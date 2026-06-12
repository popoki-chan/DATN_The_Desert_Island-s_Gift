using UnityEngine;

public class ClamRandomizer : MonoBehaviour
{
    void Start()
    {
        // 1. Lấy tất cả các vỏ sò dưới View Chest
        OpenContainer[] clams = GetComponentsInChildren<OpenContainer>(true);
        if (clams == null || clams.Length == 0) return;

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

        // 3. Chọn ngẫu nhiên vỏ sò chứa thịt
        int luckyIndex = Random.Range(0, clams.Length);
        Debug.Log($"[ClamRandomizer] Chọn vỏ sò thứ {luckyIndex} làm vỏ chứa thịt.");

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
}
