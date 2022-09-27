using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableBox : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float power;

    private float waitTime = 0.1f;
    private bool bCanAddForce = true;

    public void ApplyMovementForce(Vector3 direction)
    {
        if (bCanAddForce)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            {
                direction.z = 0;
                direction.x = direction.x > 0 ? 1 : -1;
            }
            else
            {
                direction.x = 0;
                direction.z = direction.z > 0 ? 1 : -1;
            }

            rb.AddForce(direction * power, ForceMode.VelocityChange);
            bCanAddForce = false;
            StartCoroutine(ForceWait());
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private IEnumerator ForceWait()
    {
        yield return new WaitForSeconds(waitTime);
        bCanAddForce = true;
    }
}
