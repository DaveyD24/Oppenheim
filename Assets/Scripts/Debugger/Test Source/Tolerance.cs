//#define WITH_BAT_MATHEMATICS_TESTS

#if WITH_BAT_MATHEMATICS_TESTS

// --------------------------------------------------------------------------------
// AN AUTOMATED TESTING SUBSYSTEM USED FOR THE MW UNITY NAMESPACE.
// FULL SOURCE CODE AVAILABLE AT: https://github.com/WichaelMu/MW-Unity-Namespace
// --------------------------------------------------------------------------------

// --------------------------------------------------------------------------------
// THE ORIGINAL SOURCE CODE HAS BEEN MODIFIED FOR THE RELEASE OF OPPENHEIM.
// --------------------------------------------------------------------------------

using UnityEngine;

namespace MTest
{
	internal static class Tolerance
	{
		const float kFloatingPointTolerancePercentage = .001f; // .001 = .1% error.
		const float kVectorTolerancePercentage = .001f; // .001 = .1% error.

		public static void FloatToleranceCheck(int TestNumber, float L, float R, string Operation, ref int Passed)
		{
			if (Mathf.Abs(L - R) > kFloatingPointTolerancePercentage)
			{
				Debug.LogError("<color=#FF4444>" + TestNumber + " " + L + " " + R + " " + Operation + "\tDelta: " + (L - R) + "</color>");
			}
			else
			{
				Passed++;
			}
		}

		public static void VectorToleranceCheck(int TestNumber, Vector3 M, Vector3 U, string Operation, ref int Passed)
		{
			float X = M.x - U.x;
			float Y = M.y - U.y;
			float Z = M.z - U.z;

			X = Mathf.Abs(X);
			Y = Mathf.Abs(Y);
			Z = Mathf.Abs(Z);

			bool bFailed = X > kVectorTolerancePercentage || Y > kVectorTolerancePercentage || Z > kVectorTolerancePercentage;

			if (bFailed)
			{
				Debug.LogError("color=#FF4444>"+TestNumber + " " + U.ToString("F4") + " " + M.ToString("F4") + " " + Operation+"</color>");
			}
			else
			{
				Passed++;
			}
		}
	}
}
#endif