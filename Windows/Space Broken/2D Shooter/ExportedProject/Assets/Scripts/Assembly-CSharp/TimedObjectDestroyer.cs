using UnityEngine;

public class TimedObjectDestroyer : MonoBehaviour
{
	[Tooltip("The lifetime of this gameobject")]
	public float lifetime = 5f;

	private float timeAlive;

	[Tooltip("Whether to destroy child gameobjects when this gameobject is destroyed")]
	public bool destroyChildrenOnDeath = true;

	public static bool quitting;

	private void OnApplicationQuit()
	{
		quitting = true;
		Object.DestroyImmediate(base.gameObject);
	}

	private void Update()
	{
		if (timeAlive > lifetime)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			timeAlive += Time.deltaTime;
		}
	}

	private void OnDestroy()
	{
		if (destroyChildrenOnDeath && !quitting && Application.isPlaying)
		{
			for (int num = base.transform.childCount - 1; num >= 0; num--)
			{
				GameObject gameObject = base.transform.GetChild(num).gameObject;
				if (gameObject != null)
				{
					Object.DestroyImmediate(gameObject);
				}
			}
		}
		base.transform.DetachChildren();
	}
}
