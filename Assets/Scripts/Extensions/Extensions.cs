
namespace UnityEngine.Extensions
{
	public static class XVector
	{
		public static float SquareDistance(Vector3 V1, Vector3 V2)
		{
			float X = V1.x - V2.x;
			float Y = V1.y - V2.y;
			float Z = V1.z - V2.z;

			return X * X + Y * Y + Z * Z;
		}
	}

	public static class XGameObject
	{
		/// <summary>Gets or Adds <typeparamref name="T"/> to <paramref name="Object"/>.</summary>
		/// <typeparam name="T">The <see cref="Component"/> to Get or Add to <paramref name="Object"/>.</typeparam>
		/// <param name="Object">The <see cref="GameObject"/> to get or add <typeparamref name="T"/>.</param>
		/// <returns>The <typeparamref name="T"/> attached to <paramref name="Object"/>.</returns>
		public static T GetOrAddComponent<T>(this GameObject Object) where T : Component
		{
			if (Object.TryGetComponent(out T Component))
			{
				return Component;
			}

			return Object.AddComponent<T>();
		}
	}
}
