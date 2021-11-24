using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public enum InventoryUIState { ItemSelection, PartySelection, Busy }

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] TextMeshProUGUI categoryText;
    [SerializeField] Image itemIcon;
    [SerializeField] TextMeshProUGUI itemDescription;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    [SerializeField] PartyScreen partyScreen;

    private Action onItemUsed;

    private int selectedItem = 0;
    private int selectedCategory = 0;
    private InventoryUIState state;

    const int itemsInViewport = 8;

    List<ItemSlotUI> slotUIList;
    private Inventory inventory;

    RectTransform itemListRect;

    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateItemList();

        inventory.OnUpdated += UpdateItemList;
    }

    private void UpdateItemList()
    {
        //clear all existing items
        foreach(Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }
        slotUIList = new List<ItemSlotUI>();
        foreach(var itemSlot in inventory.GetSlotsByCategory(selectedCategory))
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);
            slotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack, Action onItemUsed = null)
    {
        this.onItemUsed = onItemUsed;

        if(state == InventoryUIState.ItemSelection)
        {
            int prevSelection = selectedItem;
            int prevCategory = selectedCategory;

            if(Input.GetButtonDown("Down"))
            {
                ++selectedItem;
            }
            else if(Input.GetButtonDown("Up"))
            {
                --selectedItem;
            }

            else if(Input.GetButtonDown("Left"))
            {
                --selectedCategory;
            }
            else if(Input.GetButtonDown("Right"))
            {
                ++selectedCategory;
            }

            //IF clamping...
            //clamp selectedCategory before selectedItem to prevent outofbounds exception
            //selectedCategory = Mathf.Clamp(selectedCategory, 0, Inventory.ItemCategories.Count - 1);
            
            //IF revolving...
            if(selectedCategory > Inventory.ItemCategories.Count - 1)
            {
                selectedCategory = 0;
            }
            else if(selectedCategory < 0)
            {
                selectedCategory = Inventory.ItemCategories.Count - 1;
            }

            selectedItem = Mathf.Clamp(selectedItem, 0, inventory.GetSlotsByCategory(selectedCategory).Count - 1);

            if(prevCategory != selectedCategory)
            {
                ResetSelection();
                categoryText.text = Inventory.ItemCategories[selectedCategory];
                UpdateItemList();
            }
            else if(prevSelection != selectedItem)
            {
                UpdateItemSelection();
            }

            if(Input.GetButtonDown("Submit"))
            {
                //OpenPartyScreen();

                //Input.GetButtonDown("Submit") of OpenPartyScreen() is being immediately triggered
                //so the below delay is to prevent that
                Action partyScreenAction = () =>
                {
                    OpenPartyScreen();
                };
                StartCoroutine(Delay(partyScreenAction, 0.1f));
            }
            else if(Input.GetButtonDown("Cancel"))
            {
                onBack?.Invoke();
            }
        }
        if(state == InventoryUIState.PartySelection)
        {
            Action onSelected = () =>
            {
                //use the item on the selected mon
                StartCoroutine(UseItem());
            };

            Action onBackPartyScreen = () =>
            {
                ClosePartyScreen();
            };
            partyScreen.HandleUpdate(onSelected, onBackPartyScreen);
        }

    }

    IEnumerator Delay(Action doThis, float delay)
    {
        yield return new WaitForSeconds(delay);
        doThis?.Invoke();
    }

    IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;

        var usedItem = inventory.UseItem(selectedItem, partyScreen.SelectedMember);
        if(usedItem != null)
        {
            yield return DialogManager.Instance.ShowDialogText($"The player used {usedItem.Name}");
            onItemUsed?.Invoke();
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"It wont have any effect!");
        }

        ClosePartyScreen();
    }

    private void UpdateItemSelection()
    {
        var slots = inventory.GetSlotsByCategory(selectedCategory);

        for(int i = 0; i < slotUIList.Count; i++)
        {
            if(i == selectedItem)
            {
                slotUIList[i].NameText.color = GlobalSettings.i.HighlightedColor;
            }
            else
            {
                slotUIList[i].NameText.color = GlobalSettings.i.UnhighlightedColor;
            }
        }

        //if last item in inventory is used and then removed, number of item slots will change but selectedItem will not,
        //resulting in an out of range exception.  clamping this here prevents that.
        selectedItem = Mathf.Clamp(selectedItem, 0, slots.Count - 1);

        if(slots.Count > 0)
        {
            var item = slots[selectedItem].Item;
            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }
        

        HandleScrolling();
    }

    private void HandleScrolling()
    {
        if(slotUIList.Count <= itemsInViewport)
        {
            return;
        }
        //calculate scrollPos based on height of slotUIList item and
        //don't start scrolling until selecting itemsInViewport/2 (i.e. second half of items on screen)
        float scrollPos = Mathf.Clamp(selectedItem - itemsInViewport/2, 0, selectedItem) * slotUIList[0].Height;
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

        //show arrows
        bool showUpArrow = selectedItem > itemsInViewport / 2;
        bool showDownArrow = selectedItem + itemsInViewport / 2 < slotUIList.Count;
        upArrow.gameObject.SetActive(showUpArrow);
        downArrow.gameObject.SetActive(showDownArrow);
    }

    private void ResetSelection()
    {
        selectedItem = 0;
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);
        itemIcon.sprite = null;
        itemDescription.text = "";
    }

    private void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
    }

    private void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelection;
        partyScreen.gameObject.SetActive(false);
    }
}
