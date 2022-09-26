using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forklift : MonoBehaviour
{
    [SerializeField] private float moveSpeed;

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
        liftPos = transform.GetChild(0);
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
        yield return new WaitForSeconds(5);

        if (!bIsUpMove)
        {
                moveTween = new Tween(new Vector3(startPos.x, maxHeight, startPos.z), new Vector3(startPos.x, minHeight, startPos.z), Time.time, moveSpeed);
        }
        else
        {
                moveTween = new Tween(new Vector3(startPos.x, minHeight, startPos.z), new Vector3(startPos.x, maxHeight, startPos.z), Time.time, moveSpeed);
        }

        moveWait = null;
    }
}