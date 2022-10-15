using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// save all the nessesary data for a physics based object with no other properties on it that require saving.
/// </summary>
public class GeneralPhysicsSavable : UniqueID, IDataInterface
{
    private Rigidbody rb;

    public void LoadData(SectionData data)
    {
        if (data.GeneralPhysicsObject.Dictionary.TryGetValue(SaveID, out GeneralPhysicsObjectData boxData))
        {
            rb.angularVelocity = boxData.AngularVelocity;
            transform.position = boxData.Position;
            transform.rotation = boxData.Rotation;
            rb.velocity = boxData.Velocity;
        }
    }

    public void SaveData(SectionData data)
    {
        GeneralPhysicsObjectData boxData = new GeneralPhysicsObjectData();
        boxData.AngularVelocity = rb.angularVelocity;
        boxData.Position = transform.position;
        boxData.Rotation = transform.rotation;
        boxData.Velocity = rb.velocity;

        data.GeneralPhysicsObject.Dictionary.Add(SaveID, boxData);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
}
