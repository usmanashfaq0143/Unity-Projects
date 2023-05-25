namespace Mono.Cecil.PE
{
	internal enum TextSegment
	{
		ImportAddressTable = 0,
		CLIHeader = 1,
		Code = 2,
		Resources = 3,
		Data = 4,
		StrongNameSignature = 5,
		MetadataHeader = 6,
		TableHeap = 7,
		StringHeap = 8,
		UserStringHeap = 9,
		GuidHeap = 10,
		BlobHeap = 11,
		PdbHeap = 12,
		DebugDirectory = 13,
		ImportDirectory = 14,
		ImportHintNameTable = 15,
		StartupStub = 16
	}
}
