using UnityEngine;

public class Projectile : MonoBehaviour
{
	[Tooltip("The distance this projectile will move each second.")]
	public float projectileSpeed = 3f;

	private void Update()
	{
		MoveProjectile();
	}

	private void MoveProjectile()
	{
		base.transform.position = base.transform.position + base.transform.up * projectileSpeed * Time.deltaTime;
	}
}
