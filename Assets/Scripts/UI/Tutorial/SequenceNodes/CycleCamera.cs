using System.Collections;
using System.Collections.Generic;
using EventSystem;
using UnityEngine;

/// <summary>
/// Cycle through all the cameras controls, only completing once the doors switch has been acitvated.
/// </summary>
public class CycleCamera : Node<ActionSequence>
{
    private int settingAt = 0;
    private float timePerItem;

    private string[] cameraSequence = new string[] { "Move Camera", "Zoom In/Out", "Follow Player's Rotation", "Move" };

    public CycleCamera(ActionSequence blackboard, float timePerItem)
    {
        this.Blackboard = blackboard;
        this.timePerItem = timePerItem;
    }

    public override void Init()
    {
        base.Init();

        Blackboard.oppenheimText.gameObject.SetActive(false);
        settingAt = 0;

        // display the first camera setting
        DisplayCameraSetting();

        Blackboard.StartCoroutine(CycleCameraSettings());
    }

    public override ENodeState Evaluate()
    {
        // wait until the door has been opened before continuing
        return Blackboard.BDoorOpen ? ENodeState.Success : ENodeState.Running;
    }

    private IEnumerator CycleCameraSettings()
    {
        yield return new WaitForSeconds(timePerItem);
        DisplayCameraSetting();

        // call again to infinitely loop through the settings
        Blackboard.StartCoroutine(CycleCameraSettings());
    }

    private void DisplayCameraSetting()
    {
        string[] inputDeviceNames = UIEvents.GetInputTypes();
        if (inputDeviceNames.Length >= 2)
        {
            UIEvents.ShowInputControls(cameraSequence[settingAt], inputDeviceNames.Length, inputDeviceNames[0], inputDeviceNames[1]);
        }
        else
        {
            UIEvents.ShowInputControls(cameraSequence[settingAt], inputDeviceNames.Length, inputDeviceNames[0], string.Empty);
        }

        settingAt++;
        if (settingAt >= cameraSequence.Length)
        {
            settingAt = 0;
        }
    }

    public override void End()
    {
        base.End();
        Blackboard.StopAllCoroutines();
        Blackboard.oppenheimText.gameObject.SetActive(true);
        UIEvents.HideInputControls();
    }
}
