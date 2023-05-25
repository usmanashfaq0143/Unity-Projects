using UnityEngine;

namespace Unity.VisualScripting
{
	[UnitCategory("Math/Scalar")]
	[UnitTitle("Root")]
	[UnitOrder(106)]
	public sealed class ScalarRoot : Unit
	{
		[DoNotSerialize]
		[PortLabel("x")]
		public ValueInput radicand { get; private set; }

		[DoNotSerialize]
		[PortLabel("n")]
		public ValueInput degree { get; private set; }

		[DoNotSerialize]
		[PortLabel("ⁿ√x")]
		public ValueOutput root { get; private set; }

		protected override void Definition()
		{
			radicand = ValueInput("radicand", 1f);
			degree = ValueInput("degree", 2f);
			root = ValueOutput("root", Root);
			Requirement(radicand, root);
			Requirement(degree, root);
		}

		public float Root(Flow flow)
		{
			float value = flow.GetValue<float>(degree);
			float value2 = flow.GetValue<float>(radicand);
			if (value == 2f)
			{
				return Mathf.Sqrt(value2);
			}
			return Mathf.Pow(value2, 1f / value);
		}
	}
}
