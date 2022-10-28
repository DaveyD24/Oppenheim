using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// plays some sentences, plotting out each character individually
/// pause for a brief period at the end of each sentence as well.
/// </summary>
public class PlayDialogue : Node<ActionSequence>
{
    private List<string> dialogue; // each element denotes one sentence
    private TextMeshProUGUI text;
    private float textSpeed;
    private float sentencePauseTime; // the time to pause between each sentence

    private int charAt = 0;
    private int senetenceAt = 0;

    private AudioController audioController;

    public PlayDialogue(ActionSequence blackboard, List<string> dialogue, TextMeshProUGUI textObj, float textSpeed = 1, float sentencePauseTime = 1)
    {
        this.Blackboard = blackboard;
        this.textSpeed = textSpeed;
        this.sentencePauseTime = sentencePauseTime;
        text = textObj;
        this.dialogue = dialogue;
    }

    public override void Init()
    {
        base.Init();
        charAt = 0;
        senetenceAt = 0;
        text.text = " ";

        audioController = Blackboard.GetComponent<AudioController>();
        Blackboard.StartCoroutine(PrintText());
    }

    public override ENodeState Evaluate()
    {
        return senetenceAt <= dialogue.Count ? ENodeState.Running : ENodeState.Success; // can be whatever retrun is nessesary
    }

    public override void End()
    {
        base.End();
    }

    private IEnumerator PrintText()
    {
        AudioSource gibberish = null;
        if (audioController)
        {
            if (SceneManager.GetActiveScene().name == "IntroCutscene")
            {
                gibberish = audioController.Play("Gibberish Loop", EAudioPlayOptions.Global);
            }
            else
            {
                gibberish = audioController.Play("Gibberish Loop", EAudioPlayOptions.AtTransformPosition | EAudioPlayOptions.DestroyOnEnd);
            }

            // Object.DontDestroyOnLoad(gibberish.gameObject);
        }

        while (senetenceAt < dialogue.Count)
        {
            text.text += dialogue[senetenceAt][charAt];
            yield return new WaitForSeconds(textSpeed);
            charAt++;
            if (charAt > dialogue[senetenceAt].Length - 1)
            {
                charAt = 0;
                senetenceAt++;

                if (gibberish != null)
                {
                    gibberish.Pause();
                }

                yield return new WaitForSeconds(sentencePauseTime);

                if (gibberish != null)
                {
                    gibberish.Play();
                }

                if (senetenceAt < dialogue.Count)
                {
                    text.text = " ";
                }
                else
                {
                    senetenceAt++;
                }
            }
        }

        if (gibberish)
        {
            Object.Destroy(gibberish.gameObject);
        }
    }
}
