using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//runs actions calls from async function calls
public class MainThread : MonoBehaviour
{
    //this is public static so as to be thread-safe
    public static Queue<UnityEvent> actions;

    public static MainThread Instance { get; private set; }

    [SerializeField] int updateFrequency = 20;
    private int updateFrequencyCount;

    void Awake()
    {
        Instance = this;
        actions = new Queue<UnityEvent>();
        updateFrequencyCount = updateFrequency;
    }

    void Update()
    {
        updateFrequencyCount--;
        if(updateFrequencyCount <= 0)
        {
            updateFrequencyCount = updateFrequency;
            while(actions.Count > 0)
            {
                var nextAction = actions.Dequeue();
                nextAction?.Invoke();
            }
        }
    }

    public void Add(UnityEvent action)
    {
        actions.Enqueue(action);
    }
}
