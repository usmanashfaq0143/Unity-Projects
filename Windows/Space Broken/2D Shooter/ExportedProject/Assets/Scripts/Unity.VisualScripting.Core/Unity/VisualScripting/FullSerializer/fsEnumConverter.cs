using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting.FullSerializer.Internal;

namespace Unity.VisualScripting.FullSerializer
{
	public class fsEnumConverter : fsConverter
	{
		public override bool CanProcess(Type type)
		{
			return type.Resolve().IsEnum;
		}

		public override bool RequestCycleSupport(Type storageType)
		{
			return false;
		}

		public override bool RequestInheritanceSupport(Type storageType)
		{
			return false;
		}

		public override object CreateInstance(fsData data, Type storageType)
		{
			return Enum.ToObject(storageType, (object)0);
		}

		public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType)
		{
			if (Serializer.Config.SerializeEnumsAsInteger)
			{
				serialized = new fsData(Convert.ToInt64(instance));
			}
			else if (fsPortableReflection.GetAttribute<FlagsAttribute>(storageType) != null)
			{
				long num = Convert.ToInt64(instance);
				StringBuilder stringBuilder = new StringBuilder();
				bool flag = true;
				foreach (object value in Enum.GetValues(storageType))
				{
					long num2 = Convert.ToInt64(value);
					if (num2 != 0L && (num & num2) == num2)
					{
						if (!flag)
						{
							stringBuilder.Append(",");
						}
						flag = false;
						stringBuilder.Append(value.ToString());
					}
				}
				serialized = new fsData(stringBuilder.ToString());
			}
			else
			{
				serialized = new fsData(Enum.GetName(storageType, instance));
			}
			return fsResult.Success;
		}

		public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType)
		{
			if (data.IsString)
			{
				string[] array = data.AsString.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < array.Length; i++)
				{
					string text = array[i];
					if (!ArrayContains(Enum.GetNames(storageType), text))
					{
						if (!Enum.GetValues(storageType).Cast<Enum>().SelectMany((Enum x) => from attr in x.GetAttributeOfEnumMember<RenamedFromAttribute>()
							select (x, attr.previousName))
							.ToDictionary<(Enum, string), string, Enum>(((Enum enumMember, string previousName) x) => x.previousName, ((Enum enumMember, string previousName) x) => x.enumMember)
							.TryGetValue(text, out var value))
						{
							return fsResult.Fail("Cannot find enum name " + text + " on type " + storageType);
						}
						array[i] = value.ToString();
					}
				}
				if (Enum.GetUnderlyingType(storageType) == typeof(ulong))
				{
					ulong num = 0uL;
					foreach (string value2 in array)
					{
						ulong num2 = (ulong)Convert.ChangeType(Enum.Parse(storageType, value2), typeof(ulong));
						num |= num2;
					}
					instance = Enum.ToObject(storageType, (object)num);
				}
				else
				{
					long num3 = 0L;
					foreach (string value3 in array)
					{
						long num4 = (long)Convert.ChangeType(Enum.Parse(storageType, value3), typeof(long));
						num3 |= num4;
					}
					instance = Enum.ToObject(storageType, (object)num3);
				}
				return fsResult.Success;
			}
			if (data.IsInt64)
			{
				int num5 = (int)data.AsInt64;
				instance = Enum.ToObject(storageType, (object)num5);
				return fsResult.Success;
			}
			return fsResult.Fail($"EnumConverter encountered an unknown JSON data type for {storageType}: {data.Type}");
		}

		private static bool ArrayContains<T>(T[] values, T value)
		{
			for (int i = 0; i < values.Length; i++)
			{
				if (EqualityComparer<T>.Default.Equals(values[i], value))
				{
					return true;
				}
			}
			return false;
		}
	}
}
