using UnityEngine;

namespace Unity.VisualScripting
{
	[UnitOrder(502)]
	public abstract class MoveTowards<T> : Unit
	{
		[DoNotSerialize]
		public ValueInput current { get; private set; }

		[DoNotSerialize]
		public ValueInput target { get; private set; }

		[DoNotSerialize]
		public ValueInput maxDelta { get; private set; }

		[DoNotSerialize]
		[PortLabelHidden]
		public ValueOutput result { get; private set; }

		[Serialize]
		[Inspectable]
		[UnitHeaderInspectable("Per Second")]
		[InspectorToggleLeft]
		public bool perSecond { get; set; }

		[DoNotSerialize]
		protected virtual T defaultCurrent => default(T);

		[DoNotSerialize]
		protected virtual T defaultTarget => default(T);

		protected override void Definition()
		{
			current = ValueInput("current", defaultCurrent);
			target = ValueInput("target", defaultTarget);
			maxDelta = ValueInput("maxDelta", 0f);
			result = ValueOutput("result", Operation);
			Requirement(current, result);
			Requirement(target, result);
			Requirement(maxDelta, result);
		}

		private T Operation(Flow flow)
		{
			return Operation(flow.GetValue<T>(current), flow.GetValue<T>(target), flow.GetValue<float>(maxDelta) * (perSecond ? Time.deltaTime : 1f));
		}

		public abstract T Operation(T current, T target, float maxDelta);
	}
}
