namespace EventSystem
{
    using System;
    using UnityEngine.InputSystem;

    /// <summary>
    /// A base class handleing all game events where two or more objects need to communicate with each other.
    /// </summary>
    public static class GameEvents
    {
        public static Action OnDashCarCollide { get; set; }

        public static Action OnDie { get; set; }

        public static Action<int> OnCollectFuel { get; set; }

        public static Action<int, PlayerController> OnAddPlayerSwitch { get; set; }

        public static Action<int, PlayerInput> OnRotatePlayer { get; set; }

        public static void DashCarCollide()
        {
            OnDashCarCollide?.Invoke(); // not yet implemented
        }

        public static void CollectFuel(int playerId)
        {
            OnCollectFuel?.Invoke(playerId);
        }

        public static void Die()
        {
            OnDie?.Invoke();
        }

        public static void AddPlayerSwitch(int playerId, PlayerController playerController)
        {
            OnAddPlayerSwitch?.Invoke(playerId, playerController);
        }

        public static void RotatePlayer(int currentPlayerId, PlayerInput playerInput)
        {
            OnRotatePlayer?.Invoke(currentPlayerId, playerInput);
        }
    }
}