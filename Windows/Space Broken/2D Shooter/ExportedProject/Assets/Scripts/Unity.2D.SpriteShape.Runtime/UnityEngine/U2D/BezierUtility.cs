using Unity.Collections;
using Unity.Mathematics;

namespace UnityEngine.U2D
{
	public static class BezierUtility
	{
		private static Vector3[] s_TempPoints = new Vector3[3];

		public static Vector3 BezierPoint(Vector3 startRightTangent, Vector3 startPosition, Vector3 endPosition, Vector3 endLeftTangent, float t)
		{
			float num = 1f - t;
			return num * num * num * startRightTangent + 3f * num * num * t * startPosition + 3f * num * t * t * endPosition + t * t * t * endLeftTangent;
		}

		internal static float GetSpritePixelWidth(Sprite sprite)
		{
			float4 @float = new float4(sprite.pixelsPerUnit, sprite.pivot.y / sprite.textureRect.height, sprite.rect.width, sprite.rect.height);
			float4 float2 = new float4(sprite.border.x, sprite.border.y, sprite.border.z, sprite.border.w);
			float num = 1f / @float.x;
			float2 float3 = new float2(@float.z, @float.w) * num;
			float2 *= num;
			float x = float2.x;
			return float3.x - float2.z - x;
		}

		internal static float BezierLength(NativeArray<ShapeControlPoint> shapePoints, int splineDetail, ref float smallestSegment)
		{
			int num = shapePoints.Length - 1;
			float num2 = 0f;
			float num3 = splineDetail - 1;
			for (int i = 0; i < num; i++)
			{
				int index = i + 1;
				ShapeControlPoint shapeControlPoint = shapePoints[i];
				ShapeControlPoint shapeControlPoint2 = shapePoints[index];
				Vector3 position = shapeControlPoint.position;
				Vector3 position2 = shapeControlPoint2.position;
				Vector3 vector = position;
				Vector3 startRightTangent = position + shapeControlPoint.rightTangent;
				Vector3 endLeftTangent = position2 + shapeControlPoint2.leftTangent;
				for (int j = 1; j < splineDetail; j++)
				{
					float t = (float)j / num3;
					Vector3 vector2 = BezierPoint(startRightTangent, position, position2, endLeftTangent, t);
					float num4 = math.distance(vector2, vector);
					num2 += num4;
					vector = vector2;
				}
			}
			float num5 = num3 * (float)num;
			float x = num2 / (num5 * 1.08f);
			smallestSegment = math.min(x, smallestSegment);
			return num2;
		}

		internal static Vector3 ClosestPointOnCurve(Vector3 point, Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent, out float t)
		{
			Vector3 v = endPosition - startPosition;
			Vector3 v2 = startTangent - startPosition;
			Vector3 v3 = endTangent - endPosition;
			float num = 0.001f;
			if (Colinear(v2, v, num) && Colinear(v3, v, num))
			{
				return ClosestPointToSegment(point, startPosition, endPosition, out t);
			}
			float startT = 0f;
			float endT = 0.5f;
			float startT2 = 0.5f;
			float endT2 = 1f;
			SplitBezier(0.5f, startPosition, endPosition, startTangent, endTangent, out var leftStartPosition, out var leftEndPosition, out var leftStartTangent, out var leftEndTangent, out var rightStartPosition, out var rightEndPosition, out var rightStartTangent, out var rightEndTangent);
			Vector3 vector = ClosestPointOnCurveIterative(point, leftStartPosition, leftEndPosition, leftStartTangent, leftEndTangent, num, ref startT, ref endT);
			Vector3 vector2 = ClosestPointOnCurveIterative(point, rightStartPosition, rightEndPosition, rightStartTangent, rightEndTangent, num, ref startT2, ref endT2);
			if ((point - vector).sqrMagnitude < (point - vector2).sqrMagnitude)
			{
				t = startT;
				return vector;
			}
			t = startT2;
			return vector2;
		}

		internal static Vector3 ClosestPointOnCurveFast(Vector3 point, Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent, out float t)
		{
			float sqrError = 0.001f;
			float startT = 0f;
			float endT = 1f;
			Vector3 result = ClosestPointOnCurveIterative(point, startPosition, endPosition, startTangent, endTangent, sqrError, ref startT, ref endT);
			t = startT;
			return result;
		}

		private static Vector3 ClosestPointOnCurveIterative(Vector3 point, Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent, float sqrError, ref float startT, ref float endT)
		{
			while ((startPosition - endPosition).sqrMagnitude > sqrError)
			{
				Vector3 v = endPosition - startPosition;
				Vector3 v2 = startTangent - startPosition;
				Vector3 v3 = endTangent - endPosition;
				if (Colinear(v2, v, sqrError) && Colinear(v3, v, sqrError))
				{
					float t;
					Vector3 result = ClosestPointToSegment(point, startPosition, endPosition, out t);
					t *= endT - startT;
					startT += t;
					endT -= t;
					return result;
				}
				SplitBezier(0.5f, startPosition, endPosition, startTangent, endTangent, out var leftStartPosition, out var leftEndPosition, out var leftStartTangent, out var leftEndTangent, out var rightStartPosition, out var rightEndPosition, out var rightStartTangent, out var rightEndTangent);
				s_TempPoints[0] = leftStartPosition;
				s_TempPoints[1] = leftStartTangent;
				s_TempPoints[2] = leftEndTangent;
				float num = SqrDistanceToPolyLine(point, s_TempPoints);
				s_TempPoints[0] = rightEndPosition;
				s_TempPoints[1] = rightEndTangent;
				s_TempPoints[2] = rightStartTangent;
				float num2 = SqrDistanceToPolyLine(point, s_TempPoints);
				if (num < num2)
				{
					startPosition = leftStartPosition;
					endPosition = leftEndPosition;
					startTangent = leftStartTangent;
					endTangent = leftEndTangent;
					endT -= (endT - startT) * 0.5f;
				}
				else
				{
					startPosition = rightStartPosition;
					endPosition = rightEndPosition;
					startTangent = rightStartTangent;
					endTangent = rightEndTangent;
					startT += (endT - startT) * 0.5f;
				}
			}
			return endPosition;
		}

		internal static void SplitBezier(float t, Vector3 startPosition, Vector3 endPosition, Vector3 startRightTangent, Vector3 endLeftTangent, out Vector3 leftStartPosition, out Vector3 leftEndPosition, out Vector3 leftStartTangent, out Vector3 leftEndTangent, out Vector3 rightStartPosition, out Vector3 rightEndPosition, out Vector3 rightStartTangent, out Vector3 rightEndTangent)
		{
			Vector3 vector = startRightTangent - startPosition;
			Vector3 vector2 = endLeftTangent - endPosition;
			Vector3 vector3 = endLeftTangent - startRightTangent;
			Vector3 vector4 = startPosition + vector * t;
			Vector3 vector5 = endPosition + vector2 * (1f - t);
			Vector3 vector6 = startRightTangent + vector3 * t;
			Vector3 vector7 = vector4 + (vector6 - vector4) * t;
			Vector3 vector8 = vector5 + (vector6 - vector5) * (1f - t);
			Vector3 vector9 = vector8 - vector7;
			Vector3 vector10 = vector7 + vector9 * t;
			leftStartPosition = startPosition;
			leftEndPosition = vector10;
			leftStartTangent = vector4;
			leftEndTangent = vector7;
			rightStartPosition = vector10;
			rightEndPosition = endPosition;
			rightStartTangent = vector8;
			rightEndTangent = vector5;
		}

		private static Vector3 ClosestPointToSegment(Vector3 point, Vector3 segmentStart, Vector3 segmentEnd, out float t)
		{
			Vector3 lhs = point - segmentStart;
			Vector3 vector = segmentEnd - segmentStart;
			Vector3 normalized = vector.normalized;
			float magnitude = vector.magnitude;
			float num = Vector3.Dot(lhs, normalized);
			if (num <= 0f)
			{
				num = 0f;
			}
			else if (num >= magnitude)
			{
				num = magnitude;
			}
			t = num / magnitude;
			return segmentStart + vector * t;
		}

		private static float SqrDistanceToPolyLine(Vector3 point, Vector3[] points)
		{
			float num = float.MaxValue;
			for (int i = 0; i < points.Length - 1; i++)
			{
				float num2 = SqrDistanceToSegment(point, points[i], points[i + 1]);
				if (num2 < num)
				{
					num = num2;
				}
			}
			return num;
		}

		private static float SqrDistanceToSegment(Vector3 point, Vector3 segmentStart, Vector3 segmentEnd)
		{
			Vector3 lhs = point - segmentStart;
			Vector3 vector = segmentEnd - segmentStart;
			Vector3 normalized = vector.normalized;
			float magnitude = vector.magnitude;
			float num = Vector3.Dot(lhs, normalized);
			if (num <= 0f)
			{
				return (point - segmentStart).sqrMagnitude;
			}
			if (num >= magnitude)
			{
				return (point - segmentEnd).sqrMagnitude;
			}
			return Vector3.Cross(lhs, normalized).sqrMagnitude;
		}

		private static bool Colinear(Vector3 v1, Vector3 v2, float error = 0.0001f)
		{
			return Mathf.Abs(v1.x * v2.y - v1.y * v2.x + v1.x * v2.z - v1.z * v2.x + v1.y * v2.z - v1.z * v2.y) < error;
		}
	}
}
