using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableBox : UniqueID, IDataInterface
{
    private Rigidbody rb;
    [SerializeField] private float power;
    private float sphereRadius = 19;
    [SerializeField] private bool bNotSave = false;

    private float waitTime = 0.1f;
    private bool bCanAddForce = true;

    public void ApplyMovementForce(Vector3 direction, bool bApplyForceToNeighbours = true)
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

            rb.AddExplosionForce(power, transform.position - (direction * 3), sphereRadius, 2, ForceMode.VelocityChange);

            // for each neighbour apply a force in the direction of movement as well
            if (bApplyForceToNeighbours)
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position, sphereRadius);
                foreach (Collider box in colliders)
                {
                    if (box.gameObject.TryGetComponent(out PushableBox pushableBox))
                    {
                        pushableBox.ApplyMovementForce(direction, false);
                    }
                }
            }

            bCanAddForce = false;
            StartCoroutine(ForceWait());
        }
    }

    private void OnDrawGizmosSelected()
    {
        //Gizmos.DrawSphere(transform.position, sphereRadius);
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
        if (!bNotSave && gameObject.GetComponent<BoxCollider>() != null)
        {
            if (data.GeneralPhysicsObject.Dictionary.TryGetValue(SaveID, out GeneralPhysicsObjectData boxData))
            {
                rb.angularVelocity = boxData.AngularVelocity;
                transform.position = boxData.Position;
                transform.rotation = boxData.Rotation;
                rb.velocity = boxData.Velocity;
            }
        }
    }

    public void SaveData(SectionData data)
    {
        // only save this data if it is not currently attached to a balloon
        if (!bNotSave && this.gameObject.GetComponent<Collider>() != null)
        {
            GeneralPhysicsObjectData boxData = new GeneralPhysicsObjectData();
            boxData.AngularVelocity = rb.angularVelocity;
            boxData.Position = transform.position;
            boxData.Rotation = transform.rotation;
            boxData.Velocity = rb.velocity;

            data.GeneralPhysicsObject.Dictionary.Add(SaveID, boxData);
        }
    }
}
