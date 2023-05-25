using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting
{
	public sealed class GraphNest<TGraph, TMacro> : IGraphNest, IAotStubbable where TGraph : class, IGraph, new() where TMacro : Macro<TGraph>
	{
		[DoNotSerialize]
		private GraphSource _source = GraphSource.Macro;

		[DoNotSerialize]
		private TMacro _macro;

		[DoNotSerialize]
		private TGraph _embed;

		[DoNotSerialize]
		public IGraphNester nester { get; set; }

		[Serialize]
		public GraphSource source
		{
			get
			{
				return _source;
			}
			set
			{
				if (value != source)
				{
					BeforeGraphChange();
					_source = value;
					AfterGraphChange();
				}
			}
		}

		[Serialize]
		public TMacro macro
		{
			get
			{
				return _macro;
			}
			set
			{
				if (!((UnityEngine.Object)value == (UnityEngine.Object)macro))
				{
					BeforeGraphChange();
					_macro = value;
					AfterGraphChange();
				}
			}
		}

		[Serialize]
		public TGraph embed
		{
			get
			{
				return _embed;
			}
			set
			{
				if (value != embed)
				{
					BeforeGraphChange();
					_embed = value;
					AfterGraphChange();
				}
			}
		}

		[DoNotSerialize]
		public TGraph graph
		{
			get
			{
				switch (source)
				{
				case GraphSource.Embed:
					return embed;
				case GraphSource.Macro:
				{
					TMacro val = macro;
					if (val == null)
					{
						return null;
					}
					return val.graph;
				}
				default:
					throw new UnexpectedEnumValueException<GraphSource>(source);
				}
			}
		}

		IMacro IGraphNest.macro
		{
			get
			{
				return macro;
			}
			set
			{
				macro = (TMacro)value;
			}
		}

		IGraph IGraphNest.embed
		{
			get
			{
				return embed;
			}
			set
			{
				embed = (TGraph)value;
			}
		}

		IGraph IGraphNest.graph => graph;

		Type IGraphNest.graphType => typeof(TGraph);

		Type IGraphNest.macroType => typeof(TMacro);

		public IEnumerable<ISerializationDependency> deserializationDependencies
		{
			get
			{
				if ((UnityEngine.Object)macro != (UnityEngine.Object)null)
				{
					yield return macro;
				}
			}
		}

		[DoNotSerialize]
		public bool hasBackgroundEmbed
		{
			get
			{
				if (source == GraphSource.Macro)
				{
					return embed != null;
				}
				return false;
			}
		}

		public event Action beforeGraphChange;

		public event Action afterGraphChange;

		public void SwitchToEmbed(TGraph embed)
		{
			if (source != 0 || this.embed != embed)
			{
				BeforeGraphChange();
				_source = GraphSource.Embed;
				_embed = embed;
				_macro = null;
				AfterGraphChange();
			}
		}

		public void SwitchToMacro(TMacro macro)
		{
			if (source != GraphSource.Macro || !((UnityEngine.Object)this.macro == (UnityEngine.Object)macro))
			{
				BeforeGraphChange();
				_source = GraphSource.Macro;
				_embed = null;
				_macro = macro;
				AfterGraphChange();
			}
		}

		private void BeforeGraphChange()
		{
			if (graph != null)
			{
				nester.UninstantiateNest();
			}
			this.beforeGraphChange?.Invoke();
		}

		private void AfterGraphChange()
		{
			this.afterGraphChange?.Invoke();
			if (graph != null)
			{
				nester.InstantiateNest();
			}
		}

		public IEnumerable<object> GetAotStubs(HashSet<object> visited)
		{
			return LinqUtility.Concat<object>(new IEnumerable[1] { graph?.GetAotStubs(visited) });
		}
	}
}
