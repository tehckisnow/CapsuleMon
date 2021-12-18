using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShopUI : MonoBehaviour
{
    [SerializeField] List<ShopItem> items;

    [SerializeField] GameObject shopUI;
    [SerializeField] GameObject listObject;
    [SerializeField] TextMeshProUGUI textPrefab;
    [SerializeField] int itemsInViewport;
    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;
    [SerializeField] TextMeshProUGUI itemNameText;
    [SerializeField] TextMeshProUGUI amountText;
    [SerializeField] TextMeshProUGUI totalCostText;
    [SerializeField] TextMeshProUGUI playersMoneyText;
    [SerializeField] TextMeshProUGUI itemDescText;
    [SerializeField] TextMeshProUGUI martTitleText;
    //textholderRect

    private Shop shop;

    private List<TextMeshProUGUI> textOptionList;
    private int currentOption = 0;
    private int amount = 1;
    private Color unhighlightedColor;
    private GameState prevState;

    private void Awake()
    {
        textOptionList = new List<TextMeshProUGUI>();
        unhighlightedColor = textPrefab.color;
    }

    public void Open(Shop newShop)
    {
        //open UI
        martTitleText.text = newShop.ShopName;
        shopUI.SetActive(true);
        prevState = GameController.Instance.state;
        GameController.Instance.state = GameState.Shop;
        shop = newShop;
        currentOption = 0;
        playersMoneyText.text = $"${PlayerController.Instance.Money}";
        amount = 1;
        amountText.text = "x1";
        WipeList();
        PopulateList();
        UpdateItemSelection();
    }

    private void WipeList()
    {
        //clear list
        textOptionList = new List<TextMeshProUGUI>();
        foreach(Transform child in listObject.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void PopulateList()
    {
        //populate list
        for(int i = 0; i < shop.Items.Count; i++)
        {
            var item = shop.Items[i];
            var newOption = Instantiate(textPrefab, listObject.transform);
            string listing = $"{item.Item.Name}: ${item.Value}";
            newOption.GetComponent<TextMeshProUGUI>().text = listing;
            textOptionList.Add(newOption);
        }
    }

    private void RefreshList()
    {
        WipeList();
        PopulateList();
    }

    public void HandleUpdate()
    {
        int prevSelection = currentOption;
        int prevAmount = amount;

        if(Input.GetButtonDown("Down"))
        {
            currentOption++;
            amount = 0;
        }
        if(Input.GetButtonDown("Up"))
        {
            currentOption--;
            amount = 0;
        }

        if(Input.GetButtonDown("Left"))
        {
            amount--;
        }
        if(Input.GetButtonDown("Right"))
        {
            amount++;
        }

        currentOption = Mathf.Clamp(currentOption, 0, textOptionList.Count - 1);
        amount = Mathf.Clamp(amount, 1, 10);

        if(prevSelection != currentOption || prevAmount != amount)
        {
            UpdateItemSelection();
        }

        if(Input.GetButtonDown("Submit"))
        {
            OpenPurchaseConfirmation();
        }

        if(Input.GetButtonDown("Cancel"))
        {
            CloseShop();
        }
    }

    private void UpdateItemSelection()
    {
        for(int i = 0; i < textOptionList.Count; i++)
        {
            if(i == currentOption)
            {
                textOptionList[i].color = GlobalSettings.i.HighlightedColor;
            }
            else
            {
                textOptionList[i].color = unhighlightedColor;
            }
        }

        amountText.text = $"x{amount}";
        ShopItem shopItem = shop.Items[currentOption];
        int total = shopItem.Value * amount;
        string itemName = shopItem.Item.Name;
        itemNameText.text = itemName;
        itemDescText.text = shopItem.Item.Description;
        totalCostText.text = $"${total}";
        playersMoneyText.text = $"${PlayerController.Instance.Money}";
        if(PlayerController.Instance.Money >= total)
        {
            totalCostText.color = unhighlightedColor;
        }
        else
        {
            totalCostText.color = Color.red;
        }

        HandleScrolling();
    }

    private void HandleScrolling()
    {
        if(textOptionList.Count <= itemsInViewport)
        {
            upArrow.gameObject.SetActive(false);
            downArrow.gameObject.SetActive(false);
            return;
        }
        //calculate scrollPos based on height of textOptionList[0] and
        //dont start scrolling until selecting itemsinviewport/2
        float height = textOptionList[0].preferredHeight;
        float scrollPos = Mathf.Clamp(currentOption - itemsInViewport / 2, 0, currentOption) * height;

        //textHolderRect
        listObject.transform.localPosition = new Vector2(listObject.transform.localPosition.x, scrollPos);

        bool showUpArrow = currentOption > itemsInViewport / 2;
        bool showDownArrow = currentOption + itemsInViewport / 2 < textOptionList.Count;
        upArrow.gameObject.SetActive(showUpArrow);
        downArrow.gameObject.SetActive(showDownArrow);
    }

    public void OpenPurchaseConfirmation()
    {
        ItemBase item = shop.Items[currentOption].Item;
        int itemValue = shop.Items[currentOption].Value;
        int total = itemValue * amount;
        string plural = "";
        if(amount > 1)
        {
            plural += "s";
        }
        string message = $"Do you want to purchase {amount.ToString()} {item.Name}{plural}?";
        Action yesAction = () =>
        {
            PurchaseItem(total, item, amount);
            GameController.Instance.RevertFromDialogTo(GameState.Shop);
        };
        Action noAction = () =>
        {
            GameController.Instance.state = GameState.Shop;
        };

        GameController.Instance.OpenConfirmationMenu(message, yesAction, noAction);
    }

    public void PurchaseItem(int cost, ItemBase item, int amount=1)
    {
        string message = "";
        //handle transaction here
        if(PlayerController.Instance.SpendMoney(cost))
        {
            var inventory = PlayerController.Instance.GetComponent<Inventory>();
            inventory.AddItem(item, amount);
            string plural = "";
            if(amount > 1)
            {
                plural += "s";
            }
            message = $"Purchased {amount} {item.Name}{plural}";
        }
        else
        {
            message = "Not enough funds.";
        }
        
        //show result of transaction
        StartCoroutine(DialogManager.Instance.ShowDialogText(message));

        //since shop items are currently infinite, these are unneeded
        //RefreshList();
        UpdateItemSelection();
    }

    public void CloseShop()
    {
        GameController.Instance.state = prevState;
        shopUI.SetActive(false);
    }
}
