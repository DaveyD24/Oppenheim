using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchManager : MonoBehaviour
{

    [field: SerializeField] public PlayerController Soldier { get; private set; }

    [field: SerializeField] public PlayerController Bat { get; private set; }

    [field: SerializeField] public PlayerController Monkey { get; private set; }

    [field: SerializeField] public PlayerController Car { get; private set; }

    public GameObject GetActivePlayer()
    {
        if (Soldier.IsActive())
        {
            return Soldier.gameObject;
        }

        if (Bat.IsActive())
        {
            return Bat.gameObject;
        }

        if (Monkey.IsActive())
        {
            return Monkey.gameObject;
        }

        if (Car.IsActive())
        {
            return Car.gameObject;
        }

        return null;
    }

    private void Awake()
    {
        Monkey.Activate();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RotatePlayer();
        }

        // Debug.Log(GetActivePlayer().gameObject.name);
    }

    private void RotatePlayer()
    {
        // bygr
        if (Bat.IsActive())
        {
            DeactivateAll();
            Car.Activate();
        }
        else if (Car.IsActive())
        {
            DeactivateAll();
            Monkey.Activate();
        }
        else if (Monkey.IsActive())
        {
            DeactivateAll();
            Soldier.Activate();
        }
        else if (Soldier.IsActive())
        {
            DeactivateAll();
            Bat.Activate();
        }
    }

    private void DeactivateAll()
    {
        Soldier.Deactivate();
        Bat.Deactivate();
        Monkey.Deactivate();
        Car.Deactivate();
    }
}
