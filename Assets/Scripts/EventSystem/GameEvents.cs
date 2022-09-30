namespace EventSystem
{
    using System;
    using UnityEngine.InputSystem;
    using System.Collections.Generic;

    /// <summary>
    /// A base class handleing all game events where two or more objects need to communicate with each other.
    /// </summary>
    public static class GameEvents
    {
        public static Action OnDashCarCollide { get; set; }

        public static Action OnDie { get; set; }

        public static Action OnSavePlayerData { get; set; }

        public static Action<int> OnCollectFuel { get; set; }

        public static Action<int> OnAddPlayerSwitch { get; set; }

        public static Action<int, PlayerInput> OnRotatePlayer { get; set; }

        public static Action<int, PlayerInput> OnActivatePlayer { get; set; }

        public static Action<int> OnDeactivatePlayer { get; set; }

        public static void DashCarCollide()
        {
            OnDashCarCollide?.Invoke(); // not yet implemented
        }

        public static void CollectFuel(int playerId)
        {
            OnCollectFuel?.Invoke(playerId);
        }

        public static void SavePlayerData()
        {
            OnSavePlayerData?.Invoke();
        }

        public static void Die()
        {
            OnDie?.Invoke();

            // reset all sections of the level to their current saved state
            foreach (GatherStageObjects item in UnityEngine.MonoBehaviour.FindObjectsOfType<GatherStageObjects>())
            {
                item.LoadSection();
            }
        }

        // the below methods are to do with player input and adding/removing a player from the active list
        public static void AddPlayerSwitch(int playerId)
        {
            OnAddPlayerSwitch?.Invoke(playerId);
        }

        public static void RotatePlayer(int currentPlayerId, PlayerInput playerInput)
        {
            OnRotatePlayer?.Invoke(currentPlayerId, playerInput);
        }

        public static void ActivatePlayer(int currentPlayerId, PlayerInput playerInput)
        {
            OnActivatePlayer?.Invoke(currentPlayerId, playerInput);
        }

        public static void DeactivatePlayer(int currentPlayerId)
        {
            OnDeactivatePlayer?.Invoke(currentPlayerId);
        }
    }
}