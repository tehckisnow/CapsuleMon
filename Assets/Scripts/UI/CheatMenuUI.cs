using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public enum CheatState { Normal, Customize }
public enum CheatMode { Money, Mon, Item}

public class CheatMenuUI : MonoBehaviour
{
    [SerializeField] GameObject cheatMenuUI;
    [SerializeField] GameObject listObject;
    [SerializeField] TextMeshProUGUI textPrefab;

    [SerializeField] GameObject cheatCustomizer;
    [SerializeField] GameObject cheatCustomizerListObject;
    [SerializeField] GameObject cheatCustomizerModifierObject;
    [SerializeField] TextMeshProUGUI modifierText;
    [SerializeField] TextMeshProUGUI labelText;
    [SerializeField] TextMeshProUGUI feedbackText;

    [SerializeField] List<ItemBase> cheatItems;
    [SerializeField] List<MonBase> cheatMons;

    private CheatState cheatState = CheatState.Normal;
    private List<TextMeshProUGUI> textOptionList;
    private List<Cheat> cheats;

    private CheatMode cheatMode;

    private List<TextMeshProUGUI> customizeTextOptionList;

    private GameState prevState;
    private bool isOpen = false;

    private int currentOption = 0;
    private int customizerListCurrentOption = 0;
    private int customizerModValue = 1;
    private Color unhighlightedColor;

    private Coroutine fadeCoroutine;

    public void Open()
    {
        if(isOpen)
        {
            Close();
        }
        isOpen = true;
        cheatMenuUI.SetActive(true);
        prevState = GameController.Instance.state;
        GameController.Instance.state = GameState.CheatMenu;
        CloseCheatCustomizer();
        unhighlightedColor = textPrefab.color;
        textOptionList = new List<TextMeshProUGUI>();
        customizeTextOptionList = new List<TextMeshProUGUI>();
        BuildCheats();
        WipeList();
        PopulateList();
        WipeCustomizeList();
        UpdateItemSelection();
    }

    public void Close()
    {
        isOpen = false;
        GameController.Instance.state = prevState;
        cheatMenuUI.SetActive(false);
    }

    public void HandleUpdate()
    {
        int prevSelection = currentOption;
        int prevCustomizerListCurrentOption = customizerListCurrentOption;
        int prevCustomizerModValue = customizerModValue;

        if(Input.GetButtonDown("Down"))
        {
            if(cheatState == CheatState.Normal)
            {
                currentOption++;
            }
            else if(cheatState == CheatState.Customize)
            {
                customizerListCurrentOption++;
            }
        }
        if(Input.GetButtonDown("Up"))
        {
            if(cheatState == CheatState.Normal)
            {
                currentOption--;
            }
            else if(cheatState == CheatState.Customize)
            {
                customizerListCurrentOption--;
            }
        }

        if(cheatState == CheatState.Customize)
        {
            if(Input.GetButtonDown("Left"))
            {
                if(cheatMode == CheatMode.Money)
                {
                    customizerModValue -= 100;
                }
                else
                {
                    customizerModValue--;
                }
            }
            if(Input.GetButtonDown("Right"))
            {
                if(cheatMode == CheatMode.Money)
                {
                    customizerModValue += 100;
                }
                else
                {
                    customizerModValue++;
                }
            }
        }

        if(cheatState == CheatState.Normal)
        {
            currentOption = Mathf.Clamp(currentOption, 0, textOptionList.Count - 1);
        }
        else if(cheatState == CheatState.Customize)
        {
            int top = 1;
            switch(cheatMode)
            {
                case CheatMode.Item:
                    top = cheatItems.Count - 1;
                    break;
                case CheatMode.Mon:
                    top = cheatMons.Count - 1;
                    break;
                case CheatMode.Money:
                    break;
                default:
                    break;
            }
            customizerListCurrentOption = Mathf.Clamp(customizerListCurrentOption, 0, top);
            
            if(customizerModValue < 0)
            {
                customizerModValue = 0;
            }
        }

        if(prevSelection != currentOption)
        {
            UpdateItemSelection();
        }
        if(cheatState == CheatState.Customize)
        {
            if(prevCustomizerListCurrentOption != customizerListCurrentOption || prevCustomizerModValue != customizerModValue)
            {
                UpdateCustomizeList();
            }
        }

        if(Input.GetButtonDown("Submit"))
        {
            if(cheatState == CheatState.Normal)
            {
                SelectOption();
            }
            else if(cheatState == CheatState.Customize)
            {
                RunCheat();
            }
        }

        if(Input.GetKeyDown(KeyCode.C))
        {
            if(cheatState == CheatState.Customize)
            {
                CloseCheatCustomizer();
            }
            Close();
        }
        if(Input.GetButtonDown("Cancel"))
        {
            if(cheatState == CheatState.Normal)
            {
                Close();
            }
            else if(cheatState == CheatState.Customize)
            {
                CloseCheatCustomizer();
            }
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
    }

    private void UpdateCustomizeList()
    {
        for(int i = 0; i < customizeTextOptionList.Count; i++)
        {
            if(i == customizerListCurrentOption)
            {
                customizeTextOptionList[i].color = GlobalSettings.i.HighlightedColor;
            }
            else
            {
                customizeTextOptionList[i].color = unhighlightedColor;
            }
        }

        //!
        modifierText.text = customizerModValue.ToString();

        HandleCustomScrolling();
    }
    private void HandleCustomScrolling()
    {

    }

    private void Feedback(string text)
    {
        feedbackText.color = unhighlightedColor;
        feedbackText.text = text;
        if(fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        var c = unhighlightedColor;
        Color newColor = new Color(c.r, c.g, c.b, c.a);
        float alpha = 1f;
        float increment = 0.1f;
        yield return new WaitForSeconds(1f);
        while(alpha > 0)
        {
            alpha -= increment;
            newColor.a = alpha;
            feedbackText.color = newColor;
            yield return new WaitForSeconds(0.1f);
        }
        newColor.a = 0;
    }

    private void RunCheat()
    {
        int i = customizerListCurrentOption;
        int m = customizerModValue;
        if(cheats[currentOption].giveType == GiveType.Mon)
        {
            cheats[currentOption].subject = cheatMons[i];
        }
        else if(cheats[currentOption].giveType == GiveType.Item)
        {
            cheats[currentOption].subject = cheatItems[i];
        }
        else if(cheats[currentOption].giveType == GiveType.Money)
        {
            //subject
            //m
        }
        cheats[currentOption].modifier = m;
        cheats[currentOption].UseCheat();
        
        Feedback($"{cheats[currentOption].name} {cheats[currentOption].subject.name}");

        customizerModValue = 1;
        UpdateCustomizeList();
    }

    private void SelectOption()
    {
        //cheats[currentOption].UseCheat();

        if(cheats[currentOption].cheatType == CheatType.Give)
        {
            switch(cheats[currentOption].giveType)
            {
                case GiveType.Item:
                    cheatMode = CheatMode.Item;
                    OpenCheatCustomizer();
                    break;
                case GiveType.Mon:
                    cheatMode = CheatMode.Mon;
                    OpenCheatCustomizer();
                    break;
                case GiveType.Money:
                    cheatMode = CheatMode.Money;
                    OpenCheatCustomizer();
                    break;
                case GiveType.None:
                default:
                    break;
            }
        }
        else if(cheats[currentOption].cheatType == CheatType.Action)
        {
            Debug.Log("using Action type cheat");
            cheats[currentOption].UseCheat();
            Feedback($"{cheats[currentOption].name}");
        }
    }

    private void OpenCheatCustomizer()
    {
        cheatCustomizer.SetActive(true);
        cheatState = CheatState.Customize;
        customizerModValue = 1;

        switch(cheatMode)
        {
            case CheatMode.Item:
                labelText.text = "amount:";
                break;
            case CheatMode.Mon:
                labelText.text = "level:";
                break;
            case CheatMode.Money:
                labelText.text = "$";
                break;
            default:
                labelText.text = "";
                break;
        }
        PopulateCustomizeList();
        UpdateCustomizeList();
    }

    private void CloseCheatCustomizer()
    {
        WipeCustomizeList();
        cheatCustomizer.SetActive(false);
        cheatState = CheatState.Normal;
    }

    private void WipeCustomizeList()
    {
        customizeTextOptionList = new List<TextMeshProUGUI>();
        foreach(Transform child in cheatCustomizerListObject.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void PopulateCustomizeList()
    {
        if(cheatMode == CheatMode.Mon)
        {
            for(int i=0; i < cheatMons.Count; i++)
            {
                var item = cheatMons[i];
                var newOption = Instantiate(textPrefab, cheatCustomizerListObject.transform);
                string optionText = $"{item.Name}";
                newOption.GetComponent<TextMeshProUGUI>().text = optionText;
                customizeTextOptionList.Add(newOption);
            }
        }
        else if(cheatMode == CheatMode.Item)
        {
            for(int i=0; i < cheatItems.Count; i++)
            {
                var item = cheatItems[i];
                var newOption = Instantiate(textPrefab, cheatCustomizerListObject.transform);
                string optionText = $"{item.Name}";
                newOption.GetComponent<TextMeshProUGUI>().text = optionText;
                customizeTextOptionList.Add(newOption);
            }
        }
        else if(cheatMode == CheatMode.Money)
        {
            return;
        }
    }

    private void WipeList()
    {
        textOptionList = new List<TextMeshProUGUI>();
        foreach(Transform child in listObject.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void PopulateList()
    {
        for(int i=0; i < cheats.Count; i++)
        {
            var item = cheats[i];
            var newOption = Instantiate(textPrefab, listObject.transform);
            string optionText = $"{item.name}";
            newOption.GetComponent<TextMeshProUGUI>().text = optionText;
            textOptionList.Add(newOption);
        }
    }

    private void BuildCheats()
    {
        cheats = new List<Cheat>()
        {
            new Cheat()
            {
                name = "Give Item", cheatType = CheatType.Give, giveType = GiveType.Item, subject = cheatItems[0], modifier = 1
            },
            new Cheat()
            {
                name = "Give Money", cheatType = CheatType.Give, giveType = GiveType.Money, modifier = 100
            },
            new Cheat()
            {
                name = "Give Mon", cheatType = CheatType.Give, giveType = GiveType.Mon, subject = cheatMons[0], modifier = 5
            },
            new Cheat()
            {
                name = "Heal Party", cheatType = CheatType.Action, giveType = GiveType.None, subject = null, modifier = 1, action = HealMons.HealPlayerParty
            }
        };
    }
    
}

[System.Serializable]
public class Cheat
{
    public string name;
    public CheatType cheatType;
    public GiveType giveType;
    public UnityEngine.Object subject;
    public int modifier; //amount of item, money, level of mon
    public Action action;

    public void UseCheat()
    {
        if(cheatType == CheatType.Give)
        {
            if(giveType == GiveType.Item)
            {
                var item = subject as ItemBase;
                var inv = PlayerController.Instance.GetComponent<Inventory>();
                inv.AddItem(item, modifier);
            }
            else if(giveType == GiveType.Mon)
            {
                var monBase = subject as MonBase;
                var mon = new Mon(monBase, modifier);
                mon.Init();
                MonParty.GetPlayerParty().AddMon(mon);
            }
            else if(giveType == GiveType.Money)
            {
                PlayerController.Instance.Money += modifier;
            }
        }
        else if(cheatType == CheatType.Action)
        {
            Debug.Log("Invoking action");
            action?.Invoke();
        }
        else if(cheatType == CheatType.Setting)
        {

        }
    }
}

public enum CheatType { Give, Action, Setting }
public enum GiveType { None, Item, Mon, Money }
