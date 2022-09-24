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

    private Vector3 forwardMove;
    private Vector3 forwardForceDir;

    public DashPerform(CarController blackboard)
    {
        this.Blackboard = blackboard;
        dashMaxTime = Blackboard.DashSpeedCurve[Blackboard.DashSpeedCurve.length - 1].time;
    }

    public override void Init()
    {
        base.Init();
        dashCurrentTime = 0;
        forwardMove = Blackboard.Rb.transform.forward;
    }

    public override ENodeState Evaluate()
    {
        float currDashSpeed = Blackboard.DashSpeedCurve.Evaluate(dashCurrentTime);

        AllignToGround();
        if (Blackboard.BAnyWheelGrounded)
        {
            Blackboard.Rb.AddForceAtPosition(forwardForceDir * currDashSpeed, CalculateDashOffset(), ForceMode.Acceleration);

            // Blackboard.Rb.AddForceAtPosition(Vector3.down * Blackboard.Weight, CalculateDashOffset()); // add a downwards force so it does not flip
        }

        Blackboard.Motor = currDashSpeed;
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

    /// <summary>
    /// Get the forward direction so that the car's dash always aligns with the grounds normal.
    /// </summary>
    private void AllignToGround()
    {
        RaycastHit ground;
        if (Physics.Raycast(Blackboard.Rb.transform.position, -Vector3.up, out ground, 10.0f, ~Blackboard.PlayerLayer))
        {
            Vector3 lookDir = forwardMove;
            Vector3 up = ground.normal;

            lookDir.y = 0; // ignore all vertical height, so appears to be on flat ground
            lookDir.Normalize();

            // remove the up amount from the vector
            float d = Vector3.Dot(lookDir, up); // get the amount of the direction which was up, relative to the grounds normal
            lookDir -= d * up; // removes any upwards values, so the vectors now 90 degress to the normal and still heading in the right direction
            lookDir.Normalize();

            // convert the directional vector into a rotation
            Blackboard.Rb.transform.forward = lookDir;
            forwardForceDir = lookDir;
        }
    }
}