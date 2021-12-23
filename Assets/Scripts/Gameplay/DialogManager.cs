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

    private bool messageQueueReady = true;
    private Queue<Message> messageQueue;

    private bool isTyping = false;
    private Coroutine currentlyTypingDialog;
    private string textIfSkipped = "";

    private void Awake()
    {
        Instance = this;
        messageQueue = new Queue<Message>();
    }

    public bool IsShowing { get; private set; }

    private void Update()
    {
        if(Input.GetButtonDown("Submit"))
        {
            SkipTyping();
        }

        StartCoroutine(DequeueDialog());
    }

    public void QueueDialog(Dialog dialog)
    {
        Message message = new Message(dialog);
        messageQueue.Enqueue(message);
    }

    // This can be called with yield return in order to stall a coroutine until the message has displayed
    public IEnumerator QueueDialogCoroutine(Dialog dialog)
    {
        Message message = new Message(dialog);
        messageQueue.Enqueue(message);
        yield return new WaitUntil(() => message.Processed);
    }

    public void QueueDialogText(string dialog, bool waitForInput=true, bool autoClose=true)
    {
        Message message = new Message(dialog, waitForInput, autoClose);
        messageQueue.Enqueue(message);
    }

    // This can be called with yield return in order to stall a coroutine until the message has displayed
    public IEnumerator QueueDialogTextCoroutine(string dialog, bool waitForInput=true, bool autoClose=true)
    {
        Debug.Log(dialog);
        Message message = new Message(dialog, waitForInput, autoClose);
        messageQueue.Enqueue(message);
        yield return new WaitUntil(() => message.Processed);
    }

    private void ReadyQueue()
    {
        messageQueueReady = true;
        OnDialogFinished -= ReadyQueue;
    }

    private IEnumerator DequeueDialog()
    {
        while(messageQueue.Count > 0)
        {
            messageQueueReady = false;
            var message = messageQueue.Dequeue();
            if(message.IsDialog)
            {
                OnDialogFinished += ReadyQueue;
                //yield return ShowDialog(message.Dialog);
                yield return ShowDialog(message.Dialog);
            }
            else
            {
                OnDialogFinished += ReadyQueue;
                //yield return ShowDialogText(message.Text);
                yield return ShowDialogText(message.Text, message.WaitForInput, message.AutoClose);
            }
            message.Process();

            yield return null;
        }

        //CloseDialog();
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
        ReadyQueue();
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
            currentlyTypingDialog = StartCoroutine(TypeDialog(line));
            
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
        ReadyQueue();
    }

    private void SkipTyping()
    {
        if(isTyping)
        {
            StopCoroutine(currentlyTypingDialog);
            isTyping = false;
            dialogText.text = textIfSkipped;
            //!message.Process();
            ReadyQueue();
        }
    }
}

public class Message
{
    private bool isDialog = true;
    private Dialog dialog;
    private string text = "";
    private bool processed = false;
    private bool waitForInput;
    private bool autoClose;

    public bool IsDialog => isDialog;
    public Dialog Dialog => dialog;
    public string Text => text;
    public bool Processed => processed;
    public bool WaitForInput => waitForInput;
    public bool AutoClose => autoClose;

    public Message(Dialog _dialog)
    {
        dialog = _dialog;
        isDialog = true;
        text = "";

        LogMessage();
    }

    public Message(string _text, bool _waitForInput=true, bool _autoClose=true)
    {
        text = _text;
        isDialog = false;
        dialog = null;
        waitForInput = _waitForInput;
        autoClose = _autoClose;

        LogMessage();
    }

    public void Process()
    {
        processed = true;
    }

    public void LogMessage()
    {
        if(isDialog)
        {
            if(dialog != null)
            {
                foreach(string line in dialog.Lines)
                {
                    Debug.Log(line);
                }
            }
        }
        else
        {
            Debug.Log(text);
        }
    }
}
