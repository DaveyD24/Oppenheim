using System.Collections;
using System.Collections.Generic;
using EventSystem;
using UnityEngine;

/// <summary>
/// Class to handle what occurs when any player collides with any fuel object.
/// </summary>
public class FuelCollectible : UniqueID, IDataInterface
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

#pragma warning disable SA1202 // Elements should be ordered by access
    public void LoadData(SectionData data)
#pragma warning restore SA1202 // Elements should be ordered by access
    {
        throw new System.NotImplementedException();
    }

    public void SaveData(SectionData data)
    {
        data.AbilityItems.Add(SaveID);
    }
}
