using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GatherStageObjects : MonoBehaviour
{
    [SerializeField] private List<IDataInterface> stageSavables;

    [field: ContextMenuItem("Gather Saveable Objects", "FindSaveableObjectsArea")]
    [field: SerializeField] public int StageID { get; private set; }

    [field: SerializeField] public int SectionID { get; private set; }

    [SerializeField] private Vector3 offsetPos;
    [SerializeField] private int boxWidth;
    [SerializeField] private int boxHeight;
    [SerializeField] private int boxDepth;

    private void FindSaveableObjectsArea()
    {
        stageSavables = new List<IDataInterface>();

        Collider[] col = Physics.OverlapBox(CalculateOffset(), BoxSize() / 2);
        foreach (Collider item in col)
        {
            // ignore any collider which is a player 
            if (item.gameObject.transform.root.gameObject.GetComponent<PlayerController>() == null)
            {
                // get all scripts on the object which can have data from them saved
                IEnumerable<IDataInterface> saveableObjects = item.gameObject.GetComponents<MonoBehaviour>().OfType<IDataInterface>();
                if (saveableObjects != null)
                {
                    stageSavables.AddRange(saveableObjects);
                }
            }
        }

        // stageSavables = FindObjectsOfType<MonoBehaviour>().OfType<IDataInterface>();//FindObjectsOfType<MonoBehaviour>().OfType<IDataInterface>();
        Debug.Log(stageSavables.ToList().Count);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawCube(CalculateOffset(), BoxSize());
    }

    private Vector3 BoxSize()
    {
        return new Vector3(boxWidth, boxHeight, boxDepth);
    }

    private Vector3 CalculateOffset()
    {
        return transform.position + offsetPos;
    }

    private void SaveSection()
    {
        PersistentDataManager.SaveableData = new SaveableData(); // not the correct way to do it, use the event system once setup to handle this specifics
        PersistentDataManager.SaveableData.ResetData();
        SectionData sectionData = PersistentDataManager.SaveableData.ResetSectionData(StageID, SectionID);

        foreach (var item in stageSavables)
        {
            item.SaveData(sectionData);
        }
    }

    private void Start()
    {
        FindSaveableObjectsArea();
        SaveSection();
    }
}
