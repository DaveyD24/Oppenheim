using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableBox : UniqueID, IDataInterface
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

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private IEnumerator ForceWait()
    {
        yield return new WaitForSeconds(waitTime);
        bCanAddForce = true;
    }

#pragma warning disable SA1202 // Elements should be ordered by access
    public void LoadData(SectionData data)
#pragma warning restore SA1202 // Elements should be ordered by access
    {
        throw new System.NotImplementedException();
    }

    public void SaveData(SectionData data)
    {
        PushableBoxData boxData = new PushableBoxData();
        boxData.AngularVelocity = rb.angularVelocity;
        boxData.Position = transform.position;
        boxData.Rotation = transform.rotation;
        boxData.Velocity = rb.velocity;

        data.PushableBoxs.Dictionary.Add(SaveID, boxData);
    }
}
