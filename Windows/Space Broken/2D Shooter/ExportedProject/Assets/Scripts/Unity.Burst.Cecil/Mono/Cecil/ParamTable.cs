using Mono.Cecil.Metadata;

namespace Mono.Cecil
{
	internal sealed class ParamTable : MetadataTable<Row<ParameterAttributes, ushort, uint>>
	{
		public override void Write(TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++)
			{
				buffer.WriteUInt16((ushort)rows[i].Col1);
				buffer.WriteUInt16(rows[i].Col2);
				buffer.WriteString(rows[i].Col3);
			}
		}
	}
}
