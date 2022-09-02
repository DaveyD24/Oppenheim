namespace EventSystem
{

    using System;

    /// <summary>
    /// A base class handleing all game events where two or more objects need to communicate with each other.
    /// </summary>
    public static class GameEvents
    {
        public static Action OnDashCarCollide { get; set; }

        public static void DashCarCollide()
        {
            OnDashCarCollide?.Invoke(); // not yet implemented
        }
    }
}
