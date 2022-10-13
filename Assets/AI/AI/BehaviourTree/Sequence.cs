using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequence<T> : Node<T>
    where T : class
{
    private int currChild = 0;
    private List<Node<T>> children = new List<Node<T>>();

    public Sequence(List<Node<T>> children)
    {
        this.children = children; // when this class initialized set the list to be the one created upon initilization
    }

    protected List<Node<T>> Children
    {
        get => children; set => children = value;
    }

    public override ENodeState Evaluate()
    {
        while (currChild < children.Count)
        {
            switch (children[currChild].Execute())
            {
                case ENodeState.Running:
                    NodeState = ENodeState.Running;
                    return NodeState;
                case ENodeState.Success:
                    NodeState = ENodeState.Success;
                    children[currChild].End();
                    currChild++;
                    break;
                case ENodeState.Failure:
                    children[currChild].End();
                    currChild = 0;
                    NodeState = ENodeState.Failure;
                    End();
                    return NodeState;
            }
        }

        NodeState = ENodeState.Success; // as every node has succesfully run the whole thing was a success
        End();
        currChild = 0;
        return NodeState;
    }

    public override void Interupt() // force any all nodes to stop any execution
    {
        base.Interupt();

        currChild = 0;
        foreach (Node<T> childNode in children)
        {
            childNode.Interupt();
        }
    }
}
