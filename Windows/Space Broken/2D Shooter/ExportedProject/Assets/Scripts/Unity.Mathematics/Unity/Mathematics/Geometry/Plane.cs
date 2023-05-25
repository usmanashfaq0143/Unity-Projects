using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Unity.Mathematics.Geometry
{
	[Serializable]
	[DebuggerDisplay("{Normal}, {Distance}")]
	[Unity.IL2CPP.CompilerServices.Il2CppEagerStaticClassConstruction]
	internal struct Plane
	{
		public float4 NormalAndDistance;

		public float3 Normal
		{
			get
			{
				return NormalAndDistance.xyz;
			}
			set
			{
				NormalAndDistance.xyz = value;
			}
		}

		public float Distance
		{
			get
			{
				return NormalAndDistance.w;
			}
			set
			{
				NormalAndDistance.w = value;
			}
		}

		public Plane Flipped
		{
			get
			{
				Plane result = default(Plane);
				result.NormalAndDistance = -NormalAndDistance;
				return result;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Plane(float coefficientA, float coefficientB, float coefficientC, float coefficientD)
		{
			NormalAndDistance = Normalize(new float4(coefficientA, coefficientB, coefficientC, coefficientD));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Plane(float3 normal, float distance)
		{
			NormalAndDistance = Normalize(new float4(normal, distance));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Plane(float3 normal, float3 pointInPlane)
			: this(normal, 0f - math.dot(normal, pointInPlane))
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Plane(float3 vector1InPlane, float3 vector2InPlane, float3 pointInPlane)
			: this(math.cross(vector1InPlane, vector2InPlane), pointInPlane)
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Plane CreateFromUnitNormalAndDistance(float3 unitNormal, float distance)
		{
			Plane result = default(Plane);
			result.NormalAndDistance = new float4(unitNormal, distance);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Plane CreateFromUnitNormalAndPointInPlane(float3 unitNormal, float3 pointInPlane)
		{
			Plane result = default(Plane);
			result.NormalAndDistance = new float4(unitNormal, 0f - math.dot(unitNormal, pointInPlane));
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Plane Normalize(Plane plane)
		{
			Plane result = default(Plane);
			result.NormalAndDistance = Normalize(plane.NormalAndDistance);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float4 Normalize(float4 planeCoefficients)
		{
			float num = math.rsqrt(math.lengthsq(planeCoefficients.xyz));
			Plane plane = default(Plane);
			plane.NormalAndDistance = planeCoefficients * num;
			return plane;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float SignedDistanceToPoint(float3 point)
		{
			return math.dot(NormalAndDistance, new float4(point, 1f));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float3 Projection(float3 point)
		{
			return point - Normal * SignedDistanceToPoint(point);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator float4(Plane plane)
		{
			return plane.NormalAndDistance;
		}

		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		private void CheckPlaneIsNormalized()
		{
			float num = math.lengthsq(Normal.xyz);
			if (num < 0.99800104f || num > 1.002001f)
			{
				throw new ArgumentException("Plane must be normalized. Call Plane.Normalize() to normalize plane.");
			}
		}
	}
}
