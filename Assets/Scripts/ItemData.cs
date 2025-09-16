using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Spine.Unity.Examples;

[Serializable]
public class ItemData : IEquatable<ItemData>
{
    public string       ItemID = "";
    public int          ItemIndex = 0;
    public ItemTypes    itemTypes = ItemTypes.None;
    public int          ItemCost = 0;
    public bool         IsDefaultItem = false;
    public bool         IsSpecialItem = false;
    public bool         IsPurchased = false;
    public bool         IsEquipped = false;


    public bool Equals(ItemData other)
    {
        Debug.Log("Custom IEquatable<ItemData>");

        if (other == null)
        {
            Debug.Log("return");
            return false;
        }

        // Compare all relevant properties for equality
        return ItemID == other.ItemID &&
               ItemIndex == other.ItemIndex &&
               itemTypes == other.itemTypes &&
               ItemCost == other.ItemCost &&
               IsDefaultItem == other.IsDefaultItem &&
               IsSpecialItem == other.IsSpecialItem &&
               IsPurchased == other.IsPurchased &&
               IsEquipped == other.IsEquipped;
    }

    public override bool Equals(object obj)
    {
        Debug.Log("Custom IEquatable<ItemData>");

        if (obj == null || GetType() != obj.GetType())
        {
            Debug.Log("return");
            return false;
        }

        ItemData other = (ItemData)obj; // Cast obj to ItemData type

        // Compare all relevant properties for equality
        return ItemID == other.ItemID &&
               ItemIndex == other.ItemIndex &&
               itemTypes == other.itemTypes &&
               ItemCost == other.ItemCost &&
               IsDefaultItem == other.IsDefaultItem &&
               IsSpecialItem == other.IsSpecialItem &&
               IsPurchased == other.IsPurchased &&
               IsEquipped == other.IsEquipped;
    }

    public override int GetHashCode()
    {
        // Combine hash codes of all relevant properties
        unchecked // Overflow is fine for this purpose
        {
            int hashCode = ItemID.GetHashCode();
            hashCode = (hashCode * 397) ^ ItemIndex;
            hashCode = (hashCode * 397) ^ (int)itemTypes;
            hashCode = (hashCode * 397) ^ ItemCost;
            hashCode = (hashCode * 397) ^ IsDefaultItem.GetHashCode();
            hashCode = (hashCode * 397) ^ IsSpecialItem.GetHashCode();
            hashCode = (hashCode * 397) ^ IsPurchased.GetHashCode();
            hashCode = (hashCode * 397) ^ IsEquipped.GetHashCode();
            return hashCode;
        }
    }
}
