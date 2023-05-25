using System;
using System.Runtime.CompilerServices;

namespace Unity.VisualScripting
{
	[UnitSurtitle("Application")]
	public sealed class SetApplicationVariable : SetVariableUnit, IApplicationVariableUnit, IVariableUnit, IUnit, IGraphElementWithDebugData, IGraphElement, IGraphItem, INotifiedCollectionItem, IDisposable, IPrewarmable, IAotStubbable, IIdentifiable, IAnalyticsIdentifiable
	{
		public SetApplicationVariable()
		{
		}

		public SetApplicationVariable(string defaultName)
			: base(defaultName)
		{
		}

		protected override VariableDeclarations GetDeclarations(Flow flow)
		{
			return Variables.Application;
		}

		[SpecialName]
		FlowGraph IUnit.get_graph()
		{
			return base.graph;
		}
	}
}
