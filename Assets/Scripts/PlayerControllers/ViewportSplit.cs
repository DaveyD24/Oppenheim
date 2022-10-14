
#if UNITY_EDITOR
//#define MICHAEL_TESTING
#endif

using System.Linq;
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
	[SerializeField] float SplitPollingRate = 2f;
	float TimeOfLastSplit = -1f;

	static Transform Average;

#if MICHAEL_TESTING
	bool bFlipFlop = false;
	enum ETestType { Hold, Press };
	[SerializeField] ETestType TestType;
#endif

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

#if MICHAEL_TESTING
		/** -- Test that a delay of <see cref="TooFarThreshold"/> exists between Viewport Splits.  -- */
		/**                                --  Expected Behaviour  --                                 */
		/**          The Viewport should split every <see cref="TooFarThreshold"/> seconds.           */

		// Use (=) to trigger the test.
		if ((TestType == ETestType.Hold && Input.GetKey(KeyCode.Equals)) || (TestType == ETestType.Press && Input.GetKeyDown(KeyCode.Equals)))
		{
			// Can only test with two Players.
			if (SwitchManager.GetNumberOfPlayers() == 1)
			{
				Debug.LogError("Testing with one person?");
				return;
			}

			SwitchManager.GetAllActivePlayerTransforms(out Transform[] OutActiveTs);

			// 3+ Players are unsupported.
			Debug.Assert(OutActiveTs.Length == 2, "Viewport Split only supports 2 Active Players");

			if (Average)
			{
				// Move P2 just outside of TooFarThreshold by .5f, and then within by .5, depending on FlipFlop, each frame.
				OutActiveTs[1].position = Average.position + OutActiveTs[0].forward * (TooFarThreshold + (bFlipFlop ? .5f : -.5f));
				bFlipFlop = !bFlipFlop;
			}
		}
#endif
	}

	/// <returns>The <see langword="static"/> reference to the only <see cref="ViewportSplit"/>.</returns>
	public static ViewportSplit Get()
	{
		return Viewport;
	}

	/// <summary>Sets the camera/s position.</summary>
	/// <remarks>Determines whether the Viewport should split and/or merge, given active players.</remarks>
	public static void SetCameraPositions()
	{
		// If we need to split the Viewport.
		if (ArePlayersTooFarApart())
		{
			if (SecondSpringArm || Time.time - Get().TimeOfLastSplit < Get().SplitPollingRate)
				return;

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
			MainSpringArm.bIsAverageTracking = false;
			SetSecondaryTarget(Active[P2]); // <-- Split-Screen is done here.

			Get().TimeOfLastSplit = Time.time;
		}
		// Merge the two cameras back as one Viewport.
		else
		{
			// TODO: Interp from Split to Single Screen.
			MainSpringArm.CameraComponent.rect = new Rect(0, 0, 1, 1);

			RemoveSecondaryCamera();

			SwitchManager Multiplayer = Get().SwitchManager;
			int NumberOfPlayers = Multiplayer.GetNumberOfPlayers();
			MainSpringArm.bIsAverageTracking = Multiplayer.GetNumberOfPlayers() > 1;

			Multiplayer.GetAllPlayers(out PlayerController[] All);
			foreach (PlayerController PC in All)
				PC.TrackingCamera = MainSpringArm;

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

			// Mark this Spring Arm as Secondary and do not Initialise with Start().
			SecondSpringArm.bIsSecondarySpringArm = true;

			// Instead, Initialise using the MainSpringArm's Start() initialisation fields.
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

	/// <remarks>
	/// Too far is defined as:<br></br>
	/// <code>
	/// distance(v, Player.Position) &gt; <see cref="TooFarThreshold"/>
	/// <br></br>
	/// Where v = Average Position of all Active Players.
	/// </code>
	/// I.e., A Player is too far if distance between v and the Player's position is
	/// greater than <see cref="TooFarThreshold"/>.
	/// </remarks>
	/// <returns>
	/// <see langword="true"/> if at least one Active Player is <see cref="TooFarThreshold"/>
	/// from their Average Position.
	/// </returns>
	public static bool ArePlayersTooFarApart()
	{
		if (Get().SwitchManager.GetNumberOfPlayers() <= 1)
			return false;

		List<PlayerController> ActivePlayers = Get().SwitchManager.GetActivePlayers();

		Vector3 Mean = GetAveragePosition(ActivePlayers.ToArray());
		float T = Get().TooFarThreshold;
		float T2 = T * T;

		// Use SqrDist to save the expensive Sqrt() call against a known value.
		for (int i = 0; i < ActivePlayers.Count; ++i)
			if (Mean.SquareDistance(ActivePlayers[i].transform.position) > T2)
				return true;

		return false;
	}

	/// <inheritdoc cref="ArePlayersTooFarApart"/>
	/// <param name="PlayersTooFar">Out HashSet of the Players that are considered 'Too Far'.</param>
	public static bool ArePlayersTooFarApart(out HashSet<PlayerController> PlayersTooFar)
	{
		PlayersTooFar = new HashSet<PlayerController>();

		List<PlayerController> ActivePlayers = Get().SwitchManager.GetActivePlayers();

		Vector3 Mean = GetAveragePosition(ActivePlayers.ToArray());
		float T = Get().TooFarThreshold;
		float T2 = T * T;

		// Use SqrDist to save the expensive Sqrt() call against a known value.
		for (int i = 0; i < ActivePlayers.Count; ++i)
			if (Mean.SquareDistance(ActivePlayers[i].transform.position) > T2)
				PlayersTooFar.Add(ActivePlayers[i]);

		return PlayersTooFar.Count != 0;
	}

	/// <summary>Set the <see cref="SpringArm"/> that will never be destroyed.</summary>
	/// <param name="InMain"></param>
	public static void SetMainSpringArm(SpringArm InMain)
	{
		if (!MainSpringArm)
			MainSpringArm = InMain;
	}

	/// <summary>Destroy the Secondary Camera.</summary>
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
