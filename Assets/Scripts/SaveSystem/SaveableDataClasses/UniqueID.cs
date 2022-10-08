using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

/// <summary>
/// A class to generate a unique id for each object which needs data from it saved, so that puzzles can be reset.
/// </summary>
public class UniqueID : MonoBehaviour
{
    [field: Header("Save System Data(Only first three items)")]
    [field: SerializeField] public bool BIdUnchangeable { get; set; } = false; // ensures no id once set can ever be rest, and mess up the save system as a result

    [field: SerializeField] public bool BNotSaveData { get; set; } = false; // should this items data not be saved

    [field: SerializeField] [field: ReadOnly] public int SaveID { get; set; } // a unique id to use when saving and loading the object.

    /// <summary>
    /// Give each saveable item a unique id.
    /// Note that this is kinda inefficent at times, simply due to it needing to search all objects.
    /// </summary>
    public void UpdateId()
    {
        UniqueID[] itemIDs = FindObjectsOfType<UniqueID>();

        bool bIsUnique = false;
        while (!bIsUnique)
        {
            bool bNewUnique = true;
            foreach (UniqueID item in itemIDs)
            {
                if (item.transform.gameObject != this.transform.gameObject && item.SaveID == SaveID)
                {
                    if ((item.BIdUnchangeable && BIdUnchangeable) || BIdUnchangeable)
                    {
                        Debug.LogError("Two marked unique id's are not unique. If duplicated, consider turning off BIDUnchangeable so that these items can get a unique id " +
                            item.SaveID + ": Other Id  " + SaveID + ": This iD" + ", On Object: " + this.transform.gameObject.name);
                        return;
                    }

                    SaveID++; // continually increase the value until one is found which is unique.
                    bNewUnique = false;
                }
            }

            if (bNewUnique)
            {
                bIsUnique = true;
                BIdUnchangeable = true;
                EditorUtility.SetDirty(this);
                // Debug.Log("Scripted Updated ID");
            }
        }
    }

#if UNITY_EDITOR
    public virtual void OnValidate()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            // check if this is the prefab assets version or if the object is in the scene
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            bool isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
            bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;

            // only update when this object is detected as being in the scene, and not in its prefab mode(the asset itself being stored as the prefab file)
            // also do it if it is detected this object is not in any way connected to any prefab asset at all
            if ((!isValidPrefabStage && prefabConnected) || !PrefabUtility.IsPartOfAnyPrefab(this.transform.gameObject))
            {
                UpdateId();
            }
            else
            {
                // this is the prefab asset gameobject so ensure the values are at their default state
                BIdUnchangeable = false;
                SaveID = 0;
            }
        }
    }
#endif
}
