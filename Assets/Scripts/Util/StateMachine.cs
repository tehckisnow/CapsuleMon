using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StateMachine
{
    private int currentStateInt = -1;
    public int CurrentStateInt => currentStateInt;

    private Dictionary<int, State> states;
    private State currentState;

    private void Awake()
    {
        states = new Dictionary<int, State>();
    }

    public void SetState(int newState)
    {
        //disable/default
        if(newState == -1)
        {
            currentStateInt = newState;
            return;
        }
        else if(states[newState] != null)
        {
            currentStateInt = newState;
            currentState = states[newState];
        }

        for(int i = 0; i < states.Count - 1; i++)
        {
            if(states[i].StateInt == currentStateInt)
            {
                states[i].SetCurrentlyRunning(true);
                //! Maybe trigger Action here?
            }
            else
            {
                states[i].SetCurrentlyRunning();
            }
        }
    }

    //Called by SetAction
    private void AddNewState(int stateInt, State newState)
    {
        states.Add(stateInt, newState);
    }

    // Use this to set new Actions
    public void SetAction(int stateInt, Action action)
    {
        var newState = new State(stateInt, action);
        AddNewState(stateInt, newState);
    }

    private void Update()
    {
        //disabled/default state
        if(currentStateInt == -1)
        {
            return;
        }
        else if(currentState.CurrentlyRunning)
        {
            //currentState.Action?.Invoke();
            //!This is obviously wrong
        }
    }
}

public class State
{
    private int stateInt;
    public int StateInt => stateInt;
    private Action action;
    public Action Action => action;
    private bool currentlyRunning;
    public bool CurrentlyRunning => currentlyRunning;

    public State(int stateInt, Action action)
    {
        this.stateInt = stateInt;
        this.action = action;
    }

    public void SetCurrentlyRunning(bool val=false)
    {
        currentlyRunning = val;
        //! set StateMachine.currentState to null from here?
    }
}
