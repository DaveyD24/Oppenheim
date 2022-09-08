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
            Blackboard.Rb.AddForce(Vector3.down * Blackboard.Weight); // add a downwards force so it does not flip
        }

        // Blackboard.Motor = currDashSpeed;
        dashCurrentTime += Time.fixedDeltaTime;
        CancleSidewaysVelocity();

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

    /// <summary>
    /// elliminate all sideways velocity and rotation velocities.
    /// </summary>
    private void CancleSidewaysVelocity()
    {
        // remove all sideways movement from the local movement space
        Vector3 localVelocity = Blackboard.transform.InverseTransformDirection(Blackboard.Rb.velocity);
        localVelocity.x = 0;
        Blackboard.Rb.velocity = Blackboard.transform.TransformDirection(localVelocity);

        // remove all angular velocity which is not in the local forward direction
        Vector3 localAngleVelocity = Blackboard.transform.InverseTransformDirection(Blackboard.Rb.angularVelocity);
        localAngleVelocity.y = 0;
        localAngleVelocity.z = 0;
        Blackboard.Rb.angularVelocity = Blackboard.transform.TransformDirection(localAngleVelocity);
    }
}