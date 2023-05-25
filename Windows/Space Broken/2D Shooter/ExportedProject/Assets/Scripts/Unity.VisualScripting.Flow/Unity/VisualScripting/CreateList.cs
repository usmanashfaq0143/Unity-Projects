using System.Collections;

namespace Unity.VisualScripting
{
	[UnitCategory("Collections/Lists")]
	[UnitOrder(-1)]
	[TypeIcon(typeof(IList))]
	public sealed class CreateList : MultiInputUnit<object>
	{
		[DoNotSerialize]
		protected override int minInputCount => 0;

		[InspectorLabel("Elements")]
		[UnitHeaderInspectable("Elements")]
		[Inspectable]
		public override int inputCount
		{
			get
			{
				return base.inputCount;
			}
			set
			{
				base.inputCount = value;
			}
		}

		[DoNotSerialize]
		[PortLabelHidden]
		public ValueOutput list { get; private set; }

		protected override void Definition()
		{
			list = ValueOutput("list", Create);
			base.Definition();
			foreach (ValueInput multiInput in base.multiInputs)
			{
				Requirement(multiInput, list);
			}
			InputsAllowNull();
		}

		public IList Create(Flow flow)
		{
			AotList aotList = new AotList();
			for (int i = 0; i < inputCount; i++)
			{
				aotList.Add(flow.GetValue<object>(base.multiInputs[i]));
			}
			return aotList;
		}
	}
}
