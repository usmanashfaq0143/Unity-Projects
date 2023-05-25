using System;
using System.Runtime.CompilerServices;

namespace Unity.VisualScripting
{
	[UnitSurtitle("Save")]
	public sealed class SetSavedVariable : SetVariableUnit, ISavedVariableUnit, IVariableUnit, IUnit, IGraphElementWithDebugData, IGraphElement, IGraphItem, INotifiedCollectionItem, IDisposable, IPrewarmable, IAotStubbable, IIdentifiable, IAnalyticsIdentifiable
	{
		public SetSavedVariable()
		{
		}

		public SetSavedVariable(string defaultName)
			: base(defaultName)
		{
		}

		protected override VariableDeclarations GetDeclarations(Flow flow)
		{
			return Variables.Saved;
		}

		[SpecialName]
		FlowGraph IUnit.get_graph()
		{
			return base.graph;
		}
	}
}
