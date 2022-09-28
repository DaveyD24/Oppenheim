using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

public class UniqueID : MonoBehaviour
{
    [field: Header("Save System Data(Only first two items)")]
    [field: SerializeField] public bool BMarkUnchangeable { get; set; } = false; // ensures no id once set can ever be rest, and mess up the save system as a result

    [field: SerializeField] [field: ReadOnly] public int SaveID { get; set; }

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
                if (item.gameObject != this.gameObject && item.SaveID == SaveID)
                {
                    if ((item.BMarkUnchangeable && BMarkUnchangeable) || BMarkUnchangeable)
                    {
                        Debug.LogError("Two marked unique id's are not unique. If duplicated, consider turning off BMarkUnchangeable so that these items can get a unique id");
                        return;
                    }

                    SaveID++; // continually increase the value until one is found which is unique
                    bNewUnique = false;
                    Debug.Log("Scripted Updated ID");
                }
            }

            if (bNewUnique)
            {
                bIsUnique = true;
                BMarkUnchangeable = true;
                EditorUtility.SetDirty(this);
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // check if this is the prefab assets version or if the object is in the scene
        PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        bool isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
        bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;

        // only update when this object is detected as being in the scene, and not in its prefab mode(the asset itself being stored as the prefab file)
        if (!isValidPrefabStage && prefabConnected)
        {
            UpdateId();
        }
        else if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            // this is the prefab asset gameobject so ensure the values are at their default state
            BMarkUnchangeable = false;
            SaveID = 0;
        }
    }
#endif
}
