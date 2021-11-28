using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject menu;

    public event Action<int> onMenuSelected;
    public event Action onBack;

    private List<TextMeshProUGUI> menuItems;

    private int selectedItem = 0;
    private Color unhighlightedColor;

    private void Awake()
    {
        //menu2 is the dynamic, selectable menu, as opposed to the static, player into panel below it
        var menu2 = menu.transform.GetChild(0);
        //use menu2 instead so that the static infopanel is not selectable
        menuItems = menu2.GetComponentsInChildren<TextMeshProUGUI>().ToList();
        unhighlightedColor = menuItems[0].color;
    }

    public void OpenMenu()
    {
        menu.SetActive(true);
        UpdateItemSelection();
    }

    public void CloseMenu()
    {
        menu.SetActive(false);
    }

    public void HandleUpdate()
    {
        int prevSelection = selectedItem;

        if(Input.GetButtonDown("Down"))
        {
            ++selectedItem;
        }
        if(Input.GetButtonDown("Up"))
        {
            --selectedItem;
        }

        selectedItem = Mathf.Clamp(selectedItem, 0, menuItems.Count - 1);

        if(prevSelection != selectedItem)
        {
            UpdateItemSelection();
        }

        if(Input.GetButtonDown("Submit"))
        {
            onMenuSelected?.Invoke(selectedItem);
            CloseMenu();
        }
        if(Input.GetButtonDown("Cancel"))
        {
            onBack?.Invoke();
            CloseMenu();
        }
    }

    private void UpdateItemSelection()
    {
        for(int i = 0; i < menuItems.Count; i++)
        {
            if(i == selectedItem)
            {
                menuItems[i].color = GlobalSettings.i.HighlightedColor;
            }
            else
            {
                menuItems[i].color = unhighlightedColor;
            }
        }
    }
}
