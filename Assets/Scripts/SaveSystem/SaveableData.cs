using EventSystem;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// within this class, hold values containing all the data(or copies of it) of the game which are required to be saved.
/// </summary>
[System.Serializable]
public class SaveableData
{
    [SerializeField] public SerializableDictionary<int, PlayerData> PlayerDatas { get; set; }

    [SerializeField] public SerializableDictionary<int, StageData> StagesData { get; set; }

    // only need to reset the list values, everything else is also reset when they change
    public void ResetData()
    {
        PlayerDatas = new SerializableDictionary<int, PlayerData>();
        StagesData = new SerializableDictionary<int, StageData>();
    }

    /// <summary>
    /// return the section data for the object who requested it.
    /// if something does not exist yet add it to the list firstly, then return it.
    /// </summary>
    /// <param name="currStage">the current stage on.</param>
    /// <param name="currSection">the current section of the current stage.</param>
    /// <returns>The data for the current section of the current stage.</returns>
    public SectionData ReturnSectionsData(int currStage, int currSection)
    {
        if (StagesData.Dictionary.ContainsKey(currStage))
        {
            if (StagesData.Dictionary[currStage].EachSectionData.Dictionary.ContainsKey(currSection))
            {
                return StagesData.Dictionary[currStage].EachSectionData.Dictionary[currSection];
            }
        }

        Debug.LogError("No save data exists for the current section");
        return null;
    }

    /// <summary>
    /// As making a new save for this section, override all existing data by deleting it.
    /// </summary>
    /// <param name="currStage">the current stage at.</param>
    /// <param name="currSection">the section to reset the data for.</param>
    /// <returns>the save data created for the section once it reset.</returns>
    public SectionData ResetSectionData(int currStage, int currSection)
    {
        if (StagesData.Dictionary.ContainsKey(currStage))
        {
            if (StagesData.Dictionary[currStage].EachSectionData.Dictionary.ContainsKey(currSection))
            {
                StagesData.Dictionary[currStage].EachSectionData.Dictionary[currSection] = new SectionData();
            }
            else
            {
                StagesData.Dictionary[currStage].EachSectionData.Dictionary.Add(currSection, new SectionData());
            }
        }
        else
        {
            // as the stage does not exist yet add it and the new section data as well
            StagesData.Dictionary.Add(currStage, new StageData());

            StagesData.Dictionary[currStage].EachSectionData.Dictionary.Add(currSection, new SectionData());
        }

        return StagesData.Dictionary[currStage].EachSectionData.Dictionary[currSection];
    }
}
