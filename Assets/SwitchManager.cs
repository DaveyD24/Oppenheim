using System;
using System.Collections.Generic;
using EventSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SwitchManager : MonoBehaviour
{
    private PlayerInputManager playerInputManager;
    private int playerNo = 0;

    // a list of each input and the id of the player connected to it
    public static List<KeyValuePair<PlayerInput, int>> PlayerInputConnection { get; set; } = new List<KeyValuePair<PlayerInput, int>>(); // a list so that it is ordered and can ensure that player 1, 2 etc will always be in the correct order

    public List<int> ControlledPlayers { get; set; } = new List<int>();

    public List<int> UncontrolledPlayers { get; set; } = new List<int>();

    public int NumberOfPlayers { get; set; } = 0;

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

    public void GetAllActivePlayerTransforms(out Transform[] outActivePlayerTransforms)
    {
        List<PlayerController> active = GetActivePlayers();
        outActivePlayerTransforms = new Transform[active.Count];

        for (int i = 0; i < active.Count; ++i)
        {
            outActivePlayerTransforms[i] = active[i].transform;
        }
    }

    private void Awake()
    {
        playerInputManager = GetComponent<PlayerInputManager>();
        // InputSystem.DisableDevice(Mouse.current);
    }

    private void OnEnable()
    {
        // playerInputManager.onPlayerLeft += PlayerLeft;
        joinAction = new InputActions();
        joinAction.JoiningGame.Join.performed += Joining;

        joinAction.JoiningGame.Enable();

        // playerInputManager.onPlayerJoined += AddPlayer;
        // playerInputManager.onPlayerLeft
        // print(InputSystem.devices.Count + "Total Number of Devices");
        GameEvents.OnAddPlayerSwitch += AddInactive;
        GameEvents.OnRotatePlayer += RotatePlayer;
        GameEvents.OnPlayerCompareDistance += CompareControlledPlayerDistance;
        GameEvents.OnGetNumberActive += NumberPlayersActive;
        GameEvents.OnAddActiveInputs += AddActiveInputsToPlayers;

        UIEvents.OnGetInputTypes += GetInputControlMethods;
        UIEvents.OnPlayerConnectionRemove += PlayerRemoved;
        UIEvents.OnAddSpecificDevice += JoinSpecificDevice;
    }

    private void OnDisable()
    {
        // playerInputManager.onPlayerLeft -= PlayerLeft;
        joinAction.JoiningGame.Join.performed -= Joining;

        GameEvents.OnAddPlayerSwitch -= AddInactive;
        GameEvents.OnRotatePlayer -= RotatePlayer;
        GameEvents.OnPlayerCompareDistance -= CompareControlledPlayerDistance;
        GameEvents.OnGetNumberActive -= NumberPlayersActive;
        GameEvents.OnAddActiveInputs -= AddActiveInputsToPlayers;

        UIEvents.OnGetInputTypes -= GetInputControlMethods;
        UIEvents.OnPlayerConnectionRemove -= PlayerRemoved;
        UIEvents.OnAddSpecificDevice -= JoinSpecificDevice;
    }

    /// <summary>
    /// An input event called to enable a player to join.
    /// </summary>
    /// <param name="ctx">the info about the input registered.</param>
    private void Joining(InputAction.CallbackContext ctx)
    {
        // only perform if at least one of the players is active
        bool bIsPlayerJoinScene = SceneManager.GetActiveScene().name == "Player Join";
        if (bIsPlayerJoinScene || Monkey.gameObject.activeSelf || Car.gameObject.activeSelf || Bat.gameObject.activeSelf || Soldier.gameObject.activeSelf)
        {
            // checks if the device currently trying to connect is already connected or not
            InputControlList<InputDevice> unpairedDevices = UnityEngine.InputSystem.Users.InputUser.GetUnpairedInputDevices();
            InputDevice deviceUsing = ctx.control.device;
            bool bIsbeingUsed = true;
            foreach (InputDevice device in unpairedDevices)
            {
                // print(device.name);
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
                JoinSpecificDevice(deviceUsing, bIsPlayerJoinScene);
            }
        }
    }

    private void JoinSpecificDevice(InputDevice device, bool bIsPlayerJoinScene)
    {
        PlayerInput player = playerInputManager.JoinPlayer(playerNo, playerNo, null, device); // a function to auto handle the setup of the new device
        if (player != null)
        {
            playerNo += 1;
            AddPlayer(player, bIsPlayerJoinScene);
            player.actions.FindActionMap("JoiningGame").Disable();
            // print(player.devices[0].name);
        }
    }

    /// <summary>
    /// gets the specific player this input is connected with.
    /// </summary>
    /// <param name="player">The current device specific control setup using.</param>
    private void AddPlayer(PlayerInput player, bool bIsPlayerJoinScene)
    {
        // controlledPlayers.Add(player);
        // print("Device Used to join: " + player.devices[0].name);
        player.neverAutoSwitchControlSchemes = true;

        (int playerToControl, int playerID) = FindUncontrolledPlayer();

        if (playerToControl != -1 || bIsPlayerJoinScene)
        {
            PlayerInputConnection.Add(new KeyValuePair<PlayerInput, int>(player, playerID));

            if (playerToControl != -1)
            {
                ControlledPlayers.Add(playerID);
                UncontrolledPlayers.RemoveAt(playerToControl);
                GetPlayerByID(playerID).HumanPlayerIndex = (EPlayer)NumberOfPlayers;
                Debug.Log("New Player Added: " + playerID + " Count:" + ControlledPlayers.Count);
            }

            NumberOfPlayers++;

            if (bIsPlayerJoinScene)
            {
                UIEvents.PlayerConnectionUIAdd(player, player.devices[0].name);
            }
            else
            {
                GameEvents.ActivatePlayer(playerID, player);
            }

            // Debug.Log("New Player Added: " + playerAdded);
            // print("Is Joining Enabled: " + player.playerIndex);
        }
    }

    /// <summary>
    /// for all players which already have input setup when loading into a new level add these inputs to the appropriate players.
    /// </summary>
    private void AddActiveInputsToPlayers()
    {
        for (int i = 0; i < PlayerInputConnection.Count; i++)
        {
            AddInputToPlayer(PlayerInputConnection[i].Key, i, PlayerInputConnection[i].Value);
            playerNo++;

            // issue occuring if this is called after the player connects to the game
        }
    }

    /// <summary>
    /// if an input method already exists simply add it to one of the players.
    /// </summary>
    private void AddInputToPlayer(PlayerInput input, int inputConnectionIndex, int playerIndex)
    {
        if (playerIndex == -1)
        {
            (int playerToControl, int playerID) = FindUncontrolledPlayer();

            if (playerToControl != -1)
            {
                PlayerInputConnection[inputConnectionIndex] = new KeyValuePair<PlayerInput, int>(input, playerID);

                ControlledPlayers.Add(playerID); // issle likly being caused by adding input at same time pressing r key
                UncontrolledPlayers.RemoveAt(playerToControl);
                NumberOfPlayers++;

                GetPlayerByID(playerID).HumanPlayerIndex = (EPlayer)NumberOfPlayers;

                GameEvents.ActivatePlayer(playerID, input);
                Debug.LogError("Player To Activate: " + playerID);

                // Debug.Log("New Player Added: " + playerAdded);
                // print("Is Joining Enabled: " + player.playerIndex);
            }
        }
        else
        {
            if (!ControlledPlayers.Contains(playerIndex))
            {
                ControlledPlayers.Add(playerIndex);
                Debug.Log("Added Input: " + playerIndex + "amount: " + ControlledPlayers.Count);
                UncontrolledPlayers.Remove(playerIndex);
            }

            NumberOfPlayers++;
            GetPlayerByID(playerIndex).HumanPlayerIndex = (EPlayer)NumberOfPlayers;

            // as this input already mapped to a specific player, just connect it to that player
            GameEvents.ActivatePlayer(playerIndex, input);
        }
    }

    /// <summary>
    /// Switch out the current player for an uncontrolled one.
    /// </summary>
    /// <param name="currentPlayerId">The id of the current player which is controlled.</param>
    /// <param name="playerInput">The input system information this player has.</param>
    private void RotatePlayer(int currentPlayerId, PlayerInput playerInput)
    {
        if (UncontrolledPlayers.Count > 0 && ControlledPlayers.Count <= PlayerInputConnection.Count)
        {
            GameEvents.DeactivatePlayer(currentPlayerId);

            (int playerToControl, int playerID) = FindUncontrolledPlayer();
            if (playerToControl != -1)
            {
                // activate the choosen uncontrolled player
                ControlledPlayers.Add(playerID);
                UncontrolledPlayers.RemoveAt(playerToControl);

                // deactivate the current controlled player
                UncontrolledPlayers.Add(currentPlayerId);

                for (int i = 0; i < ControlledPlayers.Count; i++)
                {
                    if (ControlledPlayers[i] == currentPlayerId)
                    {
                        ControlledPlayers.RemoveAt(i);
                        break;
                    }
                }

                if (ControlledPlayers.Count > PlayerInputConnection.Count)
                {
                    Debug.LogAssertion("failed to remove the item: " + currentPlayerId);
                }

                PlayerController inControl = GetPlayerByID(playerID);
                PlayerController outControl = GetPlayerByID(currentPlayerId);
                inControl.HumanPlayerIndex = outControl.HumanPlayerIndex;
                outControl.HumanPlayerIndex = EPlayer.None;

                GameEvents.ActivatePlayer(playerID, playerInput);
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
        if (UncontrolledPlayers.Count > 0)
        {
            playerToControl = 0;
            playerID = UncontrolledPlayers[playerToControl];
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
        UncontrolledPlayers.Add(playerId);
    }

    /// <summary>
    /// checks if all the controlled players are within a specified distance from another objects position.
    /// </summary>
    /// <param name="distance">the max distance away it can be.</param>
    /// <param name="otherPosition">the position of the object comparing with.</param>
    /// <returns>a bool stating whether all active players are within the specified distance or not.</returns>
    private bool CompareControlledPlayerDistance(float distance, Vector3 otherPosition)
    {
        int numDistancesValid = 0;

        foreach (PlayerController player in GetActivePlayers())
        {
            if (Vector3.Distance(player.gameObject.transform.position, otherPosition) < distance)
            {
                numDistancesValid++;
            }
        }

        return numDistancesValid > 0 && NumberOfPlayers >= ControlledPlayers.Count;
    }

    /// <summary>
    /// for each connected player, get the name of the input device using.
    /// </summary>
    /// <returns>a list of the name of each players input device.</returns>
    private string[] GetInputControlMethods()
    {
        string[] inputNames = new string[PlayerInputConnection.Count];

        for (int i = 0; i < PlayerInputConnection.Count; i++)
        {
            inputNames[i] = PlayerInputConnection[i].Key.devices[0].name;
            //print("Player Input Device Name: " + inputNames[i]);
        }

        return inputNames;
    }

    private void PlayerRemoved(PlayerInput input)
    {
        foreach (var item in PlayerInputConnection)
        {
            // loop through until find the one looking for
            if (item.Key == input)
            {
                input.actions.FindActionMap("JoiningGame").Enable();
                PlayerInputConnection.Remove(item);
                Destroy(item.Key.gameObject);
                NumberOfPlayers--;
                return;
            }
        }
    }

    // TODO: fix up the player left method so it can properly handle when a player has left the game
    // private void PlayerLeft(PlayerInput playerInput)
    // {
    //     // not working yet, but not too vital to have working at the moment
    //     Debug.Log("input device has disconnected");
    //     GameEvents.DeactivatePlayer(playerInputConnection[playerInput]);
    //     playerInputConnection.Remove(playerInput);
    //
    //     --numberOfPlayers;
    // }
    public int GetNumberOfPlayers() => NumberOfPlayers;

    /// <summary>Get every Player in the game.</summary>
    public void GetAllPlayers(out PlayerController[] outPlayers)
    {
        // TODO: Make a non-alloc version.
        outPlayers = new PlayerController[4];

        outPlayers[0] = Soldier;
        outPlayers[1] = Bat;
        outPlayers[2] = Monkey;
        outPlayers[3] = Car;
    }

    public List<PlayerController> GetActivePlayers()
    {
        List<PlayerController> retVal = new List<PlayerController>();

        foreach (int iD in ControlledPlayers)
        {
            retVal.Add(GetPlayerByID(iD));
        }

        return retVal;
    }

    public PlayerController GetPlayerByID(int playerID)
    {
        GetAllPlayers(out PlayerController[] players);
        return Array.Find(players, p => p.PlayerIdSO.PlayerID == playerID);
    }

    private int NumberPlayersActive()
    {
        return ControlledPlayers.Count;
    }
}
