using System.Collections;
using System.Collections.Generic;
using EventSystem;
using UnityEngine;

/// <summary>
/// Class to handle what occurs when any player collides with any fuel object.
/// </summary>
public class FuelCollectible : MonoBehaviour
{
    [SerializeField] private PlayerIdObject playerId; // the id of the player whose fuel gets updated
    [SerializeField] private GameObject collectParticles;

    private void Start()
    {
        transform.rotation = Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            IsCollected();
        }
    }

    private void IsCollected()
    {
        GameEvents.CollectFuel(playerId.PlayerID);
        Instantiate(collectParticles, transform.position, Quaternion.identity);
        Debug.Log("Fuel Collection");

        // collection particle system and sound effect
        Destroy(gameObject);
    }
}
