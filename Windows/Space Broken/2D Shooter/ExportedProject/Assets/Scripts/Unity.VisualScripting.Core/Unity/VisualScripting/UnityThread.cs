using System;
using System.Threading;

namespace Unity.VisualScripting
{
	public static class UnityThread
	{
		public static Thread thread = Thread.CurrentThread;

		public static Action<Action> editorAsync;

		public static bool allowsAPI
		{
			get
			{
				if (!Serialization.isUnitySerializing)
				{
					return Thread.CurrentThread == thread;
				}
				return false;
			}
		}

		internal static void RuntimeInitialize()
		{
			thread = Thread.CurrentThread;
		}

		public static void EditorAsync(Action action)
		{
			editorAsync?.Invoke(action);
		}
	}
}
