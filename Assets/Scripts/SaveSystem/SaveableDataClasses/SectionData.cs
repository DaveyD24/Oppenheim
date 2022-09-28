using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hold reference to the general data relevent to each stage.
/// </summary>
[System.Serializable]
public class SectionData
{
    [SerializeField] public List<int> AbilityItems { get; set; } = new List<int>(); // the id's of the ability items still left in the current section of the map.

    [SerializeField] public SerializableDictionary<int, PushableBoxData> PushableBoxs { get; set; } = new SerializableDictionary<int, PushableBoxData>();

}

/// <summary>
/// Hold the data relevent to each stage.
/// </summary>
[System.Serializable]
#pragma warning disable SA1402 // File may only contain a single type
public class StageData
#pragma warning restore SA1402 // File may only contain a single type
{
    [SerializeField] public SerializableDictionary<int, SectionData> EachSectionData { get; set; } = new SerializableDictionary<int, SectionData>();

}
