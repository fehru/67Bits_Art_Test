using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager<T> : MonoBehaviour where T : Component
{
    protected T Context;
    protected State<T> CurrentState;
    protected List<State<T>> Substates = new List<State<T>>();

    protected virtual void Update ()
    {
        CurrentState.UpdateState(Context);
        UpdateSubstates();
    }
    public virtual void SwichState (State<T> newState)
    {
        if (CurrentState.ExitState(Context))
        {
            CurrentState = newState;
            CurrentState.EnterState(Context);
        }
        else Debug.LogWarning("Couldn't exit state" + CurrentState.name);
    }
    public virtual void UpdateSubstates()
    {
        for (int i = 0; i < Substates.Count; i++) Substates[i].UpdateState(Context);
    }
    public virtual void AddNewSubstate(State<T> substate)
    {
        if (!Substates.Contains(substate))
        {
            Substates.Add(substate);
            substate.EnterState(Context);
        }
    }
    public virtual void RemoveSubstate(State<T> substate)
    {
        if(substate.ExitState(Context))
            Substates.Remove(substate);
    }
}
