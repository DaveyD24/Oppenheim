namespace EventSystem
{
    using System;

    /// <summary>
    /// A base class handleing all game events where two or more objects need to communicate with each other.
    /// </summary>
    public static class UIEvents
    {
        public static Action<int, float> OnFuelChanged { get; set; }

        public static void FuelChanged(int playerID, float currentFuel)
        {
            // if this Action has one or more methods assigned to it then run this method
            OnFuelChanged?.Invoke(playerID, currentFuel);
        }
    }
}