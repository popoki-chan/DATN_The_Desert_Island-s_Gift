using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCursor : MonoBehaviour
{
    public LayerMask interactableLayer;
    public Texture2D defaultCursor;
    public Texture2D hoverCursor;

    void Update()
    {
        // 1. Chặn mọi tương tác khi popup Settings đang mở
        if (SettingsPopupController.IsOpen)
        {
            Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
            return;
        }

        // 2. Chặn tương tác khi chuột/touch đang đè lên các phần tử UI khác (Inventory, HUD...)
        if (IsPointerOverUI())
        {
            Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
            return;
        }

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mouseWorld.x, mouseWorld.y);

        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero, 0f, interactableLayer);
        if (hit.collider != null)
        {
            Cursor.SetCursor(hoverCursor, Vector2.zero, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (hit.collider != null)
            {
                Interactable interact = hit.collider.GetComponent<Interactable>();
                if (interact != null)
                {
                    // If player has selected an item, attempt use
                    Item selected = InventoryUI.Instance?.SelectedItem;
                    if (selected != null)
                    {
                        Inventory.Instance.TryUseOn(selected, interact);
                    }
                    else
                    {
                        // normal click
                        interact.RaiseClicked(); 
                        if (interact.isPickable) interact.Pickup();
                        else interact.Interact();
                    }
                }
            }
            else
            {
                // click empty space: deselect item
                InventoryUI.Instance?.Deselect();
            }
        }
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        if (EventSystem.current.IsPointerOverGameObject()) return true;
        
        // Touch support
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                return true;
        }
        return false;
    }
}
