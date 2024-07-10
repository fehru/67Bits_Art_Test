using UnityEngine;

public abstract class State<T> : MonoBehaviour where T : Component
{
    /// <summary>
    /// Runs when entering the state
    /// </summary>
    public abstract void EnterState(T component);
    /// <summary>
    /// Runs every update while the state is active
    /// </summary>
    public abstract void UpdateState(T component);
    /// <summary>
    /// Runs before exiting state and returns if was successfull
    /// </summary>
    /// <returns></returns>
    public abstract bool ExitState(T component);
    /// <summary>
    /// Check if the state should switch
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public abstract bool CheckStateSwitch(T component);
}
