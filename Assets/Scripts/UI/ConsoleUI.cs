using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConsoleUI : MonoBehaviour
{
    [SerializeField] GameObject inputUI;
    [SerializeField] TextMeshProUGUI log;
    [SerializeField] Image logBG;
    [SerializeField] TMP_InputField input;
    [SerializeField] string echoChar = ">";
    [SerializeField] string outputText = ":";
    [SerializeField] int scrollLines = 20;

    private float textHeight;
    private float padding = 0f;
    private int lines = 0;

    private GameState prevState;

    private void Awake()
    {
        textHeight = log.preferredHeight;
    }

    public void Open()
    {
        prevState = GameController.Instance.state;
        GameController.Instance.state = GameState.Console;

        inputUI.SetActive(true);
        input.Select();
        input.ActivateInputField();
    }

    public void HandleUpdate()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            Submit();
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    public void Close()
    {
        GameController.Instance.state = prevState;
        inputUI.SetActive(false);
    }

    private void AppendToLog(string text)
    {
        log.text += $"\n{echoChar} {text}";
        GrowRect(logBG.gameObject);
        GrowRect(inputUI);

        lines++;
        if(lines >= scrollLines)
        {
            inputUI.transform.position += new Vector3(0, textHeight + padding, 0);
        }
    }

    private void GrowRect(GameObject obj)
    {
        RectTransform objRect = obj.GetComponent<RectTransform>();
        float width = objRect.sizeDelta.x;
        float height = objRect.sizeDelta.y + textHeight + padding;
        objRect.sizeDelta = new Vector2(width, height);
    }

    private void ProcessCommand(string command, bool output=false)
    {
        string text = "";
        if(output)
        {
            text += outputText;
        }
        text += command;
        AppendToLog(text);
        input.text = "";
        input.Select();
        input.ActivateInputField();
    }

    public void Submit()
    {
        var command = input.text.ToLower();
        switch(command)
        {
            case "cheatson":
                ProcessCommand(command);
                ProcessCommand("Cheats Activated", true);
                PlayerController.Instance.CheatsOn();
                Close();
                break;
            case "cheatsoff":
                ProcessCommand(command);
                ProcessCommand("Cheats Deactivated", true);
                PlayerController.Instance.CheatsOff();
                Close();
                break;
            default:
                ProcessCommand(command);
                break;
        }
    }
}
