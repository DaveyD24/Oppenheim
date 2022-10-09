using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// get all savable objects for the defined bounds of this stage, so that the can be saved and loaded dynamically.
/// </summary>
public class GatherStageObjects : MonoBehaviour
{
    [SerializeField] private List<IDataInterface> stageSavables;
    private List<FuelCollectible> sectionFuelCollectible;
    [SerializeField] private Vector3 offsetPos;
    [SerializeField] private int boxWidth;
    [SerializeField] private int boxHeight;
    [SerializeField] private int boxDepth;

    [field: ContextMenuItem("Gather Saveable Objects", "FindSaveableObjectsArea")]
    [field: SerializeField] public int StageID { get; private set; }

    [field: SerializeField] public int SectionID { get; private set; }

    public void SaveSection()
    {
        // PersistentDataManager.SaveableData = new SaveableData(); // not the correct way to do it, use the event system once setup to handle this specifics
        // PersistentDataManager.SaveableData.ResetData();
        SectionData sectionData = PersistentDataManager.SaveableData.ResetSectionData(StageID, SectionID);

        foreach (var item in stageSavables)
        {
            item.SaveData(sectionData);
        }

        // Debug.Log("Saved Sections Data: " + SectionID);
    }

    public void LoadSection()
    {
        // PersistentDataManager.SaveableData = new SaveableData(); // not the correct way to do it, use the event system once setup to handle this specifics
        // PersistentDataManager.SaveableData.ResetData();
        SectionData sectionData = PersistentDataManager.SaveableData.ReturnSectionsData(StageID, SectionID);

        if (sectionData != null)
        {
            foreach (var item in stageSavables)
            {
                item.LoadData(sectionData);
            }
        }

        Debug.Log("Loaded in Sections Data: " + SectionID);
    }

    /// <summary>
    /// get the number of each type of collectible gathered in this section which is not saved when a player dies.
    /// </summary>
    /// <returns>an array holding the number of each type of collectible gathered.</returns>
    public int[] NumberInvalidSaveCollectibles()
    {
        int[] collectiblesGathered = new int[4];
        foreach (FuelCollectible item in sectionFuelCollectible)
        {
            // if the item is not active in the scene it means it has been gathered
            if (!item.gameObject.activeSelf)
            {
                collectiblesGathered[item.PlayerId.PlayerID] += 1;
            }
        }

        return collectiblesGathered;
    }

    private void FindSaveableObjectsArea()
    {
        stageSavables = new List<IDataInterface>();
        sectionFuelCollectible = new List<FuelCollectible>();

        Collider[] col = Physics.OverlapBox(CalculateOffset(), BoxSize() / 2);
        foreach (Collider item in col)
        {
            // ignore any collider which is a player.
            if (item.gameObject.transform.root.gameObject.GetComponent<PlayerController>() == null)
            {
                // get all scripts on the object which can have data from them saved
                IEnumerable<IDataInterface> saveableObjects = item.gameObject.GetComponents<MonoBehaviour>().OfType<IDataInterface>();
                if (saveableObjects != null)
                {
                    stageSavables.AddRange(saveableObjects);
                }
            }

            if (item.gameObject.TryGetComponent(out FuelCollectible fuel))
            {
                sectionFuelCollectible.Add(fuel);

                // Debug.Log("fuel Being added");
            }
        }

        // stageSavables = FindObjectsOfType<MonoBehaviour>().OfType<IDataInterface>();//FindObjectsOfType<MonoBehaviour>().OfType<IDataInterface>();
        // Debug.Log(stageSavables.ToList().Count);
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(CalculateOffset(), BoxSize());
    }

    private Vector3 BoxSize()
    {
        return new Vector3(boxWidth, boxHeight, boxDepth);
    }

    private Vector3 CalculateOffset()
    {
        return transform.position + offsetPos;
    }

    private void Awake()
    {
        FindSaveableObjectsArea();

        // SaveSection();
    }

    private void Start()
    {
        SaveSection();
    }
}
