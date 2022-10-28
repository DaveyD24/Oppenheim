using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransitionWait : MonoBehaviour
{
    [SerializeField] private Material transitonMaterial;
    [SerializeField] private GameObject cameraObj; // a camera to temporarily enable while changing scene so that only one will always be existing in the scene
    [SerializeField] private GameObject loadingImg;
    private Tween tween;

    private bool bEnd = false;

    private string prevScene;
    private string nextScene;

    public void StartTransition(string prevScene, string nextScene)
    {
        this.prevScene = prevScene;
        this.nextScene = nextScene;

        bEnd = false;
        tween = new Tween(1, 0, Time.time, 0.5f);
    }

    private void Update()
    {
        if (tween != null)
        {
            transitonMaterial.SetFloat("_Cutoff", tween.UpdateValue());
            if (tween.IsComplete())
            {
                if (bEnd)
                {
                    SceneManager.UnloadSceneAsync("TransitionScene");
                }
                else
                {
                    tween = null;
                    StartCoroutine(WaitNewFinished());
                }
            }
        }
    }

    // Start is called before the first frame update

    // Update is called once per frame
    private IEnumerator CloseScene()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.UnloadSceneAsync("TransitionScene");
    }

    private IEnumerator WaitNewFinished()
    {
        cameraObj.SetActive(true);
        loadingImg.SetActive(true);

        // unload the old scene
        var asyncWait = SceneManager.UnloadSceneAsync(prevScene);
        while (!asyncWait.isDone)
        {
            yield return null;
        }

        // load in the new scene
        asyncWait = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);
        while (!asyncWait.isDone)
        {
            yield return null;
        }

        cameraObj.SetActive(false);
        loadingImg.SetActive(false);

        // as the old scene has been successfully unloaded, and the new one loaded in play the fade in animation
        tween = new Tween(0, 1, Time.time, 0.5f);
        Debug.Log("End");
        bEnd = true;
    }
}
