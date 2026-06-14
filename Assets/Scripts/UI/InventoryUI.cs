using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : Singleton<InventoryUI>
{
    [Header("UI Cài đặt")]
    public Transform gridParent;

    [Header("UI Hình ảnh Slot")]
    public Sprite normalSlotSprite;  
    public Sprite selectedSlotSprite; 

    [Header("UI Text hiển thị tên Item")]
    public TMPro.TextMeshProUGUI selectedItemNameText;

    [HideInInspector] public Item SelectedItem;

    // Quản lý ánh xạ: Slot nào đang chứa Item nào và ngược lại
    private Dictionary<Item, GameObject> slotMap = new Dictionary<Item, GameObject>();
    private Dictionary<GameObject, Item> slotToItemMap = new Dictionary<GameObject, Item>();

    // Mảng lưu trữ 6 slot cố định
    private GameObject[] allSlots;
    private Inventory cachedInventory;

    protected override void Awake()
    {
        DontDestroyOnLoadEnabled = false;
        base.Awake();
    }

    void Start()
    {
        // 1. Nạp 6 slot đã có sẵn trong hierarchy vào mảng
        int childCount = gridParent.childCount;
        allSlots = new GameObject[childCount];

        for (int i = 0; i < childCount; i++)
        {
            GameObject slot = gridParent.GetChild(i).gameObject;
            allSlots[i] = slot;

            // Gán sự kiện click cố định cho từng slot
            Button btn = slot.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnSlotClicked(slot));

            // Dọn dẹp UI trắng tinh lúc mới vào game
            ClearSlotUI(slot);
        }

        // 2. Đăng ký sự kiện với Inventory core
        if (Inventory.Instance != null)
        {
            cachedInventory = Inventory.Instance;
            cachedInventory.OnItemAdded += OnItemAdded;
            cachedInventory.OnItemRemoved += OnItemRemoved;
            cachedInventory.OnInventoryChanged += RefreshAll;
            cachedInventory.OnItemSelected += HandleItemSelected;

            RefreshAll(); // Cập nhật lại UI lỡ Inventory đã có đồ từ trước
        }
    }

    protected override void OnDestroy()
    {
        if (cachedInventory != null)
        {
            cachedInventory.OnItemAdded -= OnItemAdded;
            cachedInventory.OnItemRemoved -= OnItemRemoved;
            cachedInventory.OnInventoryChanged -= RefreshAll;
            cachedInventory.OnItemSelected -= HandleItemSelected;
            cachedInventory = null;
        }
        base.OnDestroy();
    }

    void OnItemAdded(Item item) => AddSlot(item);
    void OnItemRemoved(Item item) => RemoveSlot(item);

    void RefreshAll()
    {
        // Dọn sạch toàn bộ 6 slot
        foreach (GameObject slot in allSlots)
        {
            ClearSlotUI(slot);
        }
        slotMap.Clear();
        slotToItemMap.Clear();
        Deselect();

        // Gắn lại từng item vào các slot trống
        if (Inventory.Instance.items != null)
        {
            foreach (var it in Inventory.Instance.items)
            {
                AddSlot(it);
            }
        }
    }

    void AddSlot(Item item)
    {
        if (slotMap.ContainsKey(item)) return; // Tránh trùng lặp đồ

        // Tìm slot TRỐNG đầu tiên để nhét đồ vào
        foreach (GameObject slot in allSlots)
        {
            if (!slotToItemMap.ContainsKey(slot))
            {
                // Đã tìm thấy slot trống -> Lưu data
                slotMap[item] = slot;
                slotToItemMap[slot] = item;

                // Cập nhật UI
                Image icon = slot.transform.Find("Icon").GetComponent<Image>();
                icon.sprite = item.icon;
                icon.color = Color.white; // Hiện Icon lên
                break; // Xong việc thì thoát vòng lặp
            }
        }
    }

    void RemoveSlot(Item item)
    {
        // Nếu tìm thấy đồ cần xóa đang nằm ở slot nào
        if (slotMap.TryGetValue(item, out GameObject slot))
        {
            ClearSlotUI(slot); // Xóa hình ảnh

            // Xóa data
            slotMap.Remove(item);
            slotToItemMap.Remove(slot);

            if (SelectedItem == item) Deselect();
        }
    }

    void ClearSlotUI(GameObject slot)
    {
        // Ẩn Icon đi
        Image icon = slot.transform.Find("Icon").GetComponent<Image>();
        icon.sprite = null;
        icon.color = new Color(1, 1, 1, 0); // Trong suốt

        // Trả nền slot về ảnh mặc định ban đầu
        slot.GetComponent<Image>().sprite = normalSlotSprite;
    }

    void OnSlotClicked(GameObject slot)
    {
        // Bấm vào ô trống -> Không làm gì cả
        if (!slotToItemMap.ContainsKey(slot)) return;

        Item item = slotToItemMap[slot];

        // Nếu đang chọn rồi mà bấm lại -> Bỏ chọn
        if (SelectedItem == item) { Deselect(); return; }

        SelectedItem = item;

        // BƯỚC THAY ĐỔI: Chuyển toàn bộ các slot khác về ảnh bình thường
        foreach (var s in allSlots)
        {
            s.GetComponent<Image>().sprite = normalSlotSprite;
        }

        // Chỉ đổi ảnh của slot vừa được bấm sang ảnh "Được chọn"
        slot.GetComponent<Image>().sprite = selectedSlotSprite;

        Inventory.Instance.SelectItem(item);
    }

    public void Deselect()
    {
        SelectedItem = null;
        if (allSlots != null)
        {
            // Trả toàn bộ slot về ảnh bình thường
            foreach (GameObject slot in allSlots)
            {
                slot.GetComponent<Image>().sprite = normalSlotSprite;
            }
        }
        if (Inventory.Instance != null)
        {
            Inventory.Instance.SelectItem(null);
        }
    }

    private void HandleItemSelected(Item item)
    {
        if (selectedItemNameText != null)
        {
            if (item != null)
            {
                selectedItemNameText.text = item.itemName;
                selectedItemNameText.gameObject.SetActive(true);

                // Thẳng hàng theo chiều dọc (World Y) với Slot chứa vật phẩm
                if (slotMap.TryGetValue(item, out GameObject slot))
                {
                    Vector3 slotWorldPos = slot.transform.position;
                    Vector3 textWorldPos = selectedItemNameText.transform.position;
                    textWorldPos.y = slotWorldPos.y;
                    selectedItemNameText.transform.position = textWorldPos;
                }
            }
            else
            {
                selectedItemNameText.text = "";
                selectedItemNameText.gameObject.SetActive(false);
            }
        }
    }
}