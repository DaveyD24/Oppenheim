using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Play the animation when transitioning to and from the dash.
/// </summary>
public class DashTransition : Node<CarController>
{
    private float maxTime;
    private float initTime = 0;
    private Quaternion startAngle;

    private int direction;
    private bool bIsEnd;

    private Material defaultMat;

    Vector3 forwardMove;

    public DashTransition(CarController blackboard, int direction, bool isEnd = false)
    {
        this.Blackboard = blackboard;
        this.direction = direction;

        bIsEnd = isEnd;

        defaultMat = Blackboard.CarMaterials[0];
        maxTime = Blackboard.TransitionRotCurve[Blackboard.TransitionRotCurve.length - 1].time;
        startAngle = Blackboard.BodyTransform.localRotation;
    }

    public override void Init()
    {
        initTime = 0;
        forwardMove = Blackboard.transform.forward;

        if (bIsEnd)
        {
            Blackboard.BAllowEndBreaking = true;
            Blackboard.Rb.mass = Blackboard.Weight;
        }
        else
        {
            Blackboard.Rb.mass = Blackboard.Weight / 2.5f;
        }

        base.Init();
    }

    public override ENodeState Evaluate()
    {
        initTime += Time.deltaTime;

        Vector3 currentAngle;
        currentAngle = new Vector3(startAngle.eulerAngles.x, startAngle.eulerAngles.y, startAngle.eulerAngles.z + (direction * Blackboard.TransitionRotCurve.Evaluate(initTime)));

        Blackboard.BodyTransform.localRotation = Quaternion.Euler(currentAngle);

        AllignToGround();
        Blackboard.Rb.angularVelocity = Vector3.zero;
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
            Blackboard.BAllowEndBreaking = false;
            Blackboard.CarMaterials[0] = defaultMat;

            if (Blackboard.BCancelDash)
            {
                Blackboard.BCancelDash = false;
            }
        }
        else
        {
            Blackboard.CarMaterials[0] = Blackboard.DashBodyMaterial;

            Blackboard.Rb.angularVelocity = Vector3.zero;
            Blackboard.Motor = 0;
        }

        // Blackboard.Rb.velocity = Vector3.zero;
        Blackboard.Rb.angularVelocity = Vector3.zero;
        Blackboard.BodyMeshRenderer.sharedMaterials = Blackboard.CarMaterials;

        base.End();
    }

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
        }
    }
}