using System;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

namespace Unity.Burst
{
	internal static class Unsafe
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public unsafe static T Read<T>(void* source)
		{
			return *(T*)source;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public unsafe static T ReadUnaligned<T>(void* source)
		{
			return System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(source);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public static T ReadUnaligned<T>(ref byte source)
		{
			return System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref source);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public unsafe static void Write<T>(void* destination, T value)
		{
			*(T*)destination = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public unsafe static void WriteUnaligned<T>(void* destination, T value)
		{
			System.Runtime.CompilerServices.Unsafe.WriteUnaligned(destination, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public static void WriteUnaligned<T>(ref byte destination, T value)
		{
			System.Runtime.CompilerServices.Unsafe.WriteUnaligned(ref destination, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public unsafe static void Copy<T>(void* destination, ref T source)
		{
			*(T*)destination = source;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public unsafe static void Copy<T>(ref T destination, void* source)
		{
			destination = *(T*)source;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public unsafe static void* AsPointer<T>(ref T value)
		{
			return System.Runtime.CompilerServices.Unsafe.AsPointer(ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public static int SizeOf<T>()
		{
			return System.Runtime.CompilerServices.Unsafe.SizeOf<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public unsafe static void CopyBlock(void* destination, void* source, uint byteCount)
		{
			// IL cpblk instruction
			System.Runtime.CompilerServices.Unsafe.CopyBlock(destination, source, byteCount);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public static void CopyBlock(ref byte destination, ref byte source, uint byteCount)
		{
			// IL cpblk instruction
			System.Runtime.CompilerServices.Unsafe.CopyBlock(ref destination, ref source, byteCount);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public unsafe static void CopyBlockUnaligned(void* destination, void* source, uint byteCount)
		{
			// IL cpblk instruction
			System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(destination, source, byteCount);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public static void CopyBlockUnaligned(ref byte destination, ref byte source, uint byteCount)
		{
			// IL cpblk instruction
			System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(ref destination, ref source, byteCount);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public unsafe static void InitBlock(void* startAddress, byte value, uint byteCount)
		{
			// IL initblk instruction
			System.Runtime.CompilerServices.Unsafe.InitBlock(startAddress, value, byteCount);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public static void InitBlock(ref byte startAddress, byte value, uint byteCount)
		{
			// IL initblk instruction
			System.Runtime.CompilerServices.Unsafe.InitBlock(ref startAddress, value, byteCount);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public unsafe static void InitBlockUnaligned(void* startAddress, byte value, uint byteCount)
		{
			// IL initblk instruction
			System.Runtime.CompilerServices.Unsafe.InitBlockUnaligned(startAddress, value, byteCount);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public static void InitBlockUnaligned(ref byte startAddress, byte value, uint byteCount)
		{
			// IL initblk instruction
			System.Runtime.CompilerServices.Unsafe.InitBlockUnaligned(ref startAddress, value, byteCount);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public static T As<T>(object o) where T : class
		{
			return (T)o;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public unsafe static ref T AsRef<T>(void* source)
		{
			return ref *(T*)source;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public static ref T AsRef<T>(in T source)
		{
			return ref source;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public static ref TTo As<TFrom, TTo>(ref TFrom source)
		{
			return ref System.Runtime.CompilerServices.Unsafe.As<TFrom, TTo>(ref source);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public static ref T Unbox<T>(object box) where T : struct
		{
			return ref (T)box;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public static ref T Add<T>(ref T source, int elementOffset)
		{
			return ref System.Runtime.CompilerServices.Unsafe.Add(ref source, elementOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public unsafe static void* Add<T>(void* source, int elementOffset)
		{
			return (byte*)source + (nint)elementOffset * (nint)System.Runtime.CompilerServices.Unsafe.SizeOf<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public static ref T Add<T>(ref T source, IntPtr elementOffset)
		{
			return ref System.Runtime.CompilerServices.Unsafe.Add(ref source, elementOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public static ref T AddByteOffset<T>(ref T source, IntPtr byteOffset)
		{
			return ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref source, byteOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public static ref T Subtract<T>(ref T source, int elementOffset)
		{
			return ref System.Runtime.CompilerServices.Unsafe.Subtract(ref source, elementOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public unsafe static void* Subtract<T>(void* source, int elementOffset)
		{
			return (byte*)source - (nint)elementOffset * (nint)System.Runtime.CompilerServices.Unsafe.SizeOf<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public static ref T Subtract<T>(ref T source, IntPtr elementOffset)
		{
			return ref System.Runtime.CompilerServices.Unsafe.Subtract(ref source, elementOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public static ref T SubtractByteOffset<T>(ref T source, IntPtr byteOffset)
		{
			return ref System.Runtime.CompilerServices.Unsafe.SubtractByteOffset(ref source, byteOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public static IntPtr ByteOffset<T>(ref T origin, ref T target)
		{
			return System.Runtime.CompilerServices.Unsafe.ByteOffset(target: ref target, origin: ref origin);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public static bool AreSame<T>(ref T left, ref T right)
		{
			return System.Runtime.CompilerServices.Unsafe.AreSame(ref left, ref right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public static bool IsAddressGreaterThan<T>(ref T left, ref T right)
		{
			return System.Runtime.CompilerServices.Unsafe.IsAddressGreaterThan(ref left, ref right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Runtime.Versioning.NonVersionable]
		public static bool IsAddressLessThan<T>(ref T left, ref T right)
		{
			return System.Runtime.CompilerServices.Unsafe.IsAddressLessThan(ref left, ref right);
		}
	}
}
