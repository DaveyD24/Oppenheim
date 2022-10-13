using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTowards : Node<GenericBot>
{
    private float previousDistToNode = 0;

    private Quaternion forwardOffset;

    public MoveTowards(GenericBot blackboard)
    {
        this.Blackboard = blackboard;
    }

    public override void Init()
    {
        base.Init();

        if (Blackboard.NextPosTransform != null)
        {
            previousDistToNode = NodeDistance();
        }

        forwardOffset = Quaternion.AngleAxis(0, Blackboard.transform.up);
    }

    public override ENodeState Evaluate()
    {
        if (Blackboard.NextPosTransform == null)
        {
            return ENodeState.Failure;
        }

        // if (!Blackboard.anim.GetCurrentAnimatorStateInfo(0).IsTag("isWalk")) //if not yet walking, initiate the walking animation
        //     blackboard.anim.SetTrigger("Walk");
        Blackboard.transform.position += Blackboard.transform.forward * Time.deltaTime * Blackboard.MovementSpeed;

        // Vector3 lookDir = forwardOffset * (Blackboard.NextPosVector - Blackboard.transform.position).normalized;// + (forwardOffset * blackboard.transform.right);
        Blackboard.transform.rotation = SetRotation();

        float currDist = NodeDistance(); // current distance away from the segment

        // successful if close to the node or already passed it between the last frame and this one
        if (currDist < 1 || currDist > previousDistToNode)
        {
            return ENodeState.Success;
        }
        else
        {
            previousDistToNode = currDist;
            return ENodeState.Running;
        }

    }

    private float NodeDistance()
    {
        return Vector3.Distance(Blackboard.transform.position, Blackboard.NextPosVector);
    }

    /// <summary>
    /// Orient the ant so it will be facing towards its goal point while also being flat on the ground.
    /// </summary>
    /// <returns>A smooth rotation towards this new orientation</returns>
    private Quaternion SetRotation()
    {
        // solved orientation using code from here https://forum.unity.com/threads/look-at-object-while-aligned-to-surface.515743/
        Vector3 up = SetGround(); // the the up position of the normal of the ground

        Vector3 lookDir = forwardOffset * (Blackboard.NextPosVector - Blackboard.transform.position);// + (forwardOffset*blackboard.transform.right);
        lookDir.y = 0; // ignore all vertical height, so appears to be on flat ground
        lookDir.Normalize();

        // remove the up amount from the vector
        float d = Vector3.Dot(lookDir, up); // get the amount of the direction which was up, relative to the grounds normal
        lookDir -= d * up; // removes any upwards values, so the vectors now 90 degress to the normal and still heading in the right direction
        lookDir.Normalize();

        // convert the directional vector into a rotation
        Quaternion targetRotation = Quaternion.LookRotation(lookDir, up);

        // smoothly rotate towards this point
        Quaternion smoothRotation = Quaternion.RotateTowards(Blackboard.transform.rotation, targetRotation, Time.deltaTime * Blackboard.RotationSpeed);

        return smoothRotation;
    }

    /// <summary>
    /// Set the ant so that it will always be on the ground.
    /// </summary>
    /// <returns>The normal of the ground used for orienting the bot correctly.</returns>
    private Vector3 SetGround()
    {
        RaycastHit raycastHit;
        bool didHit = Physics.Raycast(Blackboard.transform.position, -Vector3.up, out raycastHit, 15);

        // set the position of the bot to the ground
        if (didHit)
        {
            Vector3 groundPoint = Blackboard.transform.localPosition;
            groundPoint.y = raycastHit.point.y + Blackboard.GroundOffset;
            Blackboard.transform.localPosition = groundPoint;
        }

        // Vector3 upSmooth;
        // if (didHit1 && didHit)//when on the edge between two different triangles, get a vector which will point up ensuring a smooth rotation between the two
        //     upSmooth = Vector3.Cross(Blackboard.transform.right, -(raycastHit.point - raycastHit1.point).normalized);
        //// above used this link https://answers.unity.com/questions/1420677/best-way-to-rotate-player-object-to-match-the-grou.html
        // else
        //    upSmooth = Vector3.up;

// #if UNITY_EDITOR
//        Debug.DrawRay(blackboard.transform.position + blackboard.transform.forward * -0.5f , 5 * (blackboard.transform.forward- blackboard.transform.up * 0.1f), Color.green);
//        Debug.DrawRay(blackboard.transform.position, 5 * (raycastHit.normal), Color.blue);
//        Debug.DrawRay(blackboard.transform.position + blackboard.transform.forward * -blackboard.backGroundCheckOffset, 5 * (raycastHit1.normal), Color.blue);
// #endif
        return Vector3.up;
    }
}