//#define WITH_BAT_MATHEMATICS_TESTS

#if WITH_BAT_MATHEMATICS_TESTS

// --------------------------------------------------------------------------------
// AN AUTOMATED TESTING SUBSYSTEM USED FOR THE MW UNITY NAMESPACE.
// FULL SOURCE CODE AVAILABLE AT: https://github.com/WichaelMu/MW-Unity-Namespace
// --------------------------------------------------------------------------------

// --------------------------------------------------------------------------------
// THE ORIGINAL SOURCE CODE HAS BEEN MODIFIED FOR THE RELEASE OF OPPENHEIM.
// --------------------------------------------------------------------------------

using System.Diagnostics;
using UnityEngine;
using UE = UnityEngine.Debug;
using static MTest.Tolerance;
using static global::BatMathematics;

namespace MTest
{
	internal static class MTest
	{
		const int kIterations = 100;

		[Exec(true)]
		public static void ExecFullTestSuite()
		{
			Stopwatch SW = Stopwatch.StartNew();

			FArcSineTests(out int FArcSinePassed);
			StopAndRestart(SW, out long FAS);

			FAngleTests(out int FAnglePassed);
			StopAndRestart(SW, out long FA);

			FSinCosTests(out int FSCPassed);
			StopAndRestart(SW, out long FSC);

			FSqrtTests(out int FSPassed);
			StopAndRestart(SW, out long FSQ);

			FRSqrtTests(out int FRSPassed);
			StopAndRestart(SW, out long FRS);

			FInverseTests(out int FRPassed);
			StopAndRestart(SW, out long FR);

			UE.Log($"\t{GetColour(FArcSinePassed)}{nameof(FArcSineTests)}\t({FArcSinePassed}/{kIterations}) Passed.\t Completed in: {FAS} ms.</color>");
			UE.Log($"\t{GetColour(FAnglePassed)}{nameof(FAngleTests)}\t({FAnglePassed}/{kIterations}) Passed.\t Completed in: {FA} ms.</color>");
			UE.Log($"\t{GetColour(FSCPassed)}{nameof(FSinCosTests)}\t({FSCPassed}/{kIterations}) Passed.\t Completed in: {FSC} ms.</color>");
			UE.Log($"\t{GetColour(FSPassed)}{nameof(FSqrtTests)}\t({FSPassed}/{kIterations}) Passed.\t Completed in: {FSQ} ms.</color>");
			UE.Log($"\t{GetColour(FRSPassed)}{nameof(FRSqrtTests)}\t({FRSPassed}/{kIterations}) Passed.\t Completed in: {FRS} ms.</color>");
			UE.Log($"\t{GetColour(FRPassed)}{nameof(FInverseTests)}\t({FRPassed}/{kIterations}) Passed.\t Completed in: {FR} ms.</color>");
		}

		static void FArcSineTests(out int FArcSinePassed)
		{
			FArcSinePassed = 0;
			for (int i = 0; i < kIterations; ++i)
			{
				float RandomAngle = Random.Range(-720f, 720f);

				FloatToleranceCheck(i + 1, Mathf.Asin(RandomAngle), FArcSine(RandomAngle), "Fast Arc Sine", ref FArcSinePassed);
			}
		}

		static void FAngleTests(out int FAnglePassed)
		{
			FAnglePassed = 0;
			for (int i = 0; i < kIterations; ++i)
			{
				Vector3 RandomVectorA = Random.onUnitSphere;
				Vector3 RandomVectorB = Random.onUnitSphere;

				FloatToleranceCheck(i + 1, Vector3.Angle(RandomVectorA, RandomVectorB), FAngle(RandomVectorA, RandomVectorB), "Fast V-Angle", ref FAnglePassed);
			}
		}

		static void FSinCosTests(out int FSCPassed)
		{
			FSCPassed = 0;
			for (int i = 0; i < kIterations; ++i)
			{
				float RandomAngle = Random.Range(-720f, 720f);

				SinCos(out float S, out float C, RandomAngle);

				if (S >= 1F)
				{
					S = 2F - S;
				}

				if (C >= 1F)
				{
					C = 2F - C;
				}

				FloatToleranceCheck(i + 1, Mathf.Sin(RandomAngle), S, "Fast Sine", ref FSCPassed);
				FloatToleranceCheck(i + 1, Mathf.Abs(Mathf.Cos(RandomAngle)), C, "Fast Cosine", ref FSCPassed);
			}

			FSCPassed /= 2;
		}

		static void FSqrtTests(out int FSPassed)
		{
			FSPassed = 0;
			for (int i = 0; i < kIterations; ++i)
			{
				float RandomFloat = Random.Range(0f, 99375843.2938753f);

				FloatToleranceCheck(i + 1, Mathf.Sqrt(RandomFloat), FSqrt(RandomFloat, 2), "Fast Square Root", ref FSPassed);
			}
		}

		static void FRSqrtTests(out int FRSPassed)
		{
			FRSPassed = 0;
			for (int i = 0; i < kIterations; ++i)
			{
				float RandomFloat = Random.Range(0f, 99375843.2938753f);

				FloatToleranceCheck(i + 1, 1f / Mathf.Sqrt(RandomFloat), FInverseSqrt(RandomFloat, 2), "Fast Inverse Square Root", ref FRSPassed);
			}
		}

		static void FInverseTests(out int FRPassed)
		{
			FRPassed = 0;
			for (int i = 0; i < kIterations; ++i)
			{
				float RandomFloat = Random.Range(-99375843.2938753f, 99375843.2938753f);

				FloatToleranceCheck(i + 1, 1f / RandomFloat, FInverse(RandomFloat), "Fast Inverse Tests", ref FRPassed);
			}
		}

		static void StopAndRestart(Stopwatch SW, out long Time)
		{
			SW.Stop();
			Time = SW.ElapsedMilliseconds;
			SW.Restart();
		}

		static string GetColour(int Passed)
		{
			return Passed == kIterations ? "<color=#00FF00>" : "<color=#FF4444>";
		}
	}
}
#endif