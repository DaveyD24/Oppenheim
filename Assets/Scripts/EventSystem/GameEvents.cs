namespace EventSystem
{
    using System;

    /// <summary>
    /// A base class handleing all game events where two or more objects need to communicate with each other.
    /// </summary>
    public static class GameEvents
    {
        public static Action OnDashCarCollide { get; set; }

        public static Action OnDie { get; set; }

        public static Action<int> OnCollectFuel { get; set; }

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
    }
}
