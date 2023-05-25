using UnityEngine;

namespace Unity.VisualScripting
{
	[AddComponentMenu("")]
	public sealed class UnityOnCollisionExit2DMessageListener : MessageListener
	{
		private void OnCollisionExit2D(Collision2D collision)
		{
			EventBus.Trigger("OnCollisionExit2D", base.gameObject, collision);
		}
	}
}
