using System.Collections;
using System.Collections.Generic;
using EventSystem;
using UnityEngine;

/// <summary>
/// Class to handle what occurs when any player collides with any fuel object.
/// </summary>
public class FuelCollectible : UniqueID, IDataInterface
{
    [SerializeField] private GameObject collectParticles;

    [field: SerializeField] public PlayerIdObject PlayerId { get; private set; } // the id of the player whose fuel gets updated

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
        GameEvents.CollectFuel(PlayerId.PlayerID);
        Instantiate(collectParticles, transform.position, Quaternion.identity);

        // collection particle system and sound effect
        gameObject.SetActive(false);
    }

#pragma warning disable SA1202 // Elements should be ordered by access
    public void LoadData(SectionData data)
#pragma warning restore SA1202 // Elements should be ordered by access
    {
        if (data.AbilityItems.Contains(SaveID))
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void SaveData(SectionData data)
    {
        if (gameObject.activeSelf)
        {
            data.AbilityItems.Add(SaveID);
        }
    }
}
