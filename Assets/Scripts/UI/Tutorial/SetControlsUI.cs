using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EventSystem;

public class SetControlsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI headerTxt;
    [SerializeField] private GameObject speechBubble;
    [Header("One Input")]
    [SerializeField] private GameObject oneObj;
    [SerializeField] private Image controlImg;

    [Header("Two Input")]
    [SerializeField] private GameObject twoObj;
    [SerializeField] private Image leftControlImg;
    [SerializeField] private Image rightControlImg;

    public void SetControlsActive(string controlName, int numPlayers, string input1Name, string input2Name)
    {
        headerTxt.gameObject.SetActive(true);
        headerTxt.text = controlName;

        if (speechBubble != null)
        {
            speechBubble.SetActive(true);
        }

        if (numPlayers == 1)
        {
            oneObj.SetActive(true);
            print(input1Name);
            controlImg.sprite = UIEvents.GetControlSprite(input1Name, controlName);
        }

        if (numPlayers == 2)
        {
            twoObj.SetActive(true);
            leftControlImg.sprite = UIEvents.GetControlSprite(input1Name, controlName);
            rightControlImg.sprite = UIEvents.GetControlSprite(input2Name, controlName);
        }
    }

    private void DeactivateControlVisuals()
    {
        oneObj.SetActive(false);
        twoObj.SetActive(false);
        headerTxt.gameObject.SetActive(false);

        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }
    }

    private void OnEnable()
    {
        UIEvents.OnShowInputControls += SetControlsActive;
        UIEvents.OnHideInputControls += DeactivateControlVisuals;
    }

    private void OnDisable()
    {
        UIEvents.OnShowInputControls -= SetControlsActive;
        UIEvents.OnHideInputControls -= DeactivateControlVisuals;
    }
}
