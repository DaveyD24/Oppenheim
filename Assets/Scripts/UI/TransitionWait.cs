using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionWait : MonoBehaviour
{
    // Start is called before the first frame update
    public void EndTransition()
    {
        transform.GetChild(0).gameObject.GetComponent<Animator>().SetTrigger("End");
        StartCoroutine(CloseScene());
        Debug.Log("End");
    }

    // Update is called once per frame
    private IEnumerator CloseScene()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.UnloadSceneAsync("TransitionScene");
    }
}
