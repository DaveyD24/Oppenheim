using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using EventSystem;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwitchManager : MonoBehaviour
{
    private PlayerInputManager playerInputManager;
    private int playerAdded = 0;
    private int playerNo = 0;

    private List<int> controlledPlayers = new List<int>();
    private List<int> uncontrolledPlayers = new List<int>();

    [SerializeField] private InputActions joinAction; // the input for joining as a new controller

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
        else if (Bat.IsActive())
        {
            return Bat.gameObject;
        }
        else if (Monkey.IsActive())
        {
            return Monkey.gameObject;
        }
        else if (Car.IsActive())
        {
            return Car.gameObject;
        }

        return null;
    }

    public void GetAllActivePlayerTransforms(out Transform[] outActivePlayers)
    {
        GetPlayers(out PlayerController[] players);
                
        int numberActive = players.Count(b => b.IsActive());
        outActivePlayers = new Transform[numberActive];
        
        for (int b = 0, i = 0; b < 4; ++b)
        {
            if (players[b])
            {
                outActivePlayers[i++] = players[b].transform;
            }
        }
    }

    private void Awake()
    {
        playerInputManager = GetComponent<PlayerInputManager>();
        InputSystem.DisableDevice(Mouse.current);
    }

    private void OnEnable()
    {
        joinAction = new InputActions();
        joinAction.JoiningGame.Join.performed += Joining;

        joinAction.JoiningGame.Enable();

        // playerInputManager.onPlayerJoined += AddPlayer;
        //print(InputSystem.devices.Count + "Total Number of Devices");

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
            //print(device.name);
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
        //print("Device Used to join: " + player.devices[0].name);

        player.neverAutoSwitchControlSchemes = true;

        (int playerToControl, int playerID) = FindUncontrolledPlayer();

        if (playerToControl != -1)
        {
            controlledPlayers.Add(playerID);
            uncontrolledPlayers.RemoveAt(playerToControl);

            GameEvents.ActivatePlayer(playerID, player);

            playerAdded++;
            //Debug.Log("New Player Added: " + playerAdded);
            //print("Is Joining Enabled: " + player.playerIndex);
        }
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
            GameEvents.DeactivatePlayer(currentPlayerId);

            (int playerToControl, int playerID) = FindUncontrolledPlayer();
            if (playerToControl != -1)
            {
                // activate the choosen uncontrolled player
                controlledPlayers.Add(playerID);
                uncontrolledPlayers.RemoveAt(playerToControl);
                GameEvents.ActivatePlayer(playerID, playerInput);

                // deactivate the current controlled player
                uncontrolledPlayers.Add(currentPlayerId);
                controlledPlayers.Remove(currentPlayerId);

                // Switch Camera Targets.
                PlayerController outPlayer = GetPlayerByID(currentPlayerId);
                PlayerController inPlayer = GetPlayerByID(playerID);
                inPlayer.TrackingCamera = outPlayer.TrackingCamera;
                inPlayer.TrackingCamera.Target = inPlayer.transform;
                outPlayer.TrackingCamera = null;
            }
        }
    }

    /// <summary>
    /// Find an uncontrolled player.
    /// </summary>
    /// <returns>the index of the first uncontrolled player, if it exists.</returns>
    private (int, int) FindUncontrolledPlayer()
    {
        int playerToControl = -1;
        int playerID = -1;
        if (uncontrolledPlayers.Count > 0)
        {
            playerToControl = 0;
            playerID = uncontrolledPlayers[playerToControl];
        }

        return (playerToControl, playerID);
    }

    // private void DeactivateAll()
    // {
    //     Soldier.Deactivate();
    //     Bat.Deactivate();
    //     Monkey.Deactivate();
    //     Car.Deactivate();
    // }

    /// <summary>
    /// Adds a new player to the list of inactive players.
    /// </summary>
    /// <param name="playerId">The id of the player.</param>
    private void AddInactive(int playerId)
    {
        uncontrolledPlayers.Add(playerId);
    }

    /// <summary>Get every Player in the game.</summary>
    public void GetPlayers(out PlayerController[] outPlayers)
    {
        // TODO: Make a non-alloc version.
        outPlayers = new PlayerController[4];

        outPlayers[0] = Soldier;
        outPlayers[1] = Bat;
        outPlayers[2] = Monkey;
        outPlayers[3] = Car;
    }

    public PlayerController GetPlayerByID(int playerID)
    {
        GetPlayers(out PlayerController[] players);
        return Array.Find(players, p => p.PlayerIdSO.PlayerID == playerID);
    }
}
