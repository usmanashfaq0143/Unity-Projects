using UnityEngine;

namespace Unity.VisualScripting
{
	[UnitCategory("Math/Vector 2")]
	[UnitTitle("Angle")]
	public sealed class Vector2Angle : Angle<Vector2>
	{
		public override float Operation(Vector2 a, Vector2 b)
		{
			return Vector2.Angle(a, b);
		}
	}
}
