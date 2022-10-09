using System.Collections;
using System.Collections.Generic;
using EventSystem;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TextInfoManager : MonoBehaviour
{
    private InputActions Inputs;

    private bool isComplete = false;
    [SerializeField] private GameObject generalControls;
    [SerializeField] private GameObject introControls;

    [Header("Annoucements")]
    [SerializeField] private List<string> annoucementTxt = new List<string>();
    [SerializeField] private float charWaitTime = 0.1f;
    private int charAt;
    private int lineAt;

    [SerializeField] private TextMeshProUGUI announceTxt;

    [Header("Oppenheim")]
    [SerializeField] private List<string> introWorryTxt = new List<string>();
    [SerializeField] private TextMeshProUGUI oppenheimTxt;
    [SerializeField] private GameObject oppenheimObj;
    [SerializeField] private GameObject speechBubbleObj;

    private TextMeshProUGUI currentTxt;
    private List<string> currentList = new List<string>();

    public void InitilizeAnnoucement()
    {
        lineAt = 0;
        charAt = 0;
        currentTxt = announceTxt;
        currentList = annoucementTxt;
        StartCoroutine(PrintText());
    }

    private IEnumerator PrintText()
    {
        currentTxt.text += currentList[lineAt][charAt];
        yield return new WaitForSeconds(charWaitTime);
        charAt++;
        if (charAt > currentList[lineAt].Length - 1)
        {
            NextLine();
        }
        else
        {
            StartCoroutine(PrintText());
        }
    }

    private void NextLine()
    {
        if (!isComplete)
        {
            StopAllCoroutines();
            lineAt++;
            currentTxt.text = string.Empty;
            charAt = 0;
            // Debug.Log(currentList.Count + "waesrdfgthdfrdwaesfrdgd" + lineAt);
            if (lineAt < currentList.Count)
            {
                charAt = 0;
                StartCoroutine(PrintText());
            }
            else if (!oppenheimObj.activeSelf)
            {
                oppenheimObj.SetActive(true);
                speechBubbleObj.SetActive(true);
                currentTxt = oppenheimTxt;
                currentList = introWorryTxt;
                lineAt = 0;
                charAt = 0;
                StartCoroutine(PrintText());
            }
            else
            {
                oppenheimObj.SetActive(false);
                speechBubbleObj.SetActive(false);
                UIEvents.ShowInstructions();
                isComplete = true;
                introControls.SetActive(false);
                generalControls.SetActive(true);
            }
        }
    }

    private void KeyNextLine(InputAction.CallbackContext ctx)
    {
        NextLine();
    }

    private void SkipIntro(InputAction.CallbackContext ctx)
    {
        if (!isComplete)
        {
            StopAllCoroutines();
            announceTxt.gameObject.SetActive(false);
            oppenheimObj.SetActive(false);
            speechBubbleObj.SetActive(false);
            UIEvents.ShowInstructions();
            isComplete = true;
            introControls.SetActive(false);
            generalControls.SetActive(true);
        }
    }

    private void OnEnable()
    {
        UIEvents.OnBeginAnnoucement += InitilizeAnnoucement;

        Inputs = new InputActions();

        Inputs.Player.NextLine.performed += KeyNextLine;
        Inputs.Player.SkipTut.performed += SkipIntro;

        Inputs.Player.Enable();
    }

    private void OnDisable()
    {
        UIEvents.OnBeginAnnoucement -= InitilizeAnnoucement;

        if (Inputs != null)
        {
            Inputs.Player.NextLine.performed -= KeyNextLine;
            Inputs.Player.SkipTut.performed -= SkipIntro;

            Inputs.Player.Disable();
        }
    }
}
