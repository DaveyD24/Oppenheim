using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forklift : MonoBehaviour
{
    [SerializeField] private float moveSpeed;

    private const float minHeight = 36.5f;
    private const float maxHeight = 52.5f;

    Transform liftPos;
    Vector3 pos;
    Tween moveTween;

    IEnumerator moveWait;

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
        pos = liftPos.position;
    }

    private void Update()
    {
        if (moveTween != null)
        {
            liftPos.position = moveTween.UpdatePosition();

            if (moveTween.IsComplete())
            {
                liftPos.position = moveTween.EndPos;
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
            moveTween = new Tween(new Vector3(pos.x, maxHeight, pos.z), new Vector3(pos.x, minHeight, pos.z), Time.time, moveSpeed);
        }
        else
        {
            moveTween = new Tween(new Vector3(pos.x, minHeight, pos.z), new Vector3(pos.x, maxHeight, pos.z), Time.time, moveSpeed);
        }

        moveWait = null;
    }
}