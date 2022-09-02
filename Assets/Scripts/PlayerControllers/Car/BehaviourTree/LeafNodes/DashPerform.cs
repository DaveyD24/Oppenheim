using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Apply the forces to the car when dashing.
/// </summary>
public class DashPerform : Node
{
    private float dashCurrentTime = 0;
    private float dashMaxTime;

    public DashPerform(CarController blackboard)
    {
        this.Blackboard = blackboard;
        dashMaxTime = Blackboard.DashSpeedCurve[Blackboard.DashSpeedCurve.length - 1].time;
    }

    public override void Init()
    {
        base.Init();
        dashCurrentTime = 0;
    }

    public override ENodeState Evaluate()
    {
        float currDashSpeed = Blackboard.DashSpeedCurve.Evaluate(dashCurrentTime);

        if (Blackboard.BAnyWheelGrounded)
        {
            Blackboard.Rb.AddForceAtPosition(Blackboard.Rb.transform.forward * currDashSpeed, CalculateDashOffset(), ForceMode.Acceleration);
        }

        Blackboard.Motor = currDashSpeed;
        dashCurrentTime += Time.fixedDeltaTime;
        if (dashCurrentTime >= dashMaxTime)
        {
            return ENodeState.Success;
        }

        return ENodeState.Running; // can be whatever retrun is nessesary
    }

    public override void End()
    {
        base.End();
    }

    /// <summary>
    /// offsets the position the dash force is applied to the player at using local space.
    /// </summary>
    /// <returns>the offset position.</returns>
    private Vector3 CalculateDashOffset()
    {
        Vector3 pos = Blackboard.transform.position;
        pos += Blackboard.transform.forward * Blackboard.DashOffset.z;
        pos += Blackboard.transform.right * Blackboard.DashOffset.x;
        pos += Blackboard.transform.up * Blackboard.DashOffset.y;

        return pos;
    }
}