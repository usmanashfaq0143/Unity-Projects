using UnityEngine;

namespace Unity.VisualScripting
{
	[AddComponentMenu("Visual Scripting/Script Machine")]
	[RequireComponent(typeof(Variables))]
	[DisableAnnotation]
	[HelpURL("https://docs.unity3d.com/Packages/com.unity.visualscripting@latest/index.html?subfolder=/manual/vs-graphs-machines-macros.html")]
	[RenamedFrom("Bolt.FlowMachine")]
	[RenamedFrom("Unity.VisualScripting.FlowMachine")]
	public sealed class ScriptMachine : EventMachine<FlowGraph, ScriptGraphAsset>
	{
		public override FlowGraph DefaultGraph()
		{
			return FlowGraph.WithStartUpdate();
		}

		protected override void OnEnable()
		{
			if (base.hasGraph)
			{
				base.graph.StartListening(base.reference);
			}
			base.OnEnable();
		}

		protected override void OnInstantiateWhileEnabled()
		{
			if (base.hasGraph)
			{
				base.graph.StartListening(base.reference);
			}
			base.OnInstantiateWhileEnabled();
		}

		protected override void OnUninstantiateWhileEnabled()
		{
			base.OnUninstantiateWhileEnabled();
			if (base.hasGraph)
			{
				base.graph.StopListening(base.reference);
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if (base.hasGraph)
			{
				base.graph.StopListening(base.reference);
			}
		}

		[ContextMenu("Show Data...")]
		protected override void ShowData()
		{
			base.ShowData();
		}
	}
}
