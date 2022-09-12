using System.Collections;
using System.Collections.Generic;
using EventSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextInfoManager : MonoBehaviour
{
    private bool isComplete = false;
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

    private void Start()
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
            Debug.Log(currentList.Count + "waesrdfgthdfrdwaesfrdgd" + lineAt);
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
            }
        }
    }

    private void SkipIntro()
    {
        if (!isComplete)
        {
            StopAllCoroutines();
            announceTxt.gameObject.SetActive(false);
            oppenheimObj.SetActive(false);
            speechBubbleObj.SetActive(false);
            UIEvents.ShowInstructions();
            isComplete = true;
        }
    }

    private void OnEnable()
    {
        UIEvents.OnNextLine += NextLine;
        UIEvents.OnSkipIntro += SkipIntro;
    }

    private void OnDisable()
    {
        UIEvents.OnNextLine -= NextLine;
        UIEvents.OnSkipIntro -= SkipIntro;
    }
}
