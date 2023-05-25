namespace Mono.Cecil
{
	internal enum CustomAttributeValueTreatment
	{
		None = 0,
		AllowSingle = 1,
		AllowMultiple = 2,
		VersionAttribute = 3,
		DeprecatedAttribute = 4
	}
}
