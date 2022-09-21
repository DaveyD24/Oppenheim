using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Extensions;

public class ViewportSplit : MonoBehaviour
{
	static ViewportSplit Viewport;

	[SerializeField] SwitchManager SwitchManager;
	[SerializeField] float TooFarThreshold = 50f;

	void Awake()
	{
		if (!Viewport)
		{
			Viewport = this;
		}
		else
		{
			Debug.LogError($"Ensure there is only one {nameof(ViewportSplit)} in the game!");
		}
	}

	void Update()
	{
		if (Time.frameCount % 60 != 0)
			return;

		if (ArePlayersTooFarApart(out List<PlayerController> TooFar))
		{
#if UNITY_EDITOR
			if (TooFar.Count != 0)
			{
				StringBuilder Debug_TooFar = new StringBuilder();
				Debug_TooFar.Append("These players are too far: ");
				for (int i = 0; i < TooFar.Count; i++)
					Debug_TooFar.Append(TooFar[i].name + " ");
				Debug.Log(Debug_TooFar.ToString());
				//Debug.Break();
			}
#endif
		}
	}

	public static ViewportSplit Get()
	{
		return Viewport;
	}

	public static Vector3 GetAveragePosition(PlayerController[] Players)
	{
		Vector3 Positions = Vector3.zero;
		for (int i = 0; i < 4; ++i)
			Positions += Players[i].transform.position;

		return Positions * .25f;
	}

	public static bool ArePlayersTooFarApart(out List<PlayerController> PlayersTooFar)
	{
		Get().SwitchManager.GetPlayers(out PlayerController[] AllPlayers);
		PlayersTooFar = new List<PlayerController>();

		Vector3 Mean = GetAveragePosition(AllPlayers);
		float T = Get().TooFarThreshold;
		float T2 = T * T;

		for (int i = 0; i < 4; ++i)
			if (Mean.SquareDistance(AllPlayers[i].transform.position) > T2)
				PlayersTooFar.Add(AllPlayers[i]);

		return PlayersTooFar.Count != 0;
	}
}
