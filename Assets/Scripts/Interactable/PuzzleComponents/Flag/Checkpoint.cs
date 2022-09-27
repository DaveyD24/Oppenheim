using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    public static Vector3 RespawnPosition { get; private set; }

    private List<int> seenId = new List<int>();
    private float flagUpAmount;
    private Transform flagTransform;

    [SerializeField] private float animationDuration;
    [SerializeField] private AnimationCurve movementSpeedCurve;
    [SerializeField] private float minY;
    [SerializeField] private float maxY;

    private Tween flagMoveTween;

    private void Start()
    {
        flagTransform = transform.GetChild(0);
        flagUpAmount = (maxY - minY) / 4;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            int playerId = other.gameObject.transform.root.gameObject.GetComponent<PlayerController>().PlayerIdSO.PlayerID;
            if (!seenId.Contains(playerId))
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

        flagPos.y += flagUpAmount;

        flagMoveTween = new Tween(flagTransform.localPosition, flagPos, Time.time, animationDuration);

        if (seenId.Count >= 4)
        {
            RespawnPosition = transform.position;

            // SceneManager.LoadScene("WinScene");
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
}
