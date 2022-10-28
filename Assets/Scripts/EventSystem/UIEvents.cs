namespace EventSystem
{
    using System;
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// A base class handleing all game events where two or more objects need to communicate with each other.
    /// </summary>
    public static class UIEvents
    {
        public static Action<InputDevice, bool> OnAddSpecificDevice { get; set; }

        public static Action<PlayerInput, string> OnPlayerConnectionUIAdd { get; set; }

        public static Action<PlayerInput> OnPlayerConnectionRemove { get; set; }

        public static Action<int, float> OnFuelChanged { get; set; }

        public static Action<string> OnSceneChange { get; set; }

        public static Action OnBeginAnnoucement { get; set; }

        public static Action<int, bool> OnCanvasStateChanged { get; set; }

        public static Action OnShowIntructions { get; set; }

        public static Action OnPauseGame { get; set; }

        public static Action<string, int, string, string> OnShowInputControls { get; set; }

        public static Action OnHideInputControls { get; set; }

        public static Action<string, string, float, bool, string> OnTutorialUIPopupShow { get; set; }

        public static Func<string[]> OnGetInputTypes { get; set; }

        public static Func<string, string, Sprite> OnGetControlSprite { get; set; }

        public static void PlayerConnectionUIAdd(PlayerInput input, string deviceName)
        {
            OnPlayerConnectionUIAdd?.Invoke(input, deviceName);
        }

        public static void PlayerConnectionRemove(PlayerInput input)
        {
            OnPlayerConnectionRemove?.Invoke(input);
        }

        public static void AddSpecificDevice(InputDevice device, bool bIsPlayerJoinScene)
        {
            OnAddSpecificDevice?.Invoke(device, bIsPlayerJoinScene);
        }

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

        public static void SceneChange(string scene)
        {
            OnSceneChange?.Invoke(scene);
        }

        public static void BeginAnnoucement()
        {
            OnBeginAnnoucement?.Invoke();
        }

        public static void PauseGame()
        {
            OnPauseGame?.Invoke();
        }

        public static void ShowInputControls(string controlName, int numPlayers, string input1Name, string input2Name)
        {
            OnShowInputControls?.Invoke(controlName, numPlayers, input1Name, input2Name);
        }

        public static void TutorialUIPopup(string title, string text, float duration, bool bShowInstruction = false, string controlsTitle = "")
        {
            OnTutorialUIPopupShow?.Invoke(title, text, duration, bShowInstruction, controlsTitle);
        }

        public static void HideInputControls()
        {
            OnHideInputControls?.Invoke();
        }

        public static Sprite GetControlSprite(string controlType, string inputName)
        {
            if (OnGetControlSprite != null)
            {
                return OnGetControlSprite(controlType, inputName);
            }

            return null;
        }

        /// <summary>
        /// for each player connected, gather the name of the input device they are using.
        /// access this by calling the appropriate method on switch manager.
        /// </summary>
        /// <returns>the name of each players input device.</returns>
        public static string[] GetInputTypes()
        {
            if (OnGetInputTypes != null)
            {
                return OnGetInputTypes();
            }

            return null;
        }
    }
}
