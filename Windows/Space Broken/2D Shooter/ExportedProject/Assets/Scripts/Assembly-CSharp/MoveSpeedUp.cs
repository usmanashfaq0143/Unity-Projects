using UnityEngine;

public class MoveSpeedUp : MonoBehaviour
{
	public GameObject player;

	private Controller controller;

	private void Start()
	{
		controller = player.GetComponent<Controller>();
	}

	private void Update()
	{
	}

	public void OnCollisionEnter2D(Collision2D collision)
	{
		controller.moveSpeed *= 1.5f;
	}
}
