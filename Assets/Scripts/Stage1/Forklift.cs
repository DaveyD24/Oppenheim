using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forklift : MonoBehaviour, IDataInterface
{
    [SerializeField] private float moveSpeed;
    [Min(0)] [Tooltip("The amount of time to wait before it begins to move")]
    [SerializeField] private float waitMoveTime = 5;

    [SerializeField] private float minHeight = 36.5f;
    [SerializeField] private float maxHeight = 52.5f;
    [SerializeField] private bool bDoScaleInstead = false;

    private Transform liftPos;
    private Vector3 startPos;
    private Tween moveTween;

    private IEnumerator moveWait;

    public void MoveUp()
    {
        if (moveWait == null)
        {
            moveWait = MoveWait(true);
            StartCoroutine(moveWait);
        }
    }

    public void MoveDown()
    {
        if (moveWait == null)
        {
            moveWait = MoveWait(false);
            StartCoroutine(moveWait);
        }
    }

    private void Start()
    {
        liftPos = transform.childCount > 0 ? transform.GetChild(0) : transform;

        if (bDoScaleInstead)
        {
            startPos = liftPos.transform.localScale;
        }
        else
        {
            startPos = liftPos.position;
        }
    }

    private void Update()
    {
        if (moveTween != null)
        {
            if (bDoScaleInstead)
            {
                liftPos.transform.localScale = moveTween.UpdatePosition();
            }
            else
            {
                liftPos.position = moveTween.UpdatePosition();
            }

            if (moveTween.IsComplete())
            {
                if (bDoScaleInstead)
                {
                    liftPos.transform.localScale = moveTween.EndPos;
                }
                else
                {
                    liftPos.position = moveTween.EndPos;
                }

                moveTween = null;
            }
        }
    }

    private IEnumerator MoveWait(bool bIsUpMove)
    {
        Debug.Log("Moving the item upwards");
        yield return new WaitForSeconds(waitMoveTime);

        if (!bIsUpMove)
        {
                moveTween = new Tween(liftPos.position, new Vector3(startPos.x, minHeight, startPos.z), Time.time, moveSpeed);
        }
        else
        {
            moveTween = new Tween(liftPos.position, new Vector3(startPos.x, maxHeight, startPos.z), Time.time, moveSpeed);
        }

        moveWait = null;
    }

    public void LoadData(SectionData data)
    {
        moveTween = null;
        StopAllCoroutines();
        moveWait = null;
    }

    public void SaveData(SectionData data)
    {
        // do nothing
    }
}