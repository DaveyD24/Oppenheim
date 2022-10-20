namespace EventSystem
{
    using System;
    using UnityEngine.InputSystem;
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// A base class handleing all game events where two or more objects need to communicate with each other.
    /// </summary>
    public static class GameEvents
    {
        public static Action OnDashCarCollide { get; set; }

        public static Action OnDie { get; set; }

        public static Action<int[]> OnSavePlayerData { get; set; }

        public static Func<int[]> OnGatherInvalidSaveFuel { get; set; }

        public static Action<int> OnCollectFuel { get; set; }

        public static Action<int> OnAddPlayerSwitch { get; set; }

        public static Action<int, PlayerInput> OnRotatePlayer { get; set; }

        public static Action<int, PlayerInput> OnActivatePlayer { get; set; }

        // in the below func the last argument(bool) is the return type while the others two are the input parameters
        public static Func<float, Vector3, bool> OnPlayerCompareDistance { get; set; } // used to enable the players distance to another object to be calculated and compared

        public static Action<int> OnDeactivatePlayer { get; set; }

        public static void DashCarCollide()
        {
            OnDashCarCollide?.Invoke(); // not yet implemented
        }

        [Exec(Description = "WOAH OHO OH")]
        public static void CollectFuel(int playerId)
        {
            OnCollectFuel?.Invoke(playerId);
        }

        public static void SavePlayerData(int[] fuelDataReset)
        {
            OnSavePlayerData?.Invoke(fuelDataReset);
        }

        public static void Die()
        {
            // reset all sections of the level to their current saved state
            foreach (GatherStageObjects item in UnityEngine.MonoBehaviour.FindObjectsOfType<GatherStageObjects>())
            {
                item.LoadSection();
            }

            OnDie?.Invoke();
        }

        public static int[] GatherInvalidSaveFuel()
        {
            if (OnGatherInvalidSaveFuel != null)
            {
                return OnGatherInvalidSaveFuel();
            }

            return null;
        }

        public static bool PlayerCompareDistance(float distance, Vector3 otherPosition)
        {
            if (OnPlayerCompareDistance != null)
            {
                return OnPlayerCompareDistance(distance, otherPosition);
            }

            return false;
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