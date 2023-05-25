using System;

namespace Unity.Services.Core.Scheduler.Internal
{
	internal class ScheduledInvocation : IComparable<ScheduledInvocation>
	{
		public Action Action;

		public DateTime InvocationTime;

		public long ActionId;

		public int CompareTo(ScheduledInvocation that)
		{
			int num = InvocationTime.CompareTo(that.InvocationTime);
			if (num == 0)
			{
				num = ActionId.CompareTo(that.ActionId);
			}
			return num;
		}
	}
}
