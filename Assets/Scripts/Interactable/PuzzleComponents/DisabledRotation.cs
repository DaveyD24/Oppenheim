using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// the rotation of the object when disabled.
/// </summary>
public class DisabledRotation : MonoBehaviour
{
    private Tween rotTween;
    [SerializeField] private Vector3 restRotation;
    [SerializeField] private float rotSpeed;
    [SerializeField] private AnimationCurve rotCurve;

    [SerializeField] private GameObject[] wireRacks;
    [SerializeField] private GameObject solidFloor;

    public void MoveRotation()
    {
        rotTween = new Tween(transform.rotation, Quaternion.Euler(restRotation), Time.time, rotSpeed);
        AdjustVisability(false);
    }

    public void CancelMove()
    {
        rotTween = null;
        AdjustVisability(true);
    }

    private void Update()
    {
        if (rotTween != null)
        {
            transform.rotation = rotTween.UpdateRotationCurve(rotCurve);
            if (rotTween.IsComplete())
            {
                transform.rotation = rotTween.EndRot;
                rotTween = null;
            }
        }
    }

    private void AdjustVisability(bool bWireVisible)
    {
        foreach (GameObject item in wireRacks)
        {
            item.SetActive(bWireVisible);
        }

        solidFloor.SetActive(!bWireVisible);
    }
}
