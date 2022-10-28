using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCameraTrans : Node<IntroTransition>
{

    [SerializeField] private Vector3[] camPositions;
    private int currentPosAt = 0;
    private float duration;

    private Tween camMoveTween;

    public MoveCameraTrans(IntroTransition blackboard, Vector3[] camPositions, float duration)
    {
        this.Blackboard = blackboard;
        this.camPositions = camPositions;
        this.duration = duration;
    }

    // run whenever this node gets called, and it was not the node running the previous frame
    public override void Init()
    {
        base.Init();
        currentPosAt = 0;

        camMoveTween = new Tween(camPositions[currentPosAt], camPositions[currentPosAt + 1], Time.time, duration / (camPositions.Length - 1));
    }

    // called each update frame that this node returns running fall
    public override ENodeState Evaluate()
    {
        if (camMoveTween != null)
        {
            Blackboard.Camera.transform.position = camMoveTween.UpdatePosition();

            Blackboard.Camera.transform.LookAt(camPositions[currentPosAt + 1]);

            if (camMoveTween.IsComplete() && currentPosAt + 1 < camPositions.Length - 1)
            {
                currentPosAt++;
                camMoveTween = new Tween(camPositions[currentPosAt], camPositions[currentPosAt + 1], Time.time, duration / (camPositions.Length - 1));
            }

            if (camMoveTween.IsComplete())
            {
                camMoveTween = null;
                return ENodeState.Success;
            }
        }

        return ENodeState.Running; // can be whatever return is nessesary
    }

    // when this node ends its execution, by either returning a failed or success node state
    public override void End()
    {
        base.End();
    }
}
