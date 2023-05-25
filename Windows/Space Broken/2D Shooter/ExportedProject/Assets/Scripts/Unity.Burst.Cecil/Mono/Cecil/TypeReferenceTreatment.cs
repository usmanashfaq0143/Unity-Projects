namespace Mono.Cecil
{
	internal enum TypeReferenceTreatment
	{
		None = 0,
		SystemDelegate = 1,
		SystemAttribute = 2,
		UseProjectionInfo = 3
	}
}
