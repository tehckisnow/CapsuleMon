using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogManager : MonoBehaviour
{
    [SerializeField] private GameObject dialogBox;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private int lettersPerSecond = 30;

    public event Action OnShowDialog;
    public event Action OnDialogFinished;

    public static DialogManager Instance { get; private set; }

    private bool isTyping = false;
    //private IEnumerator currentlyTypingDialog;
    private Coroutine currentlyTypingDialog;
    private string textIfSkipped = "";

    private void Awake()
    {
        Instance = this;
    }

    public bool IsShowing { get; private set; }

    private void Update()
    {
        if(Input.GetButtonDown("Submit"))
        {
            SkipTyping();
        }
    }

    //show only a single line instead of a dialog
    public IEnumerator ShowDialogText(string text, bool waitForInput=true, bool autoClose=true)
    {
        OnShowDialog?.Invoke();

        IsShowing = true;
        dialogBox.SetActive(true);

        //
        currentlyTypingDialog = StartCoroutine(TypeDialog(text));
        //yield return currentlyTypingDialog = StartCoroutine(TypeDialog(text));
        
        //yield return currentlyTypingDialog = TypeDialog(text);
        //yield return TypeDialog(text);

        yield return new WaitUntil(() => isTyping != true);
        yield return new WaitForSeconds(0.1f);
        
        if(waitForInput)
        {
            yield return new WaitUntil(() => Input.GetButtonDown("Submit"));
        }

        if(autoClose)
        {
            CloseDialog();
        }
        OnDialogFinished?.Invoke();
    }

    public void CloseDialog()
    {
        dialogBox.SetActive(false);
        IsShowing = false;
        currentlyTypingDialog = null;
        textIfSkipped = "";
    }

    public IEnumerator ShowDialog(Dialog dialog)
    {
        yield return new WaitForEndOfFrame();
        //the above line is to prevent the z key from mis-triggering from before

        OnShowDialog?.Invoke();
        IsShowing = true;
        dialogBox.SetActive(true);

        foreach(var line in dialog.Lines)
        {
            //
            currentlyTypingDialog = StartCoroutine(TypeDialog(line));
            //yield return currentlyTypingDialog = TypeDialog(line);
            //yield return TypeDialog(line);
            
            yield return new WaitUntil(() => isTyping != true);
            yield return new WaitForSeconds(0.1f);
            yield return new WaitUntil(() => Input.GetButtonDown("Submit"));
        }
        dialogBox.SetActive(false);
        IsShowing = false;
        OnDialogFinished?.Invoke();
    }

    public void HandleUpdate()
    {
        if(Input.GetButtonDown("Submit"))
        {
            SkipTyping();
        }
    }

    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        textIfSkipped = dialog;
        isTyping = true;
        foreach(var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f/lettersPerSecond);
        }
        isTyping = false;
    }

    private void SkipTyping()
    {
        if(isTyping)
        {
            StopCoroutine(currentlyTypingDialog);
            isTyping = false;
            dialogText.text = textIfSkipped;
        }
    }
}
