using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Unity.VisualScripting
{
	[UnitSurtitle("Object")]
	public sealed class IsObjectVariableDefined : IsVariableDefinedUnit, IObjectVariableUnit, IVariableUnit, IUnit, IGraphElementWithDebugData, IGraphElement, IGraphItem, INotifiedCollectionItem, IDisposable, IPrewarmable, IAotStubbable, IIdentifiable, IAnalyticsIdentifiable
	{
		[DoNotSerialize]
		[PortLabelHidden]
		[NullMeansSelf]
		public ValueInput source { get; private set; }

		public IsObjectVariableDefined()
		{
		}

		public IsObjectVariableDefined(string name)
			: base(name)
		{
		}

		protected override void Definition()
		{
			source = ValueInput<GameObject>("source", null).NullMeansSelf();
			base.Definition();
			Requirement(source, base.isDefined);
		}

		protected override VariableDeclarations GetDeclarations(Flow flow)
		{
			return Variables.Object(flow.GetValue<GameObject>(source));
		}

		[SpecialName]
		FlowGraph IUnit.get_graph()
		{
			return base.graph;
		}
	}
}
