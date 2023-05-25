using UnityEngine;

public class GetFirePower : MonoBehaviour
{
	public GameObject player;

	private ShootingController fire;

	private void Start()
	{
		fire = player.GetComponent<ShootingController>();
	}

	private void Update()
	{
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		fire.fireRate = 0.1f;
	}
}
