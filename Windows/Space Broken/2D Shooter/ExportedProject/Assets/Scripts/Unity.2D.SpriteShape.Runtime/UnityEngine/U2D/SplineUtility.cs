namespace UnityEngine.U2D
{
	public class SplineUtility
	{
		public static float SlopeAngle(Vector2 start, Vector2 end)
		{
			Vector2 lhs = start - end;
			lhs.Normalize();
			Vector2 rhs = new Vector2(0f, 1f);
			float num = Vector2.Dot(rhs: new Vector2(1f, 0f), lhs: lhs);
			float num2 = Vector2.Dot(lhs, rhs);
			float num3 = Mathf.Acos(num2);
			float num4 = ((num >= 0f) ? 1f : (-1f));
			float num5 = num3 * 57.29578f * num4;
			num5 = ((num2 != 1f) ? num5 : 0f);
			return (num2 != -1f) ? num5 : (-180f);
		}

		public static void CalculateTangents(Vector3 point, Vector3 prevPoint, Vector3 nextPoint, Vector3 forward, float scale, out Vector3 rightTangent, out Vector3 leftTangent)
		{
			Vector3 normalized = (prevPoint - point).normalized;
			Vector3 normalized2 = (nextPoint - point).normalized;
			Vector3 rhs = normalized + normalized2;
			Vector3 lhs = forward;
			if (prevPoint != nextPoint)
			{
				if (Mathf.Abs(normalized.x * normalized2.y - normalized.y * normalized2.x + normalized.x * normalized2.z - normalized.z * normalized2.x + normalized.y * normalized2.z - normalized.z * normalized2.y) < 0.01f)
				{
					rightTangent = normalized2 * scale;
					leftTangent = normalized * scale;
					return;
				}
				lhs = Vector3.Cross(normalized, normalized2);
			}
			rightTangent = Vector3.Cross(lhs, rhs).normalized * scale;
			leftTangent = -rightTangent;
		}

		internal static int NextIndex(int index, int pointCount)
		{
			return Mod(index + 1, pointCount);
		}

		internal static int PreviousIndex(int index, int pointCount)
		{
			return Mod(index - 1, pointCount);
		}

		private static int Mod(int x, int m)
		{
			int num = x % m;
			if (num >= 0)
			{
				return num;
			}
			return num + m;
		}
	}
}
