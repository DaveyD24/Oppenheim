using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanParent : MonoBehaviour
{
    [SerializeField] private Fan[] childFans;

    public void SetFansActive(bool enabled)
    {
        foreach (Fan fan in childFans)
        {
            fan.SetFanState(enabled);
        }
    }
}
