
#if UNITY_EDITOR
#define MICHAEL
#endif

using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Extensions;
using static global::BatMathematics;

public class ViewportSplit : MonoBehaviour
{
	static Queue<SpringArm> AddedCameraQueue; // Probably not needed if we're having maximum two Players.
	static SpringArm MainSpringArm;
	static SpringArm SecondSpringArm;
	static ViewportSplit Viewport;

	[SerializeField] SwitchManager SwitchManager;
	[SerializeField] float TooFarThreshold = 20f;
	[SerializeField] Vector2 MinMaxCameraDistance;

	static Transform Average;

	void Awake()
	{
		if (!Viewport)
		{
			Viewport = this;

			Average = new GameObject("Average Position Marker").transform;
			AddedCameraQueue = new Queue<SpringArm>();
		}
		else
		{
			Debug.LogError($"Ensure there is only one {nameof(ViewportSplit)} in the game!");
		}
	}

	void Update()
	{
		if (Average)
		{
			SetCameraPositions();
		}
	}

	public static ViewportSplit Get()
	{
		return Viewport;
	}

	/// <summary>Sets the camera/s position.</summary>
	/// <remarks>Determines whether the Viewport should split and/or merge, given active players.</remarks>
	public static void SetCameraPositions()
	{
		// If we need to split the Viewport.
		if (ArePlayersTooFarApart(out _))
		{
			List<PlayerController> Active = Get().SwitchManager.GetActivePlayers();

			int P1, P2, P3, P4;
			P1 = P2 = P3 = P4 = -1;

			for (int i = 0; i < Active.Count; ++i)
			{
				if (Active[i].HumanPlayerIndex == EPlayer.P1)
					P1 = i;
				else if (Active[i].HumanPlayerIndex == EPlayer.P2)
					P2 = i;
				// 3-4 Players are not supported...
				else if (Active[i].HumanPlayerIndex == EPlayer.P3)
					P3 = i;
				else if (Active[i].HumanPlayerIndex == EPlayer.P4)
					P4 = i;
			}

#if UNITY_EDITOR
			// Check if Michael is stupid.
			Debug.Assert(P2 != -1, "ViewportSplit::SetCameraPosition() -> ArePlayersTooFar(...) returned true with only 1 active player!");

			Debug.Assert(P3 == -1 && P4 == -1, "3-4 Players are not supported by Viewport Split!");
#endif

			MainSpringArm.Target = Active[P1].transform;
			SetSecondaryTarget(Active[P2]); // <-- Split-Screen is done here.
		}
		// Merge the two cameras back as one Viewport.
		else
		{
			// TODO: Interp from Split to Single Screen.
			MainSpringArm.CameraComponent.rect = new Rect(0, 0, 1, 1);
			RemoveSecondaryCamera();

			Get().SwitchManager.GetAllPlayers(out PlayerController[] All);
			foreach (PlayerController PC in All)
				PC.TrackingCamera = MainSpringArm;

			int NumberOfPlayers = Get().SwitchManager.GetNumberOfPlayers();
			if (NumberOfPlayers > 1)
			{
				Vector3 AveragePosition = GetAveragePosition();

				// Check for NaN for the start of the Game when there is no
				// Active Player because division by zero.
				if (!DiagnosticCheckNaN(AveragePosition))
				{
					Average.position = AveragePosition;
					MainSpringArm.Target = Average;
				}
			}
			else if (NumberOfPlayers == 1)
			{
				PlayerController OnlyActive = All.First(PC => PC.Active);
				MainSpringArm.Target = OnlyActive.transform;
			}
		}
	}

	/// <summary>Splits the Viewport into two.</summary>
	/// <param name="Target">The player of the new second screen.</param>
	public static void SetSecondaryTarget(PlayerController Target)
	{
		if (!SecondSpringArm)
		{
			// Make a new empty GameObject.
			GameObject NewSpringArm = new GameObject();

			// Add a Spring Arm component with the same settings as the Main Camera.
			Camera CameraComponent = NewSpringArm.GetOrAddComponent<Camera>();
			SecondSpringArm = NewSpringArm.GetOrAddComponent<SpringArm>();

			SpringArmSettings Settings = MainSpringArm.GetSettings();
			SecondSpringArm.SetSettings(Settings, Target.transform, CameraComponent);

			// Split the screen vertically. P1 (Main) on Left. P2 (New) on Right.
			Rect SplitScreenP1 = new Rect(0f, 0f, .5f, 1f);
			Rect SplitScreenP2 = new Rect(.5f, 0f, .5f, 1f);

			// TODO: Interp the transition from Single to Split Screen.

			MainSpringArm.CameraComponent.rect = SplitScreenP1;
			SecondSpringArm.CameraComponent.rect = SplitScreenP2;

			AddedCameraQueue.Enqueue(SecondSpringArm);
		}

		SecondSpringArm.name = $"Spring Arm Targeting: {Target.name}";
		SecondSpringArm.Target = Target.transform;

		Target.TrackingCamera = SecondSpringArm;
	}

	/// <summary>Average position of ACTIVE players.</summary>
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

		List<PlayerController> ActivePlayers = Get().SwitchManager.GetActivePlayers();

		Vector3 Mean = GetAveragePosition(ActivePlayers.ToArray());
		float T = Get().TooFarThreshold;
		float T2 = T * T;

		for (int i = 0; i < ActivePlayers.Count; ++i)
			if (Mean.SquareDistance(ActivePlayers[i].transform.position) > T2)
				PlayersTooFar.Add(ActivePlayers[i]);

		return PlayersTooFar.Count != 0;
	}

	public static void SetMainSpringArm(SpringArm InMain)
	{
		if (!MainSpringArm)
			MainSpringArm = InMain;
	}

	static void RemoveSecondaryCamera()
	{
		if (SecondSpringArm)
		{
			Destroy(SecondSpringArm.gameObject);
			SecondSpringArm = null;
		}
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
