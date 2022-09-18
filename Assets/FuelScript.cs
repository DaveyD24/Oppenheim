using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FuelScript : MonoBehaviour
{
    private Image fuelBar;
    public float currentFuel;
    private float maxFuel = 100f;
    //PlayerControllerScript player

    private void Start() 
    {
        fuelBar = GetComponent<Image>();
        //player = FindObjectOfType<PlayerControllerScript>();
        
    }

    private void Update()
    {
        //currentFuel = player.fuel;
        currentFuel = 50f;
        fuelBar.fillAmount = currentFuel / maxFuel;
    }
}
