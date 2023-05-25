namespace Mono.Cecil.Cil
{
	public enum ImportTargetKind : byte
	{
		ImportNamespace = 1,
		ImportNamespaceInAssembly = 2,
		ImportType = 3,
		ImportXmlNamespaceWithAlias = 4,
		ImportAlias = 5,
		DefineAssemblyAlias = 6,
		DefineNamespaceAlias = 7,
		DefineNamespaceInAssemblyAlias = 8,
		DefineTypeAlias = 9
	}
}
