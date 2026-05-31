using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item")]
public class Item : ScriptableObject
{
    public string itemId;
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    public bool stackable = false;
    public int maxUses = 1;
}
