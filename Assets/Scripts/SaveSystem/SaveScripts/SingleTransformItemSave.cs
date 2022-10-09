using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used on any object which has just one single transform component to save.
/// </summary>
public class SingleTransformItemSave : UniqueID, IDataInterface
{
    [SerializeField] private TransformComponent transformComp;

    // pick the single transform component which is to be saved
    private enum TransformComponent
    {
        Position,
        Rotation,
        Scale,
    }

    public void LoadData(SectionData data)
    {
        if (transformComp == TransformComponent.Position)
        {
            transform.position = data.OneTransformChangeObjects.Dictionary[SaveID];
        }
        else if (transformComp == TransformComponent.Rotation)
        {
            transform.rotation = Quaternion.Euler(data.OneTransformChangeObjects.Dictionary[SaveID]);
        }
        else if (transformComp == TransformComponent.Scale)
        {
            transform.localScale = data.OneTransformChangeObjects.Dictionary[SaveID];
        }
    }

    public void SaveData(SectionData data)
    {
        if (transformComp == TransformComponent.Position)
        {
            data.OneTransformChangeObjects.Dictionary.Add(SaveID, transform.position);
        }
        else if (transformComp == TransformComponent.Rotation)
        {
            data.OneTransformChangeObjects.Dictionary.Add(SaveID, transform.rotation.eulerAngles);
        }
        else if (transformComp == TransformComponent.Scale)
        {
            data.OneTransformChangeObjects.Dictionary.Add(SaveID, transform.localScale);
        }
    }
}
