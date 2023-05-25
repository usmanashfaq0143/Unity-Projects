using UnityEngine;

namespace Unity.VisualScripting
{
	[AddComponentMenu("")]
	public sealed class UnityOnParticleCollisionMessageListener : MessageListener
	{
		private void OnParticleCollision(GameObject other)
		{
			EventBus.Trigger("OnParticleCollision", base.gameObject, other);
		}
	}
}
