using System;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting
{
	[UnitSurtitle("Scene")]
	public sealed class SetSceneVariable : SetVariableUnit, ISceneVariableUnit, IVariableUnit, IUnit, IGraphElementWithDebugData, IGraphElement, IGraphItem, INotifiedCollectionItem, IDisposable, IPrewarmable, IAotStubbable, IIdentifiable, IAnalyticsIdentifiable
	{
		public SetSceneVariable()
		{
		}

		public SetSceneVariable(string defaultName)
			: base(defaultName)
		{
		}

		protected override VariableDeclarations GetDeclarations(Flow flow)
		{
			Scene? scene = flow.stack.scene;
			if (!scene.HasValue)
			{
				return null;
			}
			return Variables.Scene(scene.Value);
		}

		[SpecialName]
		FlowGraph IUnit.get_graph()
		{
			return base.graph;
		}
	}
}
