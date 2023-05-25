using System.Collections;

namespace Unity.VisualScripting
{
	[UnitTitle("While Loop")]
	[UnitCategory("Control")]
	[UnitOrder(11)]
	public class While : LoopUnit
	{
		[DoNotSerialize]
		[PortLabelHidden]
		public ValueInput condition { get; private set; }

		protected override void Definition()
		{
			base.Definition();
			condition = ValueInput<bool>("condition");
			Requirement(condition, base.enter);
		}

		private int Start(Flow flow)
		{
			return flow.EnterLoop();
		}

		private bool CanMoveNext(Flow flow)
		{
			return flow.GetValue<bool>(condition);
		}

		protected override ControlOutput Loop(Flow flow)
		{
			int loop = Start(flow);
			GraphStack stack = flow.PreserveStack();
			while (flow.LoopIsNotBroken(loop) && CanMoveNext(flow))
			{
				flow.Invoke(base.body);
				flow.RestoreStack(stack);
			}
			flow.DisposePreservedStack(stack);
			flow.ExitLoop(loop);
			return base.exit;
		}

		protected override IEnumerator LoopCoroutine(Flow flow)
		{
			int loop = Start(flow);
			GraphStack stack = flow.PreserveStack();
			while (flow.LoopIsNotBroken(loop) && CanMoveNext(flow))
			{
				yield return base.body;
				flow.RestoreStack(stack);
			}
			flow.DisposePreservedStack(stack);
			flow.ExitLoop(loop);
			yield return base.exit;
		}
	}
}
