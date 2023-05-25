using System;
using System.ComponentModel;

namespace Unity.VisualScripting
{
	[TypeIcon(typeof(FlowGraph))]
	[UnitCategory("Nesting")]
	[UnitTitle("Subgraph")]
	[RenamedFrom("Bolt.SuperUnit")]
	[RenamedFrom("Unity.VisualScripting.SuperUnit")]
	[DisplayName("Subgraph Node")]
	public sealed class SubgraphUnit : NesterUnit<FlowGraph, ScriptGraphAsset>, IGraphEventListener, IGraphElementWithData, IGraphElement, IGraphItem, INotifiedCollectionItem, IDisposable, IPrewarmable, IAotStubbable, IIdentifiable, IAnalyticsIdentifiable
	{
		public sealed class Data : IGraphElementData
		{
			public bool isListening;
		}

		public IGraphElementData CreateData()
		{
			return new Data();
		}

		public SubgraphUnit()
		{
		}

		public SubgraphUnit(ScriptGraphAsset macro)
			: base(macro)
		{
		}

		public static SubgraphUnit WithInputOutput()
		{
			SubgraphUnit subgraphUnit = new SubgraphUnit();
			subgraphUnit.nest.source = GraphSource.Embed;
			subgraphUnit.nest.embed = FlowGraph.WithInputOutput();
			return subgraphUnit;
		}

		public static SubgraphUnit WithStartUpdate()
		{
			SubgraphUnit subgraphUnit = new SubgraphUnit();
			subgraphUnit.nest.source = GraphSource.Embed;
			subgraphUnit.nest.embed = FlowGraph.WithStartUpdate();
			return subgraphUnit;
		}

		public override FlowGraph DefaultGraph()
		{
			return FlowGraph.WithInputOutput();
		}

		protected override void Definition()
		{
			isControlRoot = true;
			foreach (IUnitPortDefinition validPortDefinition in base.nest.graph.validPortDefinitions)
			{
				if (validPortDefinition is ControlInputDefinition)
				{
					ControlInputDefinition controlInputDefinition = (ControlInputDefinition)validPortDefinition;
					string key = controlInputDefinition.key;
					ControlInput(key, delegate(Flow flow)
					{
						foreach (IUnit unit in base.nest.graph.units)
						{
							if (unit is GraphInput)
							{
								GraphInput obj2 = (GraphInput)unit;
								flow.stack.EnterParentElement(this);
								return obj2.controlOutputs[key];
							}
						}
						return null;
					});
				}
				else if (validPortDefinition is ValueInputDefinition)
				{
					ValueInputDefinition obj = (ValueInputDefinition)validPortDefinition;
					string key3 = obj.key;
					Type type = obj.type;
					bool hasDefaultValue = obj.hasDefaultValue;
					object defaultValue = obj.defaultValue;
					ValueInput valueInput = ValueInput(type, key3);
					if (hasDefaultValue)
					{
						valueInput.SetDefaultValue(defaultValue);
					}
				}
				else if (validPortDefinition is ControlOutputDefinition)
				{
					string key4 = ((ControlOutputDefinition)validPortDefinition).key;
					ControlOutput(key4);
				}
				else
				{
					if (!(validPortDefinition is ValueOutputDefinition))
					{
						continue;
					}
					ValueOutputDefinition valueOutputDefinition = (ValueOutputDefinition)validPortDefinition;
					string key2 = valueOutputDefinition.key;
					Type type2 = valueOutputDefinition.type;
					ValueOutput(type2, key2, delegate(Flow flow)
					{
						flow.stack.EnterParentElement(this);
						foreach (IUnit unit2 in base.nest.graph.units)
						{
							if (unit2 is GraphOutput)
							{
								GraphOutput graphOutput = (GraphOutput)unit2;
								object value = flow.GetValue(graphOutput.valueInputs[key2]);
								flow.stack.ExitParentElement();
								return value;
							}
						}
						flow.stack.ExitParentElement();
						throw new InvalidOperationException("Missing output node when to get value.");
					});
				}
			}
		}

		public void StartListening(GraphStack stack)
		{
			if (stack.TryEnterParentElement(this))
			{
				base.nest.graph.StartListening(stack);
				stack.ExitParentElement();
			}
			stack.GetElementData<Data>(this).isListening = true;
		}

		public void StopListening(GraphStack stack)
		{
			stack.GetElementData<Data>(this).isListening = false;
			if (stack.TryEnterParentElement(this))
			{
				base.nest.graph.StopListening(stack);
				stack.ExitParentElement();
			}
		}

		public bool IsListening(GraphPointer pointer)
		{
			return pointer.GetElementData<Data>(this).isListening;
		}

		public override void AfterAdd()
		{
			base.AfterAdd();
			base.nest.beforeGraphChange += StopWatchingPortDefinitions;
			base.nest.afterGraphChange += StartWatchingPortDefinitions;
			StartWatchingPortDefinitions();
		}

		public override void BeforeRemove()
		{
			base.BeforeRemove();
			StopWatchingPortDefinitions();
			base.nest.beforeGraphChange -= StopWatchingPortDefinitions;
			base.nest.afterGraphChange -= StartWatchingPortDefinitions;
		}

		private void StopWatchingPortDefinitions()
		{
			if (base.nest.graph != null)
			{
				base.nest.graph.onPortDefinitionsChanged -= Define;
			}
		}

		private void StartWatchingPortDefinitions()
		{
			if (base.nest.graph != null)
			{
				base.nest.graph.onPortDefinitionsChanged += Define;
			}
		}
	}
}
