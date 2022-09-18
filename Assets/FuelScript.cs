using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FuelScript : MonoBehaviour
{
    private Image fuelBar;
    public float currentFuel;
    private float maxFuel = 100f;
    [SerializeField]
    private Image iconImg;
    [SerializeField]
    private Sprite
    //PlayerControllerScript currentPlayer

    private void Start() 
    {
        fuelBar = GetComponent<Image>();
        //currentPlayer = FindObjectOfType<PlayerControllerScript>();
        
    }

    private void Update()
    {
        //currentFuel = currentPlayer.fuel;
        currentFuel = 50f;
        fuelBar.fillAmount = currentFuel / maxFuel;
    }
}
