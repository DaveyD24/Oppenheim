using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Play the animation when transitioning to and from the dash.
/// </summary>
public class DashTransition : Node
{
    private float maxTime;
    private float initTime = 0;
    private Quaternion startAngle;

    private int direction;
    private bool bIsEnd;

    private Material defaultMat;

    public DashTransition(CarController blackboard, int direction, bool isEnd = false)
    {
        this.Blackboard = blackboard;
        this.direction = direction;

        bIsEnd = isEnd;

        defaultMat = Blackboard.CarMaterials[0];
        maxTime = Blackboard.TransitionRotCurve[Blackboard.TransitionRotCurve.length - 1].time;
    }

    public override void Init()
    {
        Blackboard.Rb.mass = Blackboard.Weight / 2.5f;
        initTime = 0;

        startAngle = Blackboard.BodyTransform.localRotation;

        if (!bIsEnd)
        {
            Blackboard.Rb.velocity = Vector3.zero;
            Blackboard.Motor = 0;
        }

        base.Init();
    }

    public override ENodeState Evaluate()
    {
        initTime += Time.deltaTime;

        Vector3 currentAngle = startAngle.eulerAngles;
        currentAngle = new Vector3(startAngle.eulerAngles.x, startAngle.eulerAngles.y, startAngle.eulerAngles.z + (direction * Blackboard.TransitionRotCurve.Evaluate(initTime)));

        Blackboard.BodyTransform.localRotation = Quaternion.Euler(currentAngle);

        if (Blackboard.BAnyWheelGrounded)
        {
            // Blackboard.Rb.AddForce(Vector3.down * Blackboard.Weight); // add a downwards force so it does not flip
        }

        if (initTime >= maxTime)
        {
            return ENodeState.Success; // can be whatever retrun is nessesary
        }

        return ENodeState.Running;
    }

    public override void End()
    {
        Blackboard.BodyTransform.localRotation = startAngle;

        if (bIsEnd)
        {
            Blackboard.BIsDash = false;
            Blackboard.CarMaterials[0] = defaultMat;
        }
        else
        {
            Blackboard.CarMaterials[0] = Blackboard.DashBodyMaterial;
        }

        Blackboard.BodyMeshRenderer.sharedMaterials = Blackboard.CarMaterials;

        base.End();
    }
}