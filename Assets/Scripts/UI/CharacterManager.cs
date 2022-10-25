using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{

    public CharacterDatabase characterDB;

    public Text nameText;
    public SpriteRenderer characterSprite;

    private int selectedOption = 0;

    // Start is called before the first frame update
    void Start()
    {
        updateCharacter(selectedOption);
    }

    public void nextCharacter()
    {
        selectedOption++;

        if(selectedOption >= characterDB.characterCount)
        {
            selectedOption = 0;
        }

        updateCharacter(selectedOption);
    }

    public void prevCharacter()
    {
        selectedOption--;

        if(selectedOption < 0)
        {
            selectedOption = characterDB.characterCount - 1;
        }

        updateCharacter(selectedOption);
    }

    private void updateCharacter(int selectedOption)
    {
        Character character = characterDB.getCharacter(selectedOption);
        characterSprite.sprite = character.characterSprite;
        nameText.text = character.characterName;
    }

    public void StartGame()
    {
        PlayerPrefs.SetInt("SelectedCharacter", selectedOption);
        //SceneManager.LoadScene(n);
    }
    
}
