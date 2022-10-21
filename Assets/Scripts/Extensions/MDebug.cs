using UnityEngine;

public static class MDebug
{
	public static void GizmosDrawArrow(Vector3 Position, Vector3 Direction, float ArrowHeadLength = .25f, float ArrowHeadAngle = 20f)
	{
		Gizmos.DrawRay(Position, Direction);

		Vector3 R = Quaternion.LookRotation(Direction) * Quaternion.Euler(0, 180 + ArrowHeadAngle, 0) * Vector3.forward;
		Vector3 L = Quaternion.LookRotation(Direction) * Quaternion.Euler(0, 180 - ArrowHeadAngle, 0) * Vector3.forward;
		Gizmos.DrawRay(Position + Direction, R * ArrowHeadLength);
		Gizmos.DrawRay(Position + Direction, L * ArrowHeadLength);
	}

	public static void GizmosDrawArrow(Vector3 Position, Vector3 Direction, Color Colour, float ArrowHeadLength = .25f, float ArrowHeadAngle = 20f)
	{
		Gizmos.color = Colour;
		Gizmos.DrawRay(Position, Direction);

		Vector3 R = Quaternion.LookRotation(Direction) * Quaternion.Euler(0, 180 + ArrowHeadAngle, 0) * Vector3.forward;
		Vector3 L = Quaternion.LookRotation(Direction) * Quaternion.Euler(0, 180 - ArrowHeadAngle, 0) * Vector3.forward;
		Gizmos.DrawRay(Position + Direction, R * ArrowHeadLength);
		Gizmos.DrawRay(Position + Direction, L * ArrowHeadLength);
	}

	public static void DrawArrow(Vector3 Position, Vector3 Direction, float ArrowHeadLength = .25f, float ArrowHeadAngle = 20f)
	{
		Debug.DrawRay(Position, Direction);

		Vector3 R = Quaternion.LookRotation(Direction) * Quaternion.Euler(0, 180 + ArrowHeadAngle, 0) * Vector3.forward;
		Vector3 L = Quaternion.LookRotation(Direction) * Quaternion.Euler(0, 180 - ArrowHeadAngle, 0) * Vector3.forward;
		Debug.DrawRay(Position + Direction, R * ArrowHeadLength);
		Debug.DrawRay(Position + Direction, L * ArrowHeadLength);
	}

	public static void DrawArrow(Vector3 Position, Vector3 Direction, Color Colour, float ArrowHeadLength = .25f, float ArrowHeadAngle = 20f)
	{
		Debug.DrawRay(Position, Direction, Colour);

		Vector3 R = Quaternion.LookRotation(Direction) * Quaternion.Euler(0, 180 + ArrowHeadAngle, 0) * Vector3.forward;
		Vector3 L = Quaternion.LookRotation(Direction) * Quaternion.Euler(0, 180 - ArrowHeadAngle, 0) * Vector3.forward;
		Debug.DrawRay(Position + Direction, R * ArrowHeadLength, Colour);
		Debug.DrawRay(Position + Direction, L * ArrowHeadLength, Colour);
	}
}
