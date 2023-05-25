namespace Unity.VisualScripting
{
	public sealed class DecrementHandler : UnaryOperatorHandler
	{
		public DecrementHandler()
			: base("Decrement", "Decrement", "--", "op_Decrement")
		{
			Handle((byte a) => a = (byte)(a - 1));
			Handle((sbyte a) => a = (sbyte)(a - 1));
			Handle((short a) => a = (short)(a - 1));
			Handle((ushort a) => a = (ushort)(a - 1));
			Handle((int a) => --a);
			Handle((uint a) => --a);
			Handle((long a) => --a);
			Handle((ulong a) => --a);
			Handle((float a) => a -= 1f);
			Handle((decimal a) => --a);
			Handle((double a) => a -= 1.0);
		}
	}
}
