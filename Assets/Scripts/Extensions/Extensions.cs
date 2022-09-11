
namespace UnityEngine.Extensions
{
	public static class Vector
	{
		public static float SquareDistance(Vector3 V1, Vector3 V2)
		{
			float X = V1.x - V2.x;
			float Y = V1.y - V2.y;
			float Z = V1.z - V2.z;

			return X * X + Y * Y + Z * Z;
		}
	}
}
