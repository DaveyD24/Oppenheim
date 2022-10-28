using System.Collections;
using System.Collections.Generic;
using EventSystem;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Holds where the players respawn after death.
/// Only ever one may exist per each save section.
/// </summary>
public class Checkpoint : MonoBehaviour, IDataInterface
{
    private List<int> seenId = new List<int>();
    private float flagUpAmount;
    private Transform flagTransform;

    [SerializeField] private float animationDuration;
    [SerializeField] private AnimationCurve movementSpeedCurve;
    [SerializeField] private float minY;
    [SerializeField] private float maxY;
    [SerializeField] private ParticleSystem activateParticles;

    [SerializeField] private UnityEvent SaveSectionsData;
    [SerializeField] public GatherStageObjects[] ResetSectionsFuelData;

    private Tween flagMoveTween;
    private bool bCheckpointActivated = false;
    private AudioController audio; 

    public static Vector3 RespawnPosition { get; private set; }

    public static bool BUseCheckpointPos { get; private set; }

    private void Start()
    {
        flagTransform = transform.GetChild(0);
        BUseCheckpointPos = false; // as its just loading in and this is static reset it as a new level has loaded in
        audio = gameObject.GetComponent<AudioController>();

        // flagUpAmount = (maxY - minY) / 4;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerController playerController = other.gameObject.transform.root.gameObject.GetComponent<PlayerController>();
            int playerId = playerController.PlayerIdSO.PlayerID;
            if (playerController.IsActive() && !seenId.Contains(playerId))
            {
                seenId.Add(playerId);
                UpdateFlag();
            }
        }
    }

    private void UpdateFlag()
    {
        Vector3 flagPos;
        if (flagMoveTween != null)
        {
            flagPos = flagMoveTween.EndPos;
        }
        else
        {
            flagPos = flagTransform.localPosition;
        }

        // determine rest position of the flag
        int numActivePlayers = GameEvents.GetNumberPlayersActive();
        flagUpAmount = (maxY - minY) / numActivePlayers;
        flagPos.y = minY + (flagUpAmount * seenId.Count);

        flagPos.y = Mathf.Clamp(flagPos.y, minY, maxY);

        flagMoveTween = new Tween(flagTransform.localPosition, flagPos, Time.time, animationDuration);

        // when all active players have collided with this checkpoint set it as the active checkpoint
        if (seenId.Count >= numActivePlayers)
        {
            bCheckpointActivated = true;
            RespawnPosition = transform.position;
            activateParticles.Play();
            if (!BUseCheckpointPos)
            {
                BUseCheckpointPos = true;
            }

            SaveSectionsData?.Invoke();
            GameEvents.RespawnPlayersOnly(true);

            audio.Play("Activated", EAudioPlayOptions.AtTransformPosition | EAudioPlayOptions.DestroyOnEnd);

            // find all fuel which has been collected on sections that do not get saved so that these fuel values do not get saved along with it
            // this ensures that when a player dies and the section resets they do not have abilities gathered which should have been reset
            int[] fuelDataReset = new int[4];
            foreach (GatherStageObjects item in ResetSectionsFuelData)
            {
                int[] gatheredItems = item.NumberInvalidSaveCollectibles();
                for (int i = 0; i < gatheredItems.Length; i++)
                {
                    fuelDataReset[i] = gatheredItems[i];
                }
            }

            GameEvents.SavePlayerData(fuelDataReset);

            // save to analytics every time a checkpoint is fully activated
#if !UNITY_EDITOR
		Dictionary<string, object> eventData = new Dictionary<string, object>();
		eventData.Add("Position", transform.position.ToString());
		AnalyticsService.Instance.CustomData("CheckpointActivated", eventData);
		AnalyticsService.Instance.Flush();
#endif
        }
    }

    private void Update()
    {
        if (flagMoveTween != null)
        {
            if (flagMoveTween.IsComplete())
            {
                flagMoveTween = null;
            }
            else
            {
                flagTransform.localPosition = flagMoveTween.UpdatePositionCurve(movementSpeedCurve);
            }
        }
    }

#pragma warning disable SA1202 // Elements should be ordered by access
    public void LoadData(SectionData data)
#pragma warning restore SA1202 // Elements should be ordered by access
    {
        if (data.BIsCheckpointComplete)
        {
            flagTransform.localPosition = new Vector3(flagTransform.localPosition.x, maxY, flagTransform.localPosition.z);
        }
        else
        {
            // as not fully active, reset it completely
            flagTransform.localPosition = new Vector3(flagTransform.localPosition.x, minY, flagTransform.localPosition.z);
            seenId.Clear();
            flagMoveTween = null;
            // flagUpAmount = (maxY - minY) / 4;
        }
    }

    public void SaveData(SectionData data)
    {
        data.BIsCheckpointComplete = bCheckpointActivated;
    }
}
