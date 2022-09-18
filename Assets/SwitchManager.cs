using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwitchManager : MonoBehaviour
{
    private PlayerInputManager playerInputManager;
    private int playerAdded = 0;
    private List<PlayerInput> joinedPlayer = new List<PlayerInput>();
    [SerializeField] private InputActions joinAction;

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
        InputSystem.DisableDevice(Mouse.current);
    }

    private void OnEnable()
    {
        joinAction = new InputActions();
        joinAction.JoiningGame.Join.performed += Joining;

        joinAction.JoiningGame.Enable();
        // playerInputManager.onPlayerJoined += AddPlayer;
        print(InputSystem.devices.Count + "Total Number of Devices");

        int playerNo = 0;

        // below works but manually when a new player connects does not
        foreach (InputDevice device in InputSystem.devices)
        {
            if (device.displayName != "Mouse")
            {
                // PlayerInput player = playerInputManager.JoinPlayer(playerNo++, playerNo++, null, device);
                // AddPlayer(player);
            }
        }
    }

    private void OnDisable()
    {
        joinAction.JoiningGame.Join.performed -= Joining;
        // playerInputManager.onPlayerJoined -= AddPlayer;
        // joinAction.performed -= Joining;
    }

    private int playerNo = 0;

    private void Joining(InputAction.CallbackContext ctx)
    {
        if (playerInputManager.playerCount < playerInputManager.maxPlayerCount)
        {
            PlayerInput player = playerInputManager.JoinPlayer(playerNo, playerNo, null, ctx.control.device);
            if (player != null)
            {
                playerNo += 1;
                AddPlayer(player);
                player.actions.FindActionMap("JoiningGame").Disable();
            }
        }
    }

    private void AddPlayer(PlayerInput player)
    {
        joinedPlayer.Add(player);
        print("Device Used to join: " + player.devices[0].name);
        // InputSystem.DisableDevice(Mouse.current);

        // print(player.devices[1].name);
        //  player.enabled = false;

        player.neverAutoSwitchControlSchemes = true;
        switch (playerAdded)
        {
            case 0:
                Monkey.ActivateInput(player);
                break;
            case 1:
                Bat.ActivateInput(player);
                break;
            case 2:
                Soldier.ActivateInput(player);
                break;
            case 3:
                Car.ActivateInput(player);
                break;
            default:
                break;
        }

        playerAdded++;
        Debug.Log("New Player Added: " + playerAdded);
        print("Is Joining Enabled: " + player.playerIndex);
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RotatePlayer();
        }

        //if (playerInputManager.playerCount >= playerInputManager.maxPlayerCount && joinAction.JoiningGame.enabled)
        //{
        //    joinAction.JoiningGame.Disable();
        //}
        //else if (!joinAction.JoiningGame.enabled)
        //{
        //    joinAction.JoiningGame.Enable();
        //}

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
