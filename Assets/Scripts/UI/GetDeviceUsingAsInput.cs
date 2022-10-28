using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// on the main menu track the device using and set it as the device that will be auto connected to the characters
/// get the device it has detected the player as using.
/// </summary>
public class GetDeviceUsingAsInput : MonoBehaviour
{
    public static InputDevice DeviceUsingForInput { get; set; } = null;

    private void Start()
    {
        // listen for activity on any unpaired device, as this would be the one the player is using
        ++UnityEngine.InputSystem.Users.InputUser.listenForUnpairedDeviceActivity;
        UnityEngine.InputSystem.Users.InputUser.onUnpairedDeviceUsed +=
        (control, eventPtr) =>
        {
            DeviceUsingForInput = control.device;
        };
    }
}
