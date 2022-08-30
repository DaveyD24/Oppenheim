using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    this is just a generic function showcasing the base things each leaf node is required to contain to operate properly
    can simply copy this code into any leaf node and modify as needed
 */
public class GenericLeaf : Node
{
    public GenericLeaf(CarController blackboard)
    {
        this.Blackboard = blackboard;
    }

    public override void Init()
    {
        base.Init();
    }

    public override ENodeState Evaluate()
    {
        return ENodeState.Running; // can be whatever retrun is nessesary
    }

    public override void End()
    {
        base.End();
    }
}