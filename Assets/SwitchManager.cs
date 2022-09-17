using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwitchManager : MonoBehaviour
{
    private PlayerInputManager playerInputManager;
    private int playerAdded = 0;
    private List<PlayerInput> joinedPlayer = new List<PlayerInput>();

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
        playerInputManager = GetComponent<PlayerInputManager>();
    }

    private void OnEnable()
    {
        playerInputManager.onPlayerJoined += AddPlayer;
        print(InputSystem.devices.Count + "Total Number of Devices");

        //////////////////int playerNo = 0;

        //////////////////foreach (InputDevice device in InputSystem.devices)
        //////////////////{
        //////////////////    PlayerInput player = playerInputManager.JoinPlayer(playerNo++, playerNo++, null, device);
        //////////////////    AddPlayer(player);
        //////////////////}
    }

    private void OnDisable()
    {
        playerInputManager.onPlayerJoined -= AddPlayer;
    }

    private void AddPlayer(PlayerInput player)
    {
        joinedPlayer.Add(player);
        switch (playerAdded)
        {
            case 0:
                Monkey.ActivateInput(player);
                break;
            case 1:
                Car.ActivateInput(player);
                break;
            case 2:
                Soldier.ActivateInput(player);
                break;
            case 3:
                Bat.ActivateInput(player);
                break;
            default:
                break;
        }

        playerAdded++;
        Debug.Log("New Player Added");
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
