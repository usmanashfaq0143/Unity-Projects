using System.Diagnostics;
using System.Threading;
using Mono.Collections.Generic;

namespace Mono.Cecil
{
	[DebuggerDisplay("{AttributeType}")]
	public sealed class SecurityAttribute : ICustomAttribute
	{
		private TypeReference attribute_type;

		internal Collection<CustomAttributeNamedArgument> fields;

		internal Collection<CustomAttributeNamedArgument> properties;

		public TypeReference AttributeType
		{
			get
			{
				return attribute_type;
			}
			set
			{
				attribute_type = value;
			}
		}

		public bool HasFields => !fields.IsNullOrEmpty();

		public Collection<CustomAttributeNamedArgument> Fields
		{
			get
			{
				if (fields == null)
				{
					Interlocked.CompareExchange(ref fields, new Collection<CustomAttributeNamedArgument>(), null);
				}
				return fields;
			}
		}

		public bool HasProperties => !properties.IsNullOrEmpty();

		public Collection<CustomAttributeNamedArgument> Properties
		{
			get
			{
				if (properties == null)
				{
					Interlocked.CompareExchange(ref properties, new Collection<CustomAttributeNamedArgument>(), null);
				}
				return properties;
			}
		}

		public SecurityAttribute(TypeReference attributeType)
		{
			attribute_type = attributeType;
		}
	}
}
