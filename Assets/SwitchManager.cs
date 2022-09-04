using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchManager : MonoBehaviour
{

    [SerializeField] public SoldierMovement red;
    [SerializeField] public Bat blue;
    [SerializeField] public MonkeyController green;
    [SerializeField] public PlayerController yellow;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        green.Activate();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RotatePlayer();
        }

        Debug.Log(GetActivePlayer().gameObject.name);
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
