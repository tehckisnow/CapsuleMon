using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] LevelLoader levelLoader;

    [Header("Elements")]
    [SerializeField] TextMeshProUGUI textSpeedSettingText;
    [SerializeField] TextMeshProUGUI textSpeedValueText;

    [SerializeField] TextMeshProUGUI battleAnimationSettingText;
    [SerializeField] TextMeshProUGUI battleAnimationValueText;

    [SerializeField] TextMeshProUGUI soundSettingText;
    [SerializeField] TextMeshProUGUI soundValueText;

    [SerializeField] TextMeshProUGUI printSettingText;
    [SerializeField] TextMeshProUGUI printValueText;

    private List<SettingOption> options;
    private int selectedItem = 0;

    private void Start()
    {
        options = new List<SettingOption>()
        {
            new SettingOption("Text Speed", new List<string>() {"Slow", "Mid", "Fast"}, textSpeedSettingText, textSpeedValueText),
            new SettingOption("Battle Animation", new List<string>() {"On", "Off"}, battleAnimationSettingText, battleAnimationValueText),
            new SettingOption("Sound", new List<string>() {"Mono", "Stereo"}, soundSettingText, soundValueText),
            new SettingOption("Print", new List<string>() {"Light", "Dark", "Darker", "Darkest"}, printSettingText, printValueText)
        };

        foreach(SettingOption option in options)
        {
            option.Load();
        }

        foreach(SettingOption option in options)
        {
            option.UpdateUI();
        }
        options[selectedItem].SetSelected(true);
    }

    private void Update()
    {
        HandleUpdate();
    }

    public void HandleUpdate()
    {
        if(Input.GetButtonDown("Down"))
        {
            options[selectedItem].SetSelected();
            selectedItem++;
        }
        else if(Input.GetButtonDown("Up"))
        {
            options[selectedItem].SetSelected();
            selectedItem--;
        }

        selectedItem = Mathf.Clamp(selectedItem, 0, options.Count - 1);

        options[selectedItem].SetSelected(true);

        if(Input.GetButtonDown("Left"))
        {
            options[selectedItem].ModIndex(-1);
        }
        else if(Input.GetButtonDown("Right"))
        {
            options[selectedItem].ModIndex(1);
        }

        if(Input.GetButtonDown("Cancel"))
        {
            SaveSettings();
            levelLoader.LoadStartMenu();
        }
    }

    private void SaveSettings()
    {
        foreach(SettingOption option in options)
        {
            option.Save();
        }
    }

}

public class SettingOption
{
    private string name;
    private List<string> values;
    private int currentValueIndex = 0;
    private TextMeshProUGUI settingText;
    private TextMeshProUGUI valueText;
    private bool selected = false;

    public string Name => name;
    public string CurrentValue => values[currentValueIndex];

    public SettingOption(string name, List<string> values, TextMeshProUGUI settingText, TextMeshProUGUI valueText)
    {
        this.name = name;
        this.values = values;
        this.settingText = settingText;
        this.valueText = valueText;
    }

    public void ModIndex(int val)
    {
        currentValueIndex += val;

        currentValueIndex = Mathf.Clamp(currentValueIndex, 0, values.Count - 1);

        UpdateUI();
    }

    public void SetSelected(bool val=false)
    {
        selected = val;
        UpdateUI();
    }

    public void UpdateUI()
    {
        settingText.text = name;
        valueText.text = CurrentValue;

        if(selected)
        {
            settingText.color = GlobalSettings.i.HighlightedColor;
        }
        else
        {
            settingText.color = GlobalSettings.i.UnhighlightedColor;
        }
    }

    public void Save()
    {
        PlayerPrefs.SetInt(name, currentValueIndex);
    }

    public void Load()
    {
        if(PlayerPrefs.HasKey(name))
        {
            currentValueIndex = PlayerPrefs.GetInt(name);
            UpdateUI();
        }
    }
}
