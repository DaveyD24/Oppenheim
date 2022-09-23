
#if UNITY_EDITOR
//#define MICHAEL
#endif

using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Extensions;
using static global::BatMathematics;

public class ViewportSplit : MonoBehaviour
{
	static Dictionary<PlayerController, SpringArm> AdditionalCameras;
	static Queue<SpringArm> AddedCameraQueue; // Probably not needed if we're having maximum two Players.
	static SpringArm MainSpringArm;
	static ViewportSplit Viewport;

	[SerializeField] SwitchManager SwitchManager;
	[SerializeField] float TooFarThreshold = 20f;
	[SerializeField] Vector2 MinMaxCameraDistance;

	static Transform Average;

	public static ViewportSplit Get()
	{
		return viewport;
	}

	public static Vector3 GetAveragePosition(PlayerController[] players)
	{
		Vector3 positions = Vector3.zero;
		for (int i = 0; i < 4; ++i)
		{
			Viewport = this;

			Average = new GameObject("Average Position Marker").transform;
			AdditionalCameras = new Dictionary<PlayerController, SpringArm>();
			AddedCameraQueue = new Queue<SpringArm>();
		}
		else
		{
			Debug.LogError($"Ensure there is only one {nameof(ViewportSplit)} in the game!");
		}

		switchManager = gameObject.GetComponent<SwitchManager>();
	}

	private void Update()
	{
#if UNITY_EDITOR
		if (ArePlayersTooFarApart(out HashSet<PlayerController> TooFar))
		{
			if (TooFar.Count != 0)
			{
				StringBuilder Debug_TooFar = new StringBuilder();
				Debug_TooFar.Append("These players are too far: ");
				foreach (PlayerController PC in TooFar)
					Debug_TooFar.Append(PC.name + " ");
				Debug.Log(Debug_TooFar.ToString());
				//Debug.Break();
			}
		}
#endif

		if (Average)
		{
			SetCameraPositions();
		}
	}

	public static ViewportSplit Get()
	{
		return Viewport;
	}

	public static void SetCameraPositions()
	{
		if (ArePlayersTooFarApart(out HashSet<PlayerController> TooFar))
		{
			List<PlayerController> IgnoredPlayers = new List<PlayerController>();
			List<PlayerController> ExcludingIgnored = new List<PlayerController>();

			Get().SwitchManager.GetAllPlayers(out PlayerController[] All);

			// LINQ expressions.
			IgnoredPlayers.AddRange(All.Where(PC => TooFar.Contains(PC)));    // SELECT PlayerController WHERE PlayerController EXISTS IN TooFar
			ExcludingIgnored.AddRange(All.Where(PC => !TooFar.Contains(PC))); // SELECT PlayerController WHERE PlayerController NOT EXISTS IN TooFar

			foreach (PlayerController Player in IgnoredPlayers)
				MakeNewSpringArm(Player);

			if (ExcludingIgnored.Count != 0)
				Average.position = GetAveragePosition(ExcludingIgnored.ToArray());
		}
		else
		{
			// TODO: Interp from Split to Single Screen.
			MainSpringArm.CameraComponent.rect = new Rect(0, 0, 1, 1);
			if (AddedCameraQueue.Count != 0)
			{
				/* -- This part probably isn't needed if we're having maximum two Players. -- */

				SpringArm ToRemove = AddedCameraQueue.Dequeue();
				AdditionalCameras.Clear();
				Destroy(ToRemove.gameObject);
			}

			Vector3 AveragePosition = GetAveragePosition();

			// Check for NaN for the start of the Game when there is no
			// Active Player because division by zero.
			if (!DiagnosticCheckNaN(AveragePosition))
			{
				Average.position = AveragePosition;
				MainSpringArm.Target = Average;
			}
		}
	}

	public static void MakeNewSpringArm(PlayerController Target)
	{
		/**
		 --        I don't have another controller to test this.        --
		 --                         - Michael                           --
		 --                                                             --
		 --        I think it *should* work for 2 Players ONLY.         --
		 **/

		// We already have a Spring Arm tracking Target, so don't add another.
		if (AdditionalCameras.ContainsKey(Target))
			return;

		// Make a new empty GameObject.
		GameObject NewSpringArm = new GameObject($"Spring Arm Targeting: {Target.name}");

		// Add a Spring Arm component with the same settings as the Main Camera.
		Camera CameraComponent = NewSpringArm.GetOrAddComponent<Camera>();
		SpringArm Component = NewSpringArm.GetOrAddComponent<SpringArm>();
		SpringArmSettings Settings = MainSpringArm.GetSettings();
		Component.SetSettings(Settings, Target.transform, CameraComponent);

		// Split the screen vertically. P1 (Main) on Left. P2 (New) on Right.
		Rect SplitScreenP1 = new Rect(0f, 0f, .5f, 1f);
		Rect SplitScreenP2 = new Rect(.5f, 0f, .5f, 1f);

		// TODO: Interp the transition from Single to Split Screen.

		MainSpringArm.CameraComponent.rect = SplitScreenP1;
		CameraComponent.rect = SplitScreenP2;

		AdditionalCameras.Add(Target, Component);
		AddedCameraQueue.Enqueue(Component);
	}

	/// <summary>Average position of ALL players.</summary>
	public static Vector3 GetAveragePosition()
	{
		List<PlayerController> Active = Get().SwitchManager.GetActivePlayers();
		return GetAveragePosition(Active.ToArray());
	}

	/// <summary>Average position of certain players.</summary>
	/// <param name="Players">The players to consider.</param>
	public static Vector3 GetAveragePosition(PlayerController[] Players)
	{
		Vector3 Positions = Vector3.zero;
		for (int i = 0; i < Players.Length; ++i)
			Positions += Players[i].transform.position;

		return Positions / (float)Players.Length;
	}

	public static bool ArePlayersTooFarApart(out HashSet<PlayerController> PlayersTooFar)
	{
		PlayersTooFar = new HashSet<PlayerController>();
#if !MICHAEL
		List<PlayerController> ActivePlayers = Get().SwitchManager.GetActivePlayers();

		Vector3 Mean = GetAveragePosition(ActivePlayers.ToArray());
		float T = Get().TooFarThreshold;
		float T2 = T * T;

		for (int i = 0; i < ActivePlayers.Count; ++i)
			if (Mean.SquareDistance(ActivePlayers[i].transform.position) > T2)
				PlayersTooFar.Add(ActivePlayers[i]);
#else // MICHAEL
		Get().SwitchManager.GetAllPlayers(out PlayerController[] AllPlayers);

		Vector3 Mean = GetAveragePosition(AllPlayers);
		float T = Get().TooFarThreshold;
		float T2 = T * T;

		for (int i = 0; i < AllPlayers.Length; ++i)
			if (Mean.SquareDistance(AllPlayers[i].transform.position) > T2)
				PlayersTooFar.Add(AllPlayers[i]);
#endif // !MICHAEL
		return PlayersTooFar.Count != 0;
	}

	public static void SetMainSpringArm(SpringArm InMain)
	{
		if (!MainSpringArm)
			MainSpringArm = InMain;
	}

#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		if (Average)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawSphere(Average.position, .5f);
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(Average.position, TooFarThreshold);
		}
	}
#endif
}
