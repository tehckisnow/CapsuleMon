using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemCategory { Items, Capsules, Tms, KeyItems}

public class Inventory : MonoBehaviour, ISavable
{
    [Header("Items")]
    [SerializeField] List<ItemSlot> slots;
    [Header("Capsules")]
    [SerializeField] List<ItemSlot> capsuleSlots;
    [Header("TMs and HMs")]
    [SerializeField] List<ItemSlot> tmSlots;
    [Header("Key Items")]
    [SerializeField] List<ItemSlot> keyItemSlots;

    List<List<ItemSlot>> allSlots;

    public event Action OnUpdated;

    private void Awake()
    {
        allSlots = new List<List<ItemSlot>>() 
        { 
            slots, capsuleSlots, tmSlots, keyItemSlots
        };
    }

    public static List<string> ItemCategories { get; set; } = new List<string>()
    {
        "ITEMS", "CAPSULES", "TMs & HMs", "Key Items"
    };

    public List<ItemSlot> GetSlotsByCategory(int categoryIndex)
    {
        return allSlots[categoryIndex];
    }

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }

    public ItemBase GetItem(int itemIndex, int categoryIndex)
    {
        var currentSlots = GetSlotsByCategory(categoryIndex);
        
        if(currentSlots.Count < 1)
        {
            return null;
        }
        
        return currentSlots[itemIndex].Item;
    }

    public ItemBase UseItem(int itemIndex, Mon selectedMon, int selectedCategory)
    {
        var item = GetItem(itemIndex, selectedCategory);

        bool itemUsed = item.Use(selectedMon);
        if(itemUsed)
        {
            if(!item.IsReusable)
            {
                RemoveItem(item);
            }
            return item;
        }
        return null;
    }

    public void AddItem(ItemBase item, int count=1)
    {
        //find category based on item
        int category = (int)GetCategoryFromItem(item);
        var currentSlots = GetSlotsByCategory(category);
        
        //check for existing slot
        var itemSlot = currentSlots.FirstOrDefault(slot => slot.Item == item);
        if(itemSlot != null)
        {
            //slot exists; increase count
            itemSlot.Count += count;
        }
        else
        {
            //slot does not exist, create new one
            currentSlots.Add(new ItemSlot(){
                Item = item,
                Count = count
            });
        }

        OnUpdated?.Invoke();
    }

    public void RemoveItem(ItemBase item)
    {
        int category = (int)GetCategoryFromItem(item);
        var currentSlots = GetSlotsByCategory(category);

        var itemSlot = currentSlots.First(slots => slots.Item == item);
        itemSlot.Count--;
        if(itemSlot.Count == 0)
        {
            currentSlots.Remove(itemSlot);
        }

        OnUpdated?.Invoke();
    }

    public bool HasItem(ItemBase item)
    {
        int category = (int)GetCategoryFromItem(item);
        var currentSlots = GetSlotsByCategory(category);

        return currentSlots.Exists(slot => slot.Item == item);
    }

    private ItemCategory GetCategoryFromItem(ItemBase item)
    {
        if(item is RecoveryItem || item is EvolutionItem || item is LevelUpItem)
        {
            return ItemCategory.Items;
        }
        else if(item is CapsuleItem)
        {
            return ItemCategory.Capsules;
        }
        else if(item is KeyItem)
        {
            return ItemCategory.KeyItems;
        }
        else
        {
            return ItemCategory.Tms;
        }
    }

    //ISavable
    public object CaptureState()
    {
        var saveData = new InventorySaveData()
        {
            items = slots.Select(i => i.GetSaveData()).ToList(),
            capsules = capsuleSlots.Select(i => i.GetSaveData()).ToList(),
            tms = tmSlots.Select(i => i.GetSaveData()).ToList(),
            keyItems = keyItemSlots.Select(i => i.GetSaveData()).ToList()
        };
        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as InventorySaveData;

        slots = saveData.items.Select(i => new ItemSlot(i)).ToList();
        capsuleSlots = saveData.capsules.Select(i => new ItemSlot(i)).ToList();
        tmSlots = saveData.tms.Select(i => new ItemSlot(i)).ToList();
        keyItemSlots = saveData.keyItems.Select(i => new ItemSlot(i)).ToList();

        //! this is set manually in two places and thus should be refactored into a separate function
        allSlots = new List<List<ItemSlot>>() { slots, capsuleSlots, tmSlots, keyItemSlots };

        OnUpdated?.Invoke();
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;
    public ItemBase Item {
        get => item;
        set => item = value;
    }
    public int Count {
        get => count;
        set => count = value;
    }

    public ItemSlot()
    {

    }

    public ItemSlot(ItemSaveData saveData)
    {
        item = ItemDB.GetObjectByName(saveData.name);
        count = saveData.count;
    }

    public ItemSaveData GetSaveData()
    {
        var SaveData = new ItemSaveData()
        {
            name = item.name,
            count = count
        };
        return SaveData;
    }
}

[Serializable]
public class ItemSaveData
{
    public string name;
    public int count;
}

[Serializable]
public class InventorySaveData
{
    public List<ItemSaveData> items;
    public List<ItemSaveData> capsules;
    public List<ItemSaveData> tms;
    public List<ItemSaveData> keyItems;
}