using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(Collider2D))] // Đảm bảo luôn có Collider để click trúng
public class CraftingInteractable : MonoBehaviour
{
    [Header("Recipe")]
    public string requiredItemId;
    public string resultItemId;
    public GameObject resultPrefab;

    [Header("Settings & Effects")]
    [Tooltip("Dùng xong có mất đồ trên tay không?")]
    public bool consumeRequiredItem = true;
    [Tooltip("Thời gian rung lắc xử lý (giây)")]
    public float processTime = 0.5f;
    [Tooltip("Âm thanh khi tương tác")]
    public AudioClip interactSFX;

    [Header("Scraping Animation Sprites")]
    public Sprite sharpRockSprite;
    public Sprite coirSprite;

    [Header("Debug")]
    public bool spawnAtCameraForTest = false;

    private bool crafted = false;
    private SpriteRenderer mySpriteRenderer;
    private Color originalColor;

    void Awake()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        if (mySpriteRenderer != null)
        {
            originalColor = mySpriteRenderer.color;
        }
        else
        {
            originalColor = Color.white;
        }
    }

    void OnEnable()
    {
        if (mySpriteRenderer != null)
        {
            mySpriteRenderer.color = originalColor;
        }
        crafted = false;
    }


    // --- BẮT SỰ KIỆN CLICK CHUỘT (PHẦN MỚI THÊM) ---
    void OnMouseDown()
    {
        if (SettingsPopupController.IsOpen) return;
        if (IsPointerOverUI()) return;

        // Khi click vào vật thể, tự động lấy món đồ đang chọn trong túi đồ ra xài
        if (Inventory.Instance != null)
        {
            TryUseItem(Inventory.Instance.currentSelectedItem);
        }
        else
        {
            Debug.LogWarning("[Crafting] Inventory.Instance is null!");
        }
    }

    // --- HÀM CHÍNH: trả về item đang tương tác (có thể null) ---
    public Item GetInteractingItem(Item currentSelectedItem)
    {
        if (currentSelectedItem == null)
        {
            Debug.Log("[Crafting] No item selected when interacting with " + name);
            return null;
        }

        string id = GetItemId(currentSelectedItem);
        Debug.Log($"[Crafting] Interacting item id: {id} with target {name}");
        return currentSelectedItem;
    }

    // Kiểm tra nhanh xem item hiện tại có khớp recipe không
    public bool IsMatchingRequiredItem(Item currentSelectedItem)
    {
        if (currentSelectedItem == null) return false;
        string id = GetItemId(currentSelectedItem);
        return !string.IsNullOrEmpty(requiredItemId) && id == requiredItemId;
    }

    // Gọi khi muốn thực hiện craft
    public void TryUseItem(Item currentSelectedItem)
    {
        if (crafted)
        {
            Debug.Log("[Crafting] Already crafted on " + name);
            return;
        }

        GetInteractingItem(currentSelectedItem);

        if (IsMatchingRequiredItem(currentSelectedItem))
        {
            // Thay vì Spawn ngay lập tức, gọi Coroutine để chạy hiệu ứng
            StartCoroutine(ProcessCrafting(currentSelectedItem));
        }
        else
        {
            Debug.Log($"[Crafting] Item does not match recipe on {name}. Required: {requiredItemId}");
        }
    }

    // Coroutine xử lý hiệu ứng "Juice" và tạo vật phẩm
    private IEnumerator ProcessCrafting(Item currentSelectedItem)
    {
        crafted = true; // Khóa lại không cho bấm spam nữa

        // 1. Trừ vật phẩm trên tay (Dùng ConsumeSelectedItem để tắt luôn viền vàng UI)
        if (consumeRequiredItem && Inventory.Instance != null)
        {
            Inventory.Instance.ConsumeSelectedItem();
        }

        // 2. Tạo đối tượng sharp_rock ảo để chạy hoạt ảnh bào
        GameObject rockVisual = null;
        if (sharpRockSprite != null)
        {
            rockVisual = new GameObject("TempSharpRock", typeof(SpriteRenderer));
            SpriteRenderer rockSR = rockVisual.GetComponent<SpriteRenderer>();
            if (rockSR != null)
            {
                rockSR.sprite = sharpRockSprite;
                rockSR.sortingOrder = 10; // Đặt lên trước coconut
            }
        }

        Vector3 coconutPos = transform.position;
        Vector3 rockStartPos = coconutPos + new Vector3(0.5f, 0.4f, 0f);
        Vector3 rockScrapePos = coconutPos + new Vector3(-0.2f, 0.1f, 0f);

        if (rockVisual != null)
        {
            rockVisual.transform.position = rockStartPos;
            rockVisual.transform.localScale = Vector3.one;
        }

        // Xác định vị trí spawn và sinh ra vật phẩm thật trước dưới dạng vô hình / nhỏ nhất
        Vector3 spawnPos;
        if (spawnAtCameraForTest && Camera.main != null)
        {
            spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 2f;
            Debug.Log("[Crafting] Spawning at camera for test at " + spawnPos);
        }
        else
        {
            // Spawn tại mặt đất bên dưới quả dừa để các mảnh vụn rơi vào đúng chỗ
            spawnPos = transform.position + new Vector3(0f, -0.7f, 0f);
        }

        GameObject spawnedResult = null;
        Collider2D resultCol = null;
        Vector3 targetScale = Vector3.one;

        if (resultPrefab != null)
        {
            spawnedResult = Instantiate(resultPrefab, spawnPos, Quaternion.identity);
            if (spawnedResult != null)
            {
                targetScale = resultPrefab.transform.localScale;
                spawnedResult.transform.localScale = Vector3.zero;
                resultCol = spawnedResult.GetComponent<Collider2D>();
                if (resultCol != null)
                {
                    resultCol.enabled = false; // Tạm thời khóa nhặt trong lúc đang scale
                }
                spawnedResult.SetActive(true);
            }
        }

        System.Collections.Generic.List<GameObject> tempCoirs = new System.Collections.Generic.List<GameObject>();

        // Chạy hoạt ảnh bào và rơi xơ dừa 3 lần
        for (int i = 0; i < 3; i++)
        {
            // Chạy âm thanh
            if (interactSFX != null)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(interactSFX);
                }
                else if (Camera.main != null)
                {
                    AudioSource.PlayClipAtPoint(interactSFX, Camera.main.transform.position);
                }
            }

            if (rockVisual != null)
            {
                // Hoạt ảnh sharp_rock di chuyển chà xát lên quả dừa
                Tween rockTween = rockVisual.transform.DOMove(rockScrapePos, 0.25f).SetEase(Ease.OutQuad);
                yield return rockTween.WaitForCompletion();
            }
            else
            {
                yield return new WaitForSeconds(0.25f);
            }

            // Rung lắc quả dừa nhẹ khi bị chà xát
            transform.DOComplete();
            transform.DOShakePosition(0.15f, 0.05f, 10, 90, false, true);

            // Sinh ra mảnh coir nhỏ rơi xuống
            if (coirSprite != null)
            {
                GameObject coirVisual = new GameObject("TempCoirPiece", typeof(SpriteRenderer));
                SpriteRenderer coirSR = coirVisual.GetComponent<SpriteRenderer>();
                if (coirSR != null)
                {
                    coirSR.sprite = coirSprite;
                    coirSR.sortingOrder = 9;
                }
                
                // Vị trí ban đầu của mảnh coir ở trung tâm quả dừa, kích thước rất nhỏ
                coirVisual.transform.position = coconutPos;
                coirVisual.transform.localScale = Vector3.one * 0.1f;
                tempCoirs.Add(coirVisual);

                // Hoạt ảnh rơi xuống đất (rơi trúng đống xơ dừa thật đang lớn dần)
                Vector3 landPos = spawnPos + new Vector3(Random.Range(-0.4f, 0.4f), 0f, 0f);
                coirVisual.transform.DOMove(landPos, 0.4f).SetEase(Ease.OutBounce);
                coirVisual.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutQuad);
            }

            // Đồng thời scale xơ dừa thật to dần lên theo từng lần bào (3 lần tương ứng 33%, 66%, 100%)
            if (spawnedResult != null)
            {
                float scaleRatio = (i + 1) / 3f;
                spawnedResult.transform.DOScale(targetScale * scaleRatio, 0.4f).SetEase(Ease.OutQuad);
            }

            if (rockVisual != null)
            {
                // Đưa sharp_rock về vị trí xuất phát để cạo lần tiếp theo
                Tween rockTween = rockVisual.transform.DOMove(rockStartPos, 0.2f).SetEase(Ease.InQuad);
                yield return rockTween.WaitForCompletion();
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
            }
        }

        // Đợi thêm một nhịp ngắn trước khi dọn dẹp
        yield return new WaitForSeconds(0.15f);

        // 3. Hủy các đối tượng tạm (đồng thời kill các tween đang chạy trên đó để tránh lỗi DOTween)
        if (rockVisual != null)
        {
            rockVisual.transform.DOKill();
            Destroy(rockVisual);
        }
        foreach (var tc in tempCoirs)
        {
            if (tc != null)
            {
                tc.transform.DOKill();
                Destroy(tc);
            }
        }

        // 4. Chỉ có quả dừa mờ dần rồi biến mất (xơ dừa thật đã đạt kích thước đầy đủ và giữ nguyên)
        SpriteRenderer coconutSR = GetComponent<SpriteRenderer>();
        if (coconutSR != null)
        {
            Tween fadeTween = coconutSR.DOFade(0f, 0.5f).SetEase(Ease.OutQuad);
            yield return fadeTween.WaitForCompletion();
        }

        // 5. Cất quả dừa gốc đi (Biến mất)
        gameObject.SetActive(false);

        // 6. Cho phép nhặt xơ dừa thật sau khi quả dừa biến mất và xơ dừa đạt đủ kích thước
        if (resultCol != null)
        {
            resultCol.enabled = true;
        }
    }

    void SpawnResult(Vector3 spawnPos)
    {
        if (resultPrefab == null)
        {
            Debug.LogWarning("[Crafting] resultPrefab is null on " + gameObject.name);
            return;
        }

        GameObject spawned = Instantiate(resultPrefab, spawnPos, Quaternion.identity);

        if (spawned == null)
        {
            Debug.LogWarning("[Crafting] Instantiate returned null for " + resultPrefab.name);
            return;
        }

        spawned.SetActive(true);
        Debug.Log($"[Crafting] Spawned {spawned.name} at {spawned.transform.position} parent:{(spawned.transform.parent ? spawned.transform.parent.name : "root")} active:{spawned.activeSelf}");
    }

    // Helper lấy id từ Item
    string GetItemId(Item item)
    {
        return item.itemId;
    }

    // ContextMenu để test thủ công trong Play Mode
    [ContextMenu("Spawn Result Manual (use current inventory selection)")]
    public void SpawnResultManual()
    {
        var item = Inventory.Instance != null ? Inventory.Instance.currentSelectedItem : null;
        Debug.Log("[Crafting] Manual spawn test. Inventory selected: " + (item == null ? "null" : item.itemId));
        TryUseItem(item);
    }

    [ContextMenu("Spawn Result Force (no check)")]
    public void SpawnResultForce()
    {
        Debug.Log("[Crafting] Force spawn (bypass checks)");
        crafted = true;
        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
        SpawnResult(spawnPos);
        gameObject.SetActive(false);
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        if (EventSystem.current.IsPointerOverGameObject()) return true;
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                return true;
        }
        return false;
    }
}