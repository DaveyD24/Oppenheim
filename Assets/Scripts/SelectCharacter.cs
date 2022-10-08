using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectCharacter : MonoBehaviour
{
    public GameObject[] characters;
    public int selectedCharacter = 0;

    public void NextCharacter() 
    {
        characters[selectedCharacter].SetActive(false);
        if (selectedCharacter + 1 > characters.Length){
            selectedCharacter = 0;
        }
        else{
            selectedCharacter = (selectedCharacter + 1); //% characters.Length;
        } 
        characters[selectedCharacter].SetActive(true);
    }

    public void PreviousCharacter()
    {
        characters[selectedCharacter].SetActive(false);
        selectedCharacter--;
        if (selectedCharacter < 0)
            selectedCharacter += characters.Length;
        characters[selectedCharacter].SetActive(true);
    }

    public void StartGame()
    {
        PlayerPrefs.SetInt("SelectedCharacter", selectedCharacter);
        //SceneManager.LoadScene(n);
    }
}
