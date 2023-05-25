using UnityEngine;

namespace Unity.VisualScripting
{
	public class EqualityHandler : BinaryOperatorHandler
	{
		public EqualityHandler()
			: base("Equality", "Equal", "==", "op_Equality")
		{
			Handle((byte a, byte b) => a == b);
			Handle((byte a, sbyte b) => a == b);
			Handle((byte a, short b) => a == b);
			Handle((byte a, ushort b) => a == b);
			Handle((byte a, int b) => a == b);
			Handle((byte a, uint b) => a == b);
			Handle((byte a, long b) => a == b);
			Handle((byte a, ulong b) => a == b);
			Handle((byte a, float b) => (float)(int)a == b);
			Handle((byte a, decimal b) => (decimal)a == b);
			Handle((byte a, double b) => (double)(int)a == b);
			Handle((sbyte a, byte b) => a == b);
			Handle((sbyte a, sbyte b) => a == b);
			Handle((sbyte a, short b) => a == b);
			Handle((sbyte a, ushort b) => a == b);
			Handle((sbyte a, int b) => a == b);
			Handle((sbyte a, uint b) => a == b);
			Handle((sbyte a, long b) => a == b);
			Handle((sbyte a, float b) => (float)a == b);
			Handle((sbyte a, decimal b) => (decimal)a == b);
			Handle((sbyte a, double b) => (double)a == b);
			Handle((short a, byte b) => a == b);
			Handle((short a, sbyte b) => a == b);
			Handle((short a, short b) => a == b);
			Handle((short a, ushort b) => a == b);
			Handle((short a, int b) => a == b);
			Handle((short a, uint b) => a == b);
			Handle((short a, long b) => a == b);
			Handle((short a, float b) => (float)a == b);
			Handle((short a, decimal b) => (decimal)a == b);
			Handle((short a, double b) => (double)a == b);
			Handle((ushort a, byte b) => a == b);
			Handle((ushort a, sbyte b) => a == b);
			Handle((ushort a, short b) => a == b);
			Handle((ushort a, ushort b) => a == b);
			Handle((ushort a, int b) => a == b);
			Handle((ushort a, uint b) => a == b);
			Handle((ushort a, long b) => a == b);
			Handle((ushort a, ulong b) => a == b);
			Handle((ushort a, float b) => (float)(int)a == b);
			Handle((ushort a, decimal b) => (decimal)a == b);
			Handle((ushort a, double b) => (double)(int)a == b);
			Handle((int a, byte b) => a == b);
			Handle((int a, sbyte b) => a == b);
			Handle((int a, short b) => a == b);
			Handle((int a, ushort b) => a == b);
			Handle((int a, int b) => a == b);
			Handle((int a, uint b) => a == b);
			Handle((int a, long b) => a == b);
			Handle((int a, float b) => (float)a == b);
			Handle((int a, decimal b) => (decimal)a == b);
			Handle((int a, double b) => (double)a == b);
			Handle((uint a, byte b) => a == b);
			Handle((uint a, sbyte b) => a == b);
			Handle((uint a, short b) => a == b);
			Handle((uint a, ushort b) => a == b);
			Handle((uint a, int b) => a == b);
			Handle((uint a, uint b) => a == b);
			Handle((uint a, long b) => a == b);
			Handle((uint a, ulong b) => a == b);
			Handle((uint a, float b) => (float)a == b);
			Handle((uint a, decimal b) => (decimal)a == b);
			Handle((uint a, double b) => (double)a == b);
			Handle((long a, byte b) => a == b);
			Handle((long a, sbyte b) => a == b);
			Handle((long a, short b) => a == b);
			Handle((long a, ushort b) => a == b);
			Handle((long a, int b) => a == b);
			Handle((long a, uint b) => a == b);
			Handle((long a, long b) => a == b);
			Handle((long a, float b) => (float)a == b);
			Handle((long a, decimal b) => (decimal)a == b);
			Handle((long a, double b) => (double)a == b);
			Handle((ulong a, byte b) => a == b);
			Handle((ulong a, ushort b) => a == b);
			Handle((ulong a, uint b) => a == b);
			Handle((ulong a, ulong b) => a == b);
			Handle((ulong a, float b) => (float)a == b);
			Handle((ulong a, decimal b) => (decimal)a == b);
			Handle((ulong a, double b) => (double)a == b);
			Handle((float a, byte b) => a == (float)(int)b);
			Handle((float a, sbyte b) => a == (float)b);
			Handle((float a, short b) => a == (float)b);
			Handle((float a, ushort b) => a == (float)(int)b);
			Handle((float a, int b) => a == (float)b);
			Handle((float a, uint b) => a == (float)b);
			Handle((float a, long b) => a == (float)b);
			Handle((float a, ulong b) => a == (float)b);
			Handle((float a, float b) => Mathf.Approximately(a, b));
			Handle((float a, double b) => (double)a == b);
			Handle((decimal a, byte b) => a == (decimal)b);
			Handle((decimal a, sbyte b) => a == (decimal)b);
			Handle((decimal a, short b) => a == (decimal)b);
			Handle((decimal a, ushort b) => a == (decimal)b);
			Handle((decimal a, int b) => a == (decimal)b);
			Handle((decimal a, uint b) => a == (decimal)b);
			Handle((decimal a, long b) => a == (decimal)b);
			Handle((decimal a, ulong b) => a == (decimal)b);
			Handle((decimal a, decimal b) => a == b);
			Handle((double a, byte b) => a == (double)(int)b);
			Handle((double a, sbyte b) => a == (double)b);
			Handle((double a, short b) => a == (double)b);
			Handle((double a, ushort b) => a == (double)(int)b);
			Handle((double a, int b) => a == (double)b);
			Handle((double a, uint b) => a == (double)b);
			Handle((double a, long b) => a == (double)b);
			Handle((double a, ulong b) => a == (double)b);
			Handle((double a, float b) => a == (double)b);
			Handle((double a, double b) => a == b);
		}

		protected override object BothNullHandling()
		{
			return true;
		}

		protected override object SingleNullHandling()
		{
			return false;
		}

		protected override object CustomHandling(object leftOperand, object rightOperand)
		{
			return object.Equals(leftOperand, rightOperand);
		}
	}
}
