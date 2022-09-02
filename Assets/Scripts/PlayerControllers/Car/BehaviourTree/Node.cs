using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Node
{
    private CarController blackboard;

    /// <summary>
    /// Success
    /// Failure
    /// Running.
    /// </summary>
    public enum ENodeState
    {
        Success,
        Failure,
        Running
    }

    public CarController Blackboard
    {
        get => blackboard; set => blackboard = value;
    }

    public ENodeState NodeState { get; set; }

    protected bool IsRunning { get; private set; } = false;

    public virtual void Init() // called on first frame node runs, and only called again once node fully completed runthrough
    {
        IsRunning = true;
    }

    public ENodeState Execute()
    {
        if (!IsRunning)
        {
            Init();
        }

        return Evaluate();
    }

    public abstract ENodeState Evaluate(); // determine the state the node is in upon completion, what the node does while it is running

    public virtual void End() // called whenever the node stops running
    {
        IsRunning = false;
    }

    public virtual void Interupt() // called when node is interupted and forced to stop execution
    {
        End();
    }
}
