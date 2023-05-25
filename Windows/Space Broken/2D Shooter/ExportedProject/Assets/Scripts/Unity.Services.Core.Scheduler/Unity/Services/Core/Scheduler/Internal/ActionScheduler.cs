using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Services.Core.Internal;
using UnityEngine.LowLevel;

namespace Unity.Services.Core.Scheduler.Internal
{
	internal class ActionScheduler : IActionScheduler, IServiceComponent
	{
		private readonly ITimeProvider m_TimeProvider;

		private readonly MinimumBinaryHeap<ScheduledInvocation> m_ScheduledActions = new MinimumBinaryHeap<ScheduledInvocation>();

		private readonly Dictionary<long, ScheduledInvocation> m_IdScheduledInvocationMap = new Dictionary<long, ScheduledInvocation>();

		private const long k_MinimumIdValue = 1L;

		internal readonly PlayerLoopSystem SchedulerLoopSystem;

		private long m_NextId = 1L;

		public int ScheduledActionsCount => m_ScheduledActions.Count;

		public ActionScheduler()
			: this(new UtcTimeProvider())
		{
		}

		public ActionScheduler(ITimeProvider timeProvider)
		{
			m_TimeProvider = timeProvider;
			SchedulerLoopSystem = new PlayerLoopSystem
			{
				type = typeof(ActionScheduler),
				updateDelegate = ExecuteExpiredActions
			};
		}

		public long ScheduleAction([NotNull] Action action, double delaySeconds = 0.0)
		{
			if (delaySeconds < 0.0)
			{
				throw new ArgumentException("delaySeconds can not be negative");
			}
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			ScheduledInvocation scheduledInvocation = new ScheduledInvocation
			{
				Action = action,
				InvocationTime = m_TimeProvider.Now.AddSeconds(delaySeconds),
				ActionId = m_NextId++
			};
			if (m_NextId < 1)
			{
				m_NextId = 1L;
			}
			m_ScheduledActions.Insert(scheduledInvocation);
			m_IdScheduledInvocationMap.Add(scheduledInvocation.ActionId, scheduledInvocation);
			return scheduledInvocation.ActionId;
		}

		public void CancelAction(long actionId)
		{
			if (m_IdScheduledInvocationMap.ContainsKey(actionId))
			{
				ScheduledInvocation scheduledInvocation = m_IdScheduledInvocationMap[actionId];
				m_ScheduledActions.Remove(scheduledInvocation);
				m_IdScheduledInvocationMap.Remove(scheduledInvocation.ActionId);
			}
		}

		internal void ExecuteExpiredActions()
		{
			while (m_ScheduledActions.Count > 0 && m_ScheduledActions.Min.InvocationTime <= m_TimeProvider.Now)
			{
				ScheduledInvocation scheduledInvocation = m_ScheduledActions.ExtractMin();
				m_IdScheduledInvocationMap.Remove(scheduledInvocation.ActionId);
				try
				{
					scheduledInvocation.Action();
				}
				catch (Exception exception)
				{
					CoreLogger.LogException(exception);
				}
			}
		}

		private static void UpdateSubSystemList(List<PlayerLoopSystem> subSystemList, PlayerLoopSystem currentPlayerLoop)
		{
			currentPlayerLoop.subSystemList = subSystemList.ToArray();
			PlayerLoop.SetPlayerLoop(currentPlayerLoop);
		}

		public void JoinPlayerLoopSystem()
		{
			PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
			List<PlayerLoopSystem> list = new List<PlayerLoopSystem>(currentPlayerLoop.subSystemList);
			if (!list.Contains(SchedulerLoopSystem))
			{
				list.Add(SchedulerLoopSystem);
				UpdateSubSystemList(list, currentPlayerLoop);
			}
		}

		public void QuitPlayerLoopSystem()
		{
			PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
			List<PlayerLoopSystem> list = new List<PlayerLoopSystem>(currentPlayerLoop.subSystemList);
			list.Remove(SchedulerLoopSystem);
			UpdateSubSystemList(list, currentPlayerLoop);
		}
	}
}
