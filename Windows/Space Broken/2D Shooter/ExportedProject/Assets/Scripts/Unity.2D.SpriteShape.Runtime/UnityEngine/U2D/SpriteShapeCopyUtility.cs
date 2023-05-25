using Unity.Collections;

namespace UnityEngine.U2D
{
	internal class SpriteShapeCopyUtility<T> where T : struct
	{
		internal static void Copy(NativeSlice<T> dst, T[] src, int length)
		{
			new NativeSlice<T>(dst, 0, length).CopyFrom(src);
		}

		internal static void Copy(T[] dst, NativeSlice<T> src, int length)
		{
			new NativeSlice<T>(src, 0, length).CopyTo(dst);
		}
	}
}
