using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventSystem;

public class InstructionBookSequence : Node<ActionSequence>
{
    public InstructionBookSequence(ActionSequence blackboard)
    {
        this.Blackboard = blackboard;
    }

    public override void Init()
    {
        base.Init();

        UIEvents.TutorialUIPopup(string.Empty, "Amazing!!!", 10);
        Blackboard.oppenheimObj.SetActive(false);
        Blackboard.StartCoroutine(WaitTutDone());
    }

    public override ENodeState Evaluate()
    {
        return ENodeState.Running; // can be whatever return is nessesary
    }

    private IEnumerator WaitTutDone()
    {
        yield return new WaitForSeconds(3);
        UIEvents.TutorialUIPopup(string.Empty, "Use this book, it will contain details on everything I have taught", 10);

        yield return new WaitForSeconds(8);
        UIEvents.TutorialUIPopup("Open/Close Book", string.Empty, 10, true);

        yield return new WaitForSeconds(5);
        UIEvents.TutorialUIPopup(string.Empty, "Complete the challenges, make it to the head office and save my life", 10);
    }

    public override void End()
    {
        base.End();
    }
}
