using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchManager : MonoBehaviour
{

    [SerializeField] public PlayerController soldier;
    [SerializeField] public PlayerController bat;
    [SerializeField] public PlayerController monkey;
    [SerializeField] public PlayerController car; // car


    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        monkey.Activate();
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
        if (bat.IsActive())
        {
            DeactivateAll();
            car.Activate();
        }
        else if (car.IsActive())
        {
            DeactivateAll();
            monkey.Activate();
        }
        else if (monkey.IsActive())
        {
            DeactivateAll();
            soldier.Activate();
        }
        else if (soldier.IsActive())
        {
            DeactivateAll();
            bat.Activate();
        }

    }

    private void DeactivateAll()
    {
        soldier.Deactivate();
        bat.Deactivate();
        monkey.Deactivate();
        car.Deactivate();
    }

    public GameObject GetActivePlayer()
    {
        if (soldier.IsActive())
            return soldier.gameObject;
        if (bat.IsActive())
            return bat.gameObject;
        if (monkey.IsActive())
            return monkey.gameObject;
        if (car.IsActive())
            return car.gameObject;

        return null;
    }
}
