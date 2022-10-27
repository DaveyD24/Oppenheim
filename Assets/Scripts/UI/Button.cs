using System.Collections;
using System.Collections.Generic;
using EventSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Button : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject firstPauseButton;

    private IEnumerator enumerator;

    public void ChangeScene(string scene)
    {
        Time.timeScale = 1;
        string currScene = SceneManager.GetActiveScene().name;
        if (enumerator == null)
        {
            enumerator = WaitTransition(currScene, scene);
            StartCoroutine(enumerator);
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void RestToCheckpoint()
    {
        GameEvents.Die();
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }

    private void OnEnable()
    {
        UIEvents.OnPauseGame += PauseGame;
        UIEvents.OnSceneChange += ChangeScene;
    }

    private void OnDisable()
    {
        UIEvents.OnPauseGame -= PauseGame;
        UIEvents.OnSceneChange -= ChangeScene;
    }

    private void PauseGame()
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        Time.timeScale = pauseMenu.activeSelf ? 0 : 1;

        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstPauseButton);
    }

    /// <summary>
    /// wait until the scene transition has loaded in before continuing.
    /// </summary>
    /// <param name="currScene">the current scene on.</param>
    /// <param name="nextScene">the next scene to load in.</param>
    /// <returns>null.</returns>
    private IEnumerator WaitTransition(string currScene, string nextScene)
    {
        var asyncWait = SceneManager.LoadSceneAsync("TransitionScene", LoadSceneMode.Additive);

        while (!asyncWait.isDone)
        {
            yield return null;
        }

        // SceneManager.LoadScene(scene);
        GameObject.Find("CanvasTransition").GetComponent<TransitionWait>().StartTransition(currScene, nextScene);
        enumerator = null;
        ////SceneManager.LoadScene(scene);
    }
}
