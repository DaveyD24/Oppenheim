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
    private Image greyIcon;
    [SerializeField]
    private Sprite monkeyIcon, soldierIcon, carIcon, batIcon;

    //Get the player cooldowns
    public float cooldown = 5f;
    bool isCooldown = false;

    //Get the player 
    //PlayerControllerScript currentPlayer

    private void Start() 
    {
        fuelBar = GetComponent<Image>();
        //currentPlayer = FindObjectOfType<PlayerControllerScript>();
        iconImg.sprite = monkeyIcon;


    }

    private void Update()
    {
        //currentFuel = currentPlayer.fuel;
        currentFuel = 50f;
        fuelBar.fillAmount = currentFuel / maxFuel;
        playerSkill();
        /*
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            iconImg.sprite = monkeyIcon;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)
        {
            iconImg.sprite = soldierIcon;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)
        {
            iconImg.sprite = carIcon;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            iconImg.sprite = batIcon;
        }*/
    }

    private void playerSkill(){
        if (Input.GetKeyDown(KeyCode.Space) && isCooldown == false){
            isCooldown = true;
            greyIcon.fillAmount = 1;
        }

        if (isCooldown){
            greyIcon.fillAmount -= 1 / cooldown * Time.deltaTime;
        }

        if (greyIcon.fillAmount <= 0){
            greyIcon.fillAmount = 0;
            isCooldown = false;
        }
    }
}
