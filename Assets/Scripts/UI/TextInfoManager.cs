using System.Collections;
using System.Collections.Generic;
using EventSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextInfoManager : MonoBehaviour
{
    [Header("Annoucements")]
    [SerializeField] private List<string> annoucementTxt = new List<string>();
    [SerializeField] private float charWaitTime = 0.1f;

    [SerializeField] private TextMeshProUGUI announceTxt;

    [Header("Oppenheim")]
    [SerializeField] private List<string> introWorryTxt = new List<string>();
    [SerializeField] private TextMeshProUGUI oppenheimTxt;
    [SerializeField] private GameObject oppenheimObj;
    [SerializeField] private GameObject speechBubbleObj;

    private void Start()
    {
        StartCoroutine(PrintText(0, 0, announceTxt, annoucementTxt));
    }

    private IEnumerator PrintText(int charAt, int lineAt, TextMeshProUGUI textUI, List<string> info)
    {
        textUI.text += info[lineAt][charAt];
        yield return new WaitForSeconds(charWaitTime);
        charAt++;
        if (charAt > info[lineAt].Length - 1)
        {
            lineAt++;
            textUI.text = string.Empty;
            charAt = 0;
        }

        if (lineAt < info.Count)
        {
            StartCoroutine(PrintText(charAt, lineAt, textUI, info));
        }
        else if (!oppenheimObj.activeSelf)
        {
            oppenheimObj.SetActive(true);
            speechBubbleObj.SetActive(true);
            StartCoroutine(PrintText(0, 0, oppenheimTxt, introWorryTxt));
        }
        else
        {
            oppenheimObj.SetActive(false);
            speechBubbleObj.SetActive(false);
            UIEvents.ShowInstructions();
        }
    }
}
