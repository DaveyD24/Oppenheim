using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchManager : MonoBehaviour
{

    [SerializeField] public RedController red;
    [SerializeField] public BlueController blue;
    [SerializeField] public GreenController green;
    [SerializeField] public YellowController yellow;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RotatePlayer();
        }
    }

    void RotatePlayer()
    {
        //bygr
        if (blue.isActive())
        {
            DeactivateAll();
            yellow.Activate();
        }
        else if (yellow.isActive())
        {
            DeactivateAll();
            green.Activate();
        }
        else if (green.isActive())
        {
            DeactivateAll();
            red.Activate();
        }
        else if (red.isActive())
        {
            DeactivateAll();
            blue.Activate();
        }

    }

    void DeactivateAll()
    {
        red.Deactivate();
        blue.Deactivate();
        green.Deactivate();
        yellow.Deactivate();
    }

    public GameObject GetActivePlayer()
    {
        if (red.isActive())
            return red.gameObject;
        if (blue.isActive())
            return blue.gameObject;
        if (green.isActive())
            return green.gameObject;
        if (yellow.isActive())
            return yellow.gameObject;

        return null;
    }
}
