using UnityEngine;

public class DirectionalMover : MonoBehaviour
{
	[Header("Settings")]
	[Tooltip("The direction to move in")]
	public Vector3 direction = Vector3.down;

	[Tooltip("The speed to move at")]
	public float speed = 5f;

	private void Update()
	{
		Move();
	}

	private void Move()
	{
		base.transform.position = base.transform.position + direction.normalized * speed * Time.deltaTime;
	}
}
