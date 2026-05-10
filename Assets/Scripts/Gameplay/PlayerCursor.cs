using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerCursor : MonoBehaviour
{
    public LayerMask interactableLayer;
    public Texture2D defaultCursor;
    public Texture2D hoverCursor;

    void Update()
    {
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
}
