using UnityEngine;

namespace Unity.VisualScripting
{
	[UnitCategory("Math/Vector 2")]
	[UnitTitle("Absolute")]
	public sealed class Vector2Absolute : Absolute<Vector2>
	{
		protected override Vector2 Operation(Vector2 input)
		{
			return new Vector2(Mathf.Abs(input.x), Mathf.Abs(input.y));
		}
	}
}
