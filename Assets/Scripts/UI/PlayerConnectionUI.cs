using System.Collections;
using System.Collections.Generic;
using EventSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name
public class PlayerConnectionData
#pragma warning restore SA1649 // File name should match first type name
#pragma warning restore SA1402 // File may only contain a single type
{
    [field: SerializeField] public GameObject ConnectedTxt { get; private set; }

    [field: SerializeField] public GameObject WaitTxt { get; private set; }

    [field: SerializeField] public GameObject DisconnectBtn { get; private set; }

    [field: SerializeField] public Image ControllerImg { get; private set; }

    public PlayerInput InputUIConnectedTo { get; set; }
}

public class PlayerConnectionUI : MonoBehaviour
{
    [SerializeField] private List<PlayerConnectionData> playerConnectionData = new List<PlayerConnectionData>();
    [SerializeField] private int playerNumber;

    public void RemovePlayer(GameObject connectionTxt)
    {
        foreach (PlayerConnectionData item in playerConnectionData)
        {
            // lopop through all player UI's until one with the desired connectionTxt is found so can get the appropriate input to remove
            if (item.ConnectedTxt == connectionTxt)
            {
                item.ConnectedTxt.SetActive(false);
                item.WaitTxt.SetActive(true);
                item.DisconnectBtn.SetActive(false);
                item.ControllerImg.gameObject.SetActive(false);
                UIEvents.PlayerConnectionRemove(item.InputUIConnectedTo);
                item.InputUIConnectedTo = null;
                return;
            }
        }
    }

    private void OnEnable()
    {
        UIEvents.OnPlayerConnectionUIAdd += AddPlayer;
    }

    private void OnDisable()
    {
        UIEvents.OnPlayerConnectionUIAdd -= AddPlayer;
    }

    private void Start()
    {
        if (SwitchManager.PlayerInputConnection.Count > 0)
        {
            foreach (var item in SwitchManager.PlayerInputConnection)
            {
                AddPlayer(item.Key, item.Key.devices[0].name);
            }
        }
        else if (GetDeviceUsingAsInput.DeviceUsingForInput != null)
        {
            UIEvents.AddSpecificDevice(GetDeviceUsingAsInput.DeviceUsingForInput, true);
        }
    }

    private void AddPlayer(PlayerInput input, string deviceName)
    {
        foreach (PlayerConnectionData item in playerConnectionData)
        {
            // lopop through all player UI's until one without a device connected to it is found
            if (!item.ConnectedTxt.activeSelf)
            {
                item.ConnectedTxt.SetActive(true);
                item.WaitTxt.SetActive(false);
                item.DisconnectBtn.SetActive(true);
                item.ControllerImg.gameObject.SetActive(true);

                if (deviceName == "Mouse")
                {
                    deviceName = "Keyboard";
                }

                if (deviceName != "Keyboard")
                {
                    deviceName = "gamepad";
                }

                item.ControllerImg.sprite = UIEvents.GetControlSprite(deviceName, "Join");
                item.InputUIConnectedTo = input;
                return;
            }
        }
    }
}
