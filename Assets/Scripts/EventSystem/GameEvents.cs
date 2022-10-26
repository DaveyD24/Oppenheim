namespace EventSystem
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// A base class handleing all game events where two or more objects need to communicate with each other.
    /// </summary>
    public static class GameEvents
    {

        // actions for when player controller uses the camera
        public static Action<Transform, float> OnCameraZoom { get; set; }

        public static Action<Transform, Vector3, bool> OnCameraMove { get; set; }

        public static Action<Transform> OnCameraFollowRotation { get; set; }

        public static Action OnDashCarCollide { get; set; }

        public static Action OnAddActiveInputs { get; set; }

        public static Action OnDie { get; set; }

        public static Action<bool> OnRespawnPlayersOnly { get; set; }

        public static Func<int> OnGetNumberActive { get; set; }

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

        [Exec(Description = "WOAH OHO OH")] // ????? what?
        public static void CollectFuel(int playerId)
        {
            OnCollectFuel?.Invoke(playerId);
        }

        public static void AddActiveInputs()
        {
            OnAddActiveInputs?.Invoke();
        }

        public static void SavePlayerData(int[] fuelDataReset)
        {
            OnSavePlayerData?.Invoke(fuelDataReset);
        }

        public static void RespawnPlayersOnly(bool bOnlyInactive)
        {
            OnRespawnPlayersOnly?.Invoke(bOnlyInactive);
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

        public static int GetNumberPlayersActive()
        {
            if (OnGetNumberActive != null)
            {
                return OnGetNumberActive();
            }

            return 0;
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

        // below are the events related to moving the camera with input recieved from the player controller class
        public static void CameraMove(Transform transform, Vector3 inputAmount, bool bCamFinished = false)
        {
            OnCameraMove?.Invoke(transform, inputAmount, bCamFinished);
        }

        public static void CameraZoom(Transform transform, float scrollAmount)
        {
            OnCameraZoom?.Invoke(transform, scrollAmount);
        }

        public static void CameraFollowRotation(Transform transform)
        {
            OnCameraFollowRotation?.Invoke(transform);
        }
    }
}