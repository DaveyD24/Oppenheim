using UnityEngine;

public static class StaticCheats
{
	static SwitchManager SwitchManager;

	static void Set() => SwitchManager ??= ViewportSplit.GetSwitchManager();

	[Exec("Teleports all players to the entry of the Seesaw in Stage 1.")]
	public static void TP_Seesaw()
	{
		Set();

		TP_All(164.81f, 22.37f, -433.025f);
	}

	[Exec("Teleports all players to the entry of the Boat in Stage 1.")]
	public static void TP_Boat()
	{
		Set();

		TP_All(164.81f, 37.04f, -382.52f);
	}

	[Exec("Teleports all players to the entry of the Forklift in Stage 1.")]
	public static void TP_Forklift()
	{
		Set();

		TP_All(164.77f, 55.25f, -308.92f);
	}

	[Exec("Teleports all active players to the Switch Puzzle at the end of Stage 1.")]
	public static void TP_SwitchPuzzle()
	{
		Set();

		TP_Active(164.77f, 69.22f, -240.42f);
	}

	[Exec("Teleports all players to Puzzle 1 in Stage 2.")]
	public static void TP_Vents1()
	{
		Set();

		TP_All(-47.512f, 140.258f, 39.596f);
	}

	[Exec("Teleports all players to Puzzle 2 in Stage 2.")]
	public static void TP_Vents2()
	{
		Set();

		TP_All(-172.205002f, 117.119003f, 41.9799995f);
	}

	[Exec("Teleports all players to Puzzle 3 in Stage 2.")]
	public static void TP_Vents3()
	{
		Set();

		TP_All(-226.037003f, 115.07f, 82.3769989f);
	}

	[Exec("Teleports all players to Puzzle 4 in Stage 2.")]
	public static void TP_Vents4()
	{
		Set();

		TP_All(-224.369003f, 115.07f, 162.742996f);
	}

	[Exec("Teleports all players to Puzzle 5 in Stage 2.")]
	public static void TP_Vents5()
	{
		Set();

		TP_All(-16.8799992f, 114.879997f, 315.679993f);
	}

	[Exec("Teleports all players to the specified X, Y, and Z Coordinates in World Space.")]
	public static void TP_All(float X, float Y, float Z)
	{
		Set();

		SwitchManager.GetAllPlayers(out PlayerController[] Players);

		Vector3 Coordinates = new Vector3(X, Y, Z);

		foreach (PlayerController T in Players)
			T.transform.position = Coordinates;
	}

	[Exec("Teleports all Active players to the specified X, Y, and Z Coordinates in World Space.")]
	public static void TP_Active(float X, float Y, float Z)
	{
		Set();

		SwitchManager.GetAllActivePlayerTransforms(out Transform[] Players);

		Vector3 Coordinates = new Vector3(X, Y, Z);

		foreach (Transform T in Players)
			T.position = Coordinates;
	}

	[Exec("Disables the invisible boundaries around Stage 1.")]
	public static void DisableBoundaries()
	{
		GameObject Boundaries = GameObject.Find("Invisible Colliders");

		if (Boundaries)
			Boundaries.SetActive(false);
	}
}
