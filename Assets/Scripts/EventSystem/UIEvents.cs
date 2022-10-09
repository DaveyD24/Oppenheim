namespace EventSystem
{
    using System;

    /// <summary>
    /// A base class handleing all game events where two or more objects need to communicate with each other.
    /// </summary>
    public static class UIEvents
    {
        public static Action<int, float> OnFuelChanged { get; set; }

        public static Action OnBeginAnnoucement { get; set; }

        public static Action<int, bool> OnCanvasStateChanged { get; set; }

        public static Action OnShowIntructions { get; set; }

        public static void FuelChanged(int playerID, float currentFuel)
        {
            // if this Action has one or more methods assigned to it then run this method
            OnFuelChanged?.Invoke(playerID, currentFuel);
        }

        public static void ShowInstructions()
        {
            // if this Action has one or more methods assigned to it then run this method
            OnShowIntructions?.Invoke();
        }

        public static void CanvasStateChanged(int playerID, bool value)
        {
            // if this Action has one or more methods assigned to it then run this method
            OnCanvasStateChanged?.Invoke(playerID, value);
        }

        public static void BeginAnnoucement()
        {
            OnBeginAnnoucement?.Invoke();
        }
    }
}
