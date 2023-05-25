namespace Microsoft.Cci.Pdb
{
	internal enum CV_ptrtype
	{
		CV_PTR_BASE_SEG = 3,
		CV_PTR_BASE_VAL = 4,
		CV_PTR_BASE_SEGVAL = 5,
		CV_PTR_BASE_ADDR = 6,
		CV_PTR_BASE_SEGADDR = 7,
		CV_PTR_BASE_TYPE = 8,
		CV_PTR_BASE_SELF = 9,
		CV_PTR_NEAR32 = 10,
		CV_PTR_64 = 12,
		CV_PTR_UNUSEDPTR = 13
	}
}
