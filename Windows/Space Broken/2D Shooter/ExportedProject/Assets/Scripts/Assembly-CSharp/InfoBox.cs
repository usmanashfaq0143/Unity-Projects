using UnityEngine;

public class InfoBox : MonoBehaviour
{
	public float timeRemaining = 3f;

	public new GameObject gameObject;

	private void Start()
	{
	}

	private void Update()
	{
		if (timeRemaining > 0f)
		{
			timeRemaining -= Time.deltaTime;
		}
		else
		{
			Object.Destroy(gameObject);
		}
	}
}
