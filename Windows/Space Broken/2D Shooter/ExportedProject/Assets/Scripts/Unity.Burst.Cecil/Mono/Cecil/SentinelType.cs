using System;
using Mono.Cecil.Metadata;

namespace Mono.Cecil
{
	public sealed class SentinelType : TypeSpecification
	{
		public override bool IsValueType
		{
			get
			{
				return false;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public override bool IsSentinel => true;

		public SentinelType(TypeReference type)
			: base(type)
		{
			Mixin.CheckType(type);
			etype = Mono.Cecil.Metadata.ElementType.Sentinel;
		}
	}
}
