using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hold reference to the general data relevent to each section of each stage.
/// </summary>
[System.Serializable]
public class SectionData
{
    [SerializeField] public bool BIsCheckpointComplete { get; set; }

    [SerializeField] public List<int> AbilityItems { get; set; } = new List<int>(); // the id's of the ability items still left in the current section of the map.

    [SerializeField] public List<int> Balloons { get; set; } = new List<int>(); // id's for the items which get destroyed

    [SerializeField] public List<int> BreakableBoxes { get; set; } = new List<int>(); // id's for the items which get destroyed

    [SerializeField] public SerializableDictionary<int, GeneralPhysicsObjectData> GeneralPhysicsObject { get; set; } = new SerializableDictionary<int, GeneralPhysicsObjectData>();

    [SerializeField] public SerializableDictionary<int, Vector3> OneTransformChangeObjects { get; set; } = new SerializableDictionary<int, Vector3>();

    [SerializeField] public SerializableDictionary<int, Vector3> PositionChangeObjects { get; set; } = new SerializableDictionary<int, Vector3>();

    [SerializeField] public SerializableDictionary<int, bool> SwitchData { get; set; } = new SerializableDictionary<int, bool>();

}

/// <summary>
/// Hold the information about all sections of the current stage.
/// </summary>
[System.Serializable]
#pragma warning disable SA1402 // File may only contain a single type
public class StageData
#pragma warning restore SA1402 // File may only contain a single type
{
    [SerializeField] public SerializableDictionary<int, SectionData> EachSectionData { get; set; } = new SerializableDictionary<int, SectionData>();

}
