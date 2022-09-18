using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using EventSystem;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwitchManager : MonoBehaviour
{
    private PlayerInputManager playerInputManager;
    private int playerAdded = 0;
    private int playerNo = 0;

    private List<KeyValuePair<int, PlayerController>> controlledPlayers = new List<KeyValuePair<int, PlayerController>>();
    private List<KeyValuePair<int, PlayerController>> uncontrolledPlayers = new List<KeyValuePair<int, PlayerController>>();
    
    // private SortedDictionary<int, PlayerController> controlledPlayers = new SortedDictionary<int, PlayerController>();
    // private SortedDictionary<int, PlayerController> uncontrolledPlayers = new SortedDictionary<int, PlayerController>();
    [SerializeField] private InputActions joinAction;
    // private Dictionary<int, PlayerController> uncontrolledPlayers = new Dictionary<int, PlayerController>();

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

        GameEvents.OnAddPlayerSwitch += AddInactive;
        GameEvents.OnRotatePlayer += RotatePlayer;
    }

    private void OnDisable()
    {
        joinAction.JoiningGame.Join.performed -= Joining;

        GameEvents.OnAddPlayerSwitch -= AddInactive;
        GameEvents.OnRotatePlayer -= RotatePlayer;
    }

    /// <summary>
    /// An input event called to enable a player to join.
    /// </summary>
    /// <param name="ctx">the info about the input registered.</param>
    private void Joining(InputAction.CallbackContext ctx)
    {
        // checks if the device currently trying to connect is already connected or not
        InputControlList<InputDevice> unpairedDevices = UnityEngine.InputSystem.Users.InputUser.GetUnpairedInputDevices();
        InputDevice deviceUsing = ctx.control.device;
        bool bIsbeingUsed = true;
        foreach (InputDevice device in unpairedDevices)
        {
            print(device.name);
            if (device == deviceUsing)
            {
                // as the device is listed as unpaired it can be used
                bIsbeingUsed = false;
                break;
            }
        }

        // if uncontrolled players exist and this device is not in use, connect it to a player
        if (playerInputManager.playerCount < playerInputManager.maxPlayerCount && !bIsbeingUsed)
        {
            PlayerInput player = playerInputManager.JoinPlayer(playerNo, playerNo, null, ctx.control.device); // a function to auto handle the setup of the new device
            if (player != null)
            {
                playerNo += 1;
                AddPlayer(player);
                player.actions.FindActionMap("JoiningGame").Disable();
            }
        }
    }

    /// <summary>
    /// gets the specific player this input is connected with.
    /// </summary>
    /// <param name="player">The current device specific control setup using.</param>
    private void AddPlayer(PlayerInput player)
    {
        // controlledPlayers.Add(player);
        print("Device Used to join: " + player.devices[0].name);

        player.neverAutoSwitchControlSchemes = true;
        // switch (playerAdded)
        // {
        //    case 0:
        //        Monkey.ActivateInput(player);
        //        break;
        //    case 1:
        //        Bat.ActivateInput(player);
        //        break;
        //    case 2:
        //        Soldier.ActivateInput(player);
        //        break;
        //    case 3:
        //        Car.ActivateInput(player);
        //        break;
        //    default:
        //        break;
        // }

        int playerToControl = FindUncontrolledPlayer();

        controlledPlayers.Add(uncontrolledPlayers[0]); //new KeyValuePair<int, PlayerController>(playerToControl, uncontrolledPlayers[playerToControl].Value));
        uncontrolledPlayers.RemoveAt(0);

        controlledPlayers[controlledPlayers.Count - 1].Value.ActivateInput(player);

        playerAdded++;
        Debug.Log("New Player Added: " + playerAdded);
        print("Is Joining Enabled: " + player.playerIndex);
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            print(uncontrolledPlayers.Count);
            // RotatePlayer();
        }

        // if (playerInputManager.playerCount >= playerInputManager.maxPlayerCount && joinAction.JoiningGame.enabled)
        // {
        //     joinAction.JoiningGame.Disable();
        // }
        // else if (!joinAction.JoiningGame.enabled)
        // {
        //     joinAction.JoiningGame.Enable();
        // }

        // Debug.Log(GetActivePlayer().gameObject.name);
    }


    /// <summary>
    /// Switch out the current player for an uncontrolled one.
    /// </summary>
    /// <param name="currentPlayerId">The id of the current player which is controlled.</param>
    /// <param name="playerInput">The input system information this player has.</param>
    private void RotatePlayer(int currentPlayerId, PlayerInput playerInput)
    {
        if (uncontrolledPlayers.Count > 0)
        {
            int currentPlayerIndex = 0;
            for (int i = 0; i > controlledPlayers.Count; i++)
            {
                if (controlledPlayers[i].Key == currentPlayerId)
                {
                    currentPlayerId = i;
                    break;
                }
            }

            controlledPlayers[currentPlayerIndex].Value.DeactivateInput();

            // int playerToControl = uncontrolledPlayers[0];

            controlledPlayers.Add(uncontrolledPlayers[0]);//playerToControl, uncontrolledPlayers[playerToControl]);
            uncontrolledPlayers.RemoveAt(0);
            controlledPlayers[controlledPlayers.Count - 1].Value.ActivateInput(playerInput);

            uncontrolledPlayers.Add(controlledPlayers[currentPlayerIndex]);
            controlledPlayers.RemoveAt(currentPlayerIndex);

            // bygr
            ////if (Bat.IsActive())
            ////{
            ////    DeactivateAll();
            ////    Car.Activate();
            ////}
            ////else if (Car.IsActive())
            ////{
            ////    DeactivateAll();
            ////    Monkey.Activate();
            ////}
            ////else if (Monkey.IsActive())
            ////{
            ////    DeactivateAll();
            ////    Soldier.Activate();
            ////}
            ////else if (Soldier.IsActive())
            ////{
            ////    DeactivateAll();
            ////    Bat.Activate();
            ////}
        }
    }

    private int FindUncontrolledPlayer()
    {
        int playerToControl = 0;

        // get a random uncontrolled player
        //foreach (var item in uncontrolledPlayers)
        //{
        //    playerToControl = item.Key;
        //    break;
        //}

        return uncontrolledPlayers[0].Key;
    }

    private void DeactivateAll()
    {
        Soldier.Deactivate();
        Bat.Deactivate();
        Monkey.Deactivate();
        Car.Deactivate();
    }

    private void AddInactive(int playerId, PlayerController playerController)
    {
        uncontrolledPlayers.Add(new KeyValuePair<int, PlayerController>(playerId, playerController));
    }
}
