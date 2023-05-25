using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Unity.VisualScripting
{
	[SerializationVersion("A", new Type[] { })]
	[SpecialUnit]
	public abstract class EventUnit<TArgs> : Unit, IEventUnit, IUnit, IGraphElementWithDebugData, IGraphElement, IGraphItem, INotifiedCollectionItem, IDisposable, IPrewarmable, IAotStubbable, IIdentifiable, IAnalyticsIdentifiable, IGraphEventListener, IGraphElementWithData, IGraphEventHandler<TArgs>
	{
		public class Data : IGraphElementData
		{
			public EventHook hook;

			public Delegate handler;

			public bool isListening;

			public HashSet<Flow> activeCoroutines = new HashSet<Flow>();
		}

		[Serialize]
		[Inspectable]
		[InspectorExpandTooltip]
		public bool coroutine { get; set; }

		[DoNotSerialize]
		[PortLabelHidden]
		public ControlOutput trigger { get; private set; }

		[DoNotSerialize]
		protected abstract bool register { get; }

		public virtual IGraphElementData CreateData()
		{
			return new Data();
		}

		protected override void Definition()
		{
			isControlRoot = true;
			trigger = ControlOutput("trigger");
		}

		public virtual EventHook GetHook(GraphReference reference)
		{
			throw new InvalidImplementationException($"Missing event hook for '{this}'.");
		}

		public virtual void StartListening(GraphStack stack)
		{
			Data elementData = stack.GetElementData<Data>(this);
			if (elementData.isListening)
			{
				return;
			}
			if (register)
			{
				GraphReference reference = stack.ToReference();
				EventHook hook = GetHook(reference);
				Action<TArgs> handler = delegate(TArgs args)
				{
					Trigger(reference, args);
				};
				EventBus.Register(hook, handler);
				elementData.hook = hook;
				elementData.handler = handler;
			}
			elementData.isListening = true;
		}

		public virtual void StopListening(GraphStack stack)
		{
			Data elementData = stack.GetElementData<Data>(this);
			if (!elementData.isListening)
			{
				return;
			}
			foreach (Flow activeCoroutine in elementData.activeCoroutines)
			{
				activeCoroutine.StopCoroutine(disposeInstantly: false);
			}
			if (register)
			{
				EventBus.Unregister(elementData.hook, elementData.handler);
				elementData.handler = null;
			}
			elementData.isListening = false;
		}

		public override void Uninstantiate(GraphReference instance)
		{
			StopAllCoroutines(instance.GetElementData<Data>(this).activeCoroutines.ToHashSetPooled());
			base.Uninstantiate(instance);
		}

		private static void StopAllCoroutines(HashSet<Flow> activeCoroutines)
		{
			foreach (Flow activeCoroutine in activeCoroutines)
			{
				activeCoroutine.StopCoroutineImmediate();
			}
			activeCoroutines.Free();
		}

		public bool IsListening(GraphPointer pointer)
		{
			if (!pointer.hasData)
			{
				return false;
			}
			return pointer.GetElementData<Data>(this).isListening;
		}

		public void Trigger(GraphReference reference, TArgs args)
		{
			Flow flow = Flow.New(reference);
			if (!ShouldTrigger(flow, args))
			{
				flow.Dispose();
				return;
			}
			AssignArguments(flow, args);
			Run(flow);
		}

		protected virtual bool ShouldTrigger(Flow flow, TArgs args)
		{
			return true;
		}

		protected virtual void AssignArguments(Flow flow, TArgs args)
		{
		}

		private void Run(Flow flow)
		{
			if (flow.enableDebug)
			{
				IUnitDebugData elementDebugData = flow.stack.GetElementDebugData<IUnitDebugData>(this);
				elementDebugData.lastInvokeFrame = EditorTimeBinding.frame;
				elementDebugData.lastInvokeTime = EditorTimeBinding.time;
			}
			if (coroutine)
			{
				flow.StartCoroutine(trigger, flow.stack.GetElementData<Data>(this).activeCoroutines);
			}
			else
			{
				flow.Run(trigger);
			}
		}

		protected static bool CompareNames(Flow flow, ValueInput namePort, string calledName)
		{
			Ensure.That("calledName").IsNotNull(calledName);
			return calledName.Trim().Equals(flow.GetValue<string>(namePort)?.Trim(), StringComparison.OrdinalIgnoreCase);
		}

		[SpecialName]
		FlowGraph IUnit.get_graph()
		{
			return base.graph;
		}
	}
}
