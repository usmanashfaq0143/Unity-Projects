using System;
using System.Linq;
using System.Reflection;

namespace Unity.VisualScripting
{
	public class ReflectionInvoker : IOptimizedInvoker
	{
		private readonly MethodInfo methodInfo;

		private static readonly object[] EmptyObjects = new object[0];

		public ReflectionInvoker(MethodInfo methodInfo)
		{
			if (OptimizedReflection.safeMode)
			{
				Ensure.That("methodInfo").IsNotNull(methodInfo);
			}
			this.methodInfo = methodInfo;
		}

		public void Compile()
		{
		}

		public object Invoke(object target, params object[] args)
		{
			return methodInfo.Invoke(target, args);
		}

		public object Invoke(object target)
		{
			return methodInfo.Invoke(target, EmptyObjects);
		}

		public object Invoke(object target, object arg0)
		{
			return methodInfo.Invoke(target, new object[1] { arg0 });
		}

		public object Invoke(object target, object arg0, object arg1)
		{
			return methodInfo.Invoke(target, new object[2] { arg0, arg1 });
		}

		public object Invoke(object target, object arg0, object arg1, object arg2)
		{
			return methodInfo.Invoke(target, new object[3] { arg0, arg1, arg2 });
		}

		public object Invoke(object target, object arg0, object arg1, object arg2, object arg3)
		{
			return methodInfo.Invoke(target, new object[4] { arg0, arg1, arg2, arg3 });
		}

		public object Invoke(object target, object arg0, object arg1, object arg2, object arg3, object arg4)
		{
			return methodInfo.Invoke(target, new object[5] { arg0, arg1, arg2, arg3, arg4 });
		}

		public Type[] GetParameterTypes()
		{
			return (from pi in methodInfo.GetParameters()
				select pi.ParameterType).ToArray();
		}
	}
}
