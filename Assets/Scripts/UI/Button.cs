using System.Collections;
using System.Collections.Generic;
using EventSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Button : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;

    public void ChangeScene(string scene)
    {
        Time.timeScale = 1;
        string currScene = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene(scene);

        // SceneManager.LoadSceneAsync("TransitionScene", LoadSceneMode.Additive);
        // StartCoroutine(WaitTransition(scene, currScene));
        //SceneManager.LoadScene(scene);
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
    }

    private IEnumerator WaitTransition(string scene, string currScene)
    {
        yield return new WaitForSeconds(0.5f);

        var asyncWait = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

        while (!asyncWait.isDone)
        {
            yield return null;
        }

        GameObject.Find("CanvasTransition").GetComponent<TransitionWait>().EndTransition();

        SceneManager.UnloadSceneAsync(currScene);
    }
}
