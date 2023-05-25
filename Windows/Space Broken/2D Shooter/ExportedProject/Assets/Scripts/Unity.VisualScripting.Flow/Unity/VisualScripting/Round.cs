namespace Unity.VisualScripting
{
	[UnitOrder(202)]
	public abstract class Round<TInput, TOutput> : Unit
	{
		public enum Rounding
		{
			Floor = 0,
			Ceiling = 1,
			AwayFromZero = 2
		}

		[Inspectable]
		[UnitHeaderInspectable]
		[Serialize]
		public Rounding rounding { get; set; } = Rounding.AwayFromZero;


		[DoNotSerialize]
		[PortLabelHidden]
		public ValueInput input { get; private set; }

		[DoNotSerialize]
		[PortLabelHidden]
		public ValueOutput output { get; private set; }

		protected override void Definition()
		{
			input = ValueInput<TInput>("input");
			output = ValueOutput("output", Operation).Predictable();
			Requirement(input, output);
		}

		protected abstract TOutput Floor(TInput input);

		protected abstract TOutput AwayFromZero(TInput input);

		protected abstract TOutput Ceiling(TInput input);

		public TOutput Operation(Flow flow)
		{
			return rounding switch
			{
				Rounding.Floor => Floor(flow.GetValue<TInput>(input)), 
				Rounding.AwayFromZero => AwayFromZero(flow.GetValue<TInput>(input)), 
				Rounding.Ceiling => Ceiling(flow.GetValue<TInput>(input)), 
				_ => throw new UnexpectedEnumValueException<Rounding>(rounding), 
			};
		}
	}
}
