using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EventSystem;
using UnityEngine;

// does the handling of the saving and loading of the games data
// only ever have one of these in the scene at one time. only this is required to be added for the entire saving to function correctly
public class PersistentDataManager : MonoBehaviour
{
    [Tooltip("The name to call the specific folder saving to")][ContextMenuItem("Delete Saved Data for Specified Directory", "DeleteSaveFile")]
    [SerializeField]private string directoryName = "SaveFiles"; // the name to call the folder saving to
    [Tooltip("The name to call the specific save file")][ContextMenuItem("Delete Saved Data for Specified Directory", "DeleteSaveFile")]
    [SerializeField]private string dataFileName = "GameSave"; // the name to call the save file
    [Tooltip("The file ending to append for the backup copy file")][ContextMenuItem("Delete Saved Data for Specified Directory", "DeleteSaveFile")]
    [SerializeField]private string backupFileEnding = ".bak"; // .bak is the common method used for identifying a save file

    private string dataDirectory; // for the unity project the persistant data path

    public static SaveableData SaveableData { get; set; } // the data which is getting saved

    public void DeleteSaveFile()
    {
        if (dataDirectory == null)
        {
            dataDirectory = Application.persistentDataPath;
        }

        string directoryPath = Path.Combine(dataDirectory, directoryName); // this accounts for different paths having different path seperators
        try
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(Path.GetDirectoryName(directoryPath), true); // recursivly delete entire directory
                Debug.Log(directoryName + " has be succesfully deleted");
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("failed to load, specified path could not be found: " + directoryPath + "/" + e);
        }

        string fullPath = Path.Combine(dataDirectory, directoryName, dataFileName); // this accounts for different paths having different path seperators
        try
        {
            if (File.Exists(fullPath))
            {
                Directory.Delete(Path.GetDirectoryName(fullPath), true); // recursivly delete entire directory
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("failed to load, specified path could not be found: " + fullPath + "/" + e);
        }
    }

    public SaveableData LoadFromFile(bool bAllowRestoreBackup)
    {
        string fullPath = Path.Combine(dataDirectory, directoryName, dataFileName); // this accounts for different operating systems having different path seperators
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad;
                using (FileStream stream = new FileStream(fullPath, FileMode.Open)) // get the specified file and open it
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd(); // load the data into the specified string
                    }
                }

                SaveableData loadedSavedData = JsonUtility.FromJson<SaveableData>(dataToLoad); // deserilize and load the data back in to the file
                return loadedSavedData;
            }
            catch (System.Exception e)
            {
                // something, such as the save file being corrupt occured, so load in the backup
                Debug.Log("failed to load, specified path could not be found: " + fullPath + "/" + e);
                if (bAllowRestoreBackup && AttemptLoadBackup(fullPath))
                {
                    return LoadFromFile(false);
                }
            }
        }

        return null; // no save file found
    }

    /// <summary>
    /// loads in all saved data for a level when it first opens.
    /// </summary>
    public void LoadLevelData()
    {
        //Debug.Log("Loading the levels data.....................................");

        //IEnumerable<IDataInterface> dataSavedObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataInterface>();
        //foreach (IDataInterface dataObj in dataSavedObjects)
        //{
        //    dataObj.LoadData(SaveableData);
        //}

        // use the event system to call the load data function on each section
    }

    /// <summary>
    /// saves all saveable data to a file.
    /// </summary>
    public void SaveToFile()
    {
        Debug.Log("saveing the game to a file.....................................");
        string fullPath = Path.Combine(dataDirectory, directoryName, dataFileName); // this accounts for different operating systems having different path seperators
        string backupPath = fullPath + backupFileEnding;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)); // if the specified directory does not exist create it

            // serilize the data from a C# script into a JSON file
            string dataToSave = JsonUtility.ToJson(SaveableData, true);
            using (FileStream stream = new FileStream(fullPath, FileMode.Create)) // make a new file in the specified location
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToSave); // save the specified data to the file
                }
            }

            // check if the data is corrupt or not
            SaveableData saveableDataChecker = LoadFromFile(false);
            if (saveableDataChecker != null)
            {
                File.Copy(fullPath, backupPath, true); // save a backup version of the file
            }
            else
            {
                Debug.Log("Failed to make the backup as the save file: " + fullPath + ", is corrupted");
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Error saving file to specified path: " + fullPath + "/" + e);
        }
    }

    private void Awake()
    {
        dataDirectory = Application.persistentDataPath;
        dataDirectory = Path.Combine(dataDirectory, directoryName);

        // the full path(on windows) C:\Users\[usersname]\AppData\LocalLow\[companyname]\[projectname]
    }

    private void OnDisable()
    {
        if (SaveableData != null)
        {

        }
    }

    private void OnEnable()
    {
        SaveableData = LoadFromFile(true);

        if (SaveableData == null)
        {
            NewGame();
        }
    }

    private void NewGame()
    {
        SaveableData = new SaveableData();
        SaveableData.ResetData();
    }

    private bool AttemptLoadBackup(string fullPath)
    {
        string backupPath = fullPath + backupFileEnding;
        try
        {
            if (File.Exists(backupPath))
            {
                File.Copy(backupPath, fullPath, true); // copy the backup to the original full path file
                Debug.LogWarning("Backup of " + fullPath + " loaded in");
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (System.Exception)
        {
            Debug.LogError("backup " + backupPath + " is also corrupt, failed to load in");
            return false;
        }
    }

    private SaveableData ReturnSavedData()
    {
        return SaveableData;
    }
}