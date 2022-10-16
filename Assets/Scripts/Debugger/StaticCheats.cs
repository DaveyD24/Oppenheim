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
}
