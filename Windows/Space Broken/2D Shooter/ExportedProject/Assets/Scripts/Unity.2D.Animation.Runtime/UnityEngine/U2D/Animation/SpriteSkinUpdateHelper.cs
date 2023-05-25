using System;
using Unity.Profiling;

namespace UnityEngine.U2D.Animation
{
	[AddComponentMenu("")]
	[DefaultExecutionOrder(-1)]
	[ExecuteInEditMode]
	internal class SpriteSkinUpdateHelper : MonoBehaviour
	{
		private ProfilerMarker m_ProfilerMarker = new ProfilerMarker("SpriteSkinUpdateHelper.LateUpdate");

		public Action<GameObject> onDestroyingComponent { get; set; }

		private void OnDestroy()
		{
			onDestroyingComponent?.Invoke(base.gameObject);
		}

		private void LateUpdate()
		{
		}
	}
}
