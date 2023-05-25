using UnityEngine;

public class SpeedUp : MonoBehaviour
{
	public GameObject player;

	private ShootingController fireRate;

	public float timeRemaining = 20f;

	private void Start()
	{
		fireRate = player.GetComponent<ShootingController>();
	}

	private void Update()
	{
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		fireRate.fireRate /= 2f;
	}
}
