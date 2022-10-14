

Shader "Opp/Water"
{
	Properties
	{
		_ColourHigh("Colour High", Color) = (0, 0, 1, 1)
		_ColourLow("Colour Low", Color) = (0, 0, 1, 1)
		_WaveStrength("Wave Strength", Range(0,2)) = 1
		_Speed("Speed", Range(.001, 10)) = 10
	}

		SubShader
		{
		
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}

		// Transparent.
		Blend SrcAlpha OneMinusSrcAlpha

		// Enables anything 'behind' this Water surface to be rendered as if this
		// Water isn't there.
		ZWrite off

		Pass {

		Cull Off // Don't ignore pixels behind other pixels (Transparancy).

		CGPROGRAM

		#pragma vertex WVertex
		#pragma fragment WFragment

		#include "UnityCG.cginc"

		float4 _ColourHigh;
		float4 _ColourLow;
		float _WaveStrength;
		float _HeightStrength;
		float _Speed;
		sampler2D _Height;

		struct VInput
		{
			// InPosition of the vertex.
			float4 Vertex : POSITION;
			float4 UV : TEXCOORD0;
			float3 Normal : NORMAL;
		};

		struct VOutput
		{
			// OutPosition of the vertex after computations.
			float4 Position : SV_POSITION;
			float4 UV : TEXCOORD0;
			float3 ViewDirection : NORMAL;
		};

		float3 wglnoise_mod289(float3 x)
		{
			return x - floor(x / 289) * 289;
		}

		float4 wglnoise_mod289(float4 x)
		{
			return x - floor(x / 289) * 289;
		}

		float4 wglnoise_permute(float4 x)
		{
			return wglnoise_mod289((x * 34 + 1) * x);
		}

		float4 SimplexNoiseGrad(float3 v)
		{
			// First corner
			float3 i = floor(v + dot(v, 1.0 / 3));
			float3 x0 = v - i + dot(i, 1.0 / 6);

			// Other corners
			float3 g = x0.yzx <= x0.xyz;
			float3 l = 1 - g;
			float3 i1 = min(g.xyz, l.zxy);
			float3 i2 = max(g.xyz, l.zxy);

			float3 x1 = x0 - i1 + 1.0 / 6;
			float3 x2 = x0 - i2 + 1.0 / 3;
			float3 x3 = x0 - 0.5;

			// Permutations
			i = wglnoise_mod289(i); // Avoid truncation effects in permutation
			float4 p = wglnoise_permute(i.z + float4(0, i1.z, i2.z, 1));
			p = wglnoise_permute(p + i.y + float4(0, i1.y, i2.y, 1));
			p = wglnoise_permute(p + i.x + float4(0, i1.x, i2.x, 1));

			// Gradients: 7x7 points over a square, mapped onto an octahedron.
			// The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
			float4 gx = lerp(-1, 1, frac(floor(p / 7) / 7));
			float4 gy = lerp(-1, 1, frac(floor(p % 7) / 7));
			float4 gz = 1 - abs(gx) - abs(gy);

			bool4 zn = gz < -0.01;
			gx += zn * (gx < -0.01 ? 1 : -1);
			gy += zn * (gy < -0.01 ? 1 : -1);

			float3 g0 = normalize(float3(gx.x, gy.x, gz.x));
			float3 g1 = normalize(float3(gx.y, gy.y, gz.y));
			float3 g2 = normalize(float3(gx.z, gy.z, gz.z));
			float3 g3 = normalize(float3(gx.w, gy.w, gz.w));

			// Compute noise and gradient at P
			float4 m = float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3));
			float4 px = float4(dot(g0, x0), dot(g1, x1), dot(g2, x2), dot(g3, x3));

			m = max(0.5 - m, 0);
			float4 m3 = m * m * m;
			float4 m4 = m * m3;

			float4 temp = -8 * m3 * px;
			float3 grad = m4.x * g0 + temp.x * x0 +
				m4.y * g1 + temp.y * x1 +
				m4.z * g2 + temp.z * x2 +
				m4.w * g3 + temp.w * x3;

			return 107 * float4(grad, dot(m4, px));
		}

		float SimplexNoise(float3 v)
		{
			return SimplexNoiseGrad(v).w;
		}
		

		VOutput WVertex(VInput IN)
		{
			VOutput O;

			float4 WorldPosition = mul(unity_ObjectToWorld, IN.Vertex);
			float HeightAtPosition = SimplexNoiseGrad(WorldPosition.xyz);
			float YCosine = cos(WorldPosition.y);
			float WaveDisplacement = (YCosine + cos(WorldPosition.x + _Speed * _Time.y));
			WorldPosition.y += WaveDisplacement * _WaveStrength * HeightAtPosition;

			O.Position = mul(UNITY_MATRIX_VP, WorldPosition);
			O.UV = IN.UV;
			O.ViewDirection = mul(unity_ObjectToWorld, IN.Normal); // World Normals.

			return O;
		}

		float4 WFragment(VOutput IN) : COLOR
		{
			return lerp(_ColourLow, _ColourHigh, .5);
		}

		ENDCG
		}
	}
}