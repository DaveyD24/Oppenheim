using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Extensions;

public class ViewportSplit : MonoBehaviour
{
	private static ViewportSplit viewport;

	private SwitchManager switchManager;
	[SerializeField] private float tooFarThreshold = 50f;

	public static ViewportSplit Get()
	{
		return viewport;
	}

	public static Vector3 GetAveragePosition(PlayerController[] players)
	{
		Vector3 positions = Vector3.zero;
		for (int i = 0; i < 4; ++i)
		{
			positions += players[i].transform.position;
		}

		return positions * .25f;
	}

	public static bool ArePlayersTooFarApart(out List<PlayerController> playersTooFar)
	{
		Get().switchManager.GetPlayers(out PlayerController[] allPlayers);
		playersTooFar = new List<PlayerController>();

		Vector3 mean = GetAveragePosition(allPlayers);
		float t = Get().tooFarThreshold;
		float t2 = t * t;

		for (int i = 0; i < 4; ++i)
		{
			if (mean.SquareDistance(allPlayers[i].transform.position) > t2)
			{
				playersTooFar.Add(allPlayers[i]);
			}
		}

		return playersTooFar.Count != 0;
	}

	private void Awake()
	{
		if (!viewport)
		{
			viewport = this;
		}
		else
		{
			Debug.LogError($"Ensure there is only one {nameof(ViewportSplit)} in the game!");
		}

		switchManager = gameObject.GetComponent<SwitchManager>();
	}

	private void Update()
	{
		if (Time.frameCount % 60 != 0)
		{
			return;
		}

		if (ArePlayersTooFarApart(out List<PlayerController> tooFar))
		{
#if UNITY_EDITOR
			if (tooFar.Count != 0)
			{
				StringBuilder debug_TooFar = new StringBuilder();
				debug_TooFar.Append("These players are too far: ");
				for (int i = 0; i < tooFar.Count; i++)
				{
					debug_TooFar.Append(tooFar[i].name + " ");
				}

				Debug.Log(debug_TooFar.ToString());

				// Debug.Break();
			}
#endif
		}
	}
}
