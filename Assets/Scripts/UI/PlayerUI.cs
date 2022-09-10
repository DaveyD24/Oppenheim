using System.Collections;
using System.Collections.Generic;
using EventSystem;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The UI each player uses, being assigned to the corresponding player canvas.
/// </summary>
public class PlayerUI : MonoBehaviour
{
    [SerializeField] private PlayerIdObject assigedPlayerID;
    [SerializeField] private GameObject oppenheim;

    private Canvas canvas;

    // [SerializeField] private Slider fuelSlider;
    private void Awake()
    {
        canvas = gameObject.GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        // assign the fuel UI to be updated whever another class calls the specified Event System Action
        UIEvents.OnFuelChanged += UpdateFuelSlider;
        UIEvents.OnShowIntructions += ShowInstructions;
        UIEvents.OnCanvasStateChanged += ActivateCanvas;
    }

    private void OnDisable()
    {
        // So the system doesn't error out when no longer in use unassign the method as well
        UIEvents.OnFuelChanged -= UpdateFuelSlider;
        UIEvents.OnShowIntructions -= ShowInstructions;
        UIEvents.OnCanvasStateChanged -= ActivateCanvas;
    }

    private void UpdateFuelSlider(int playerID, float value)
    {
        if (playerID == assigedPlayerID.PlayerID)
        {
            // fuelSlider.value = value;
        }
    }

    private void ShowInstructions()
    {
        oppenheim.SetActive(true);
    }

    private void ActivateCanvas(int playerID, bool value)
    {
        if (playerID == assigedPlayerID.PlayerID)
        {
            canvas.enabled = value;
        }
    }
}
