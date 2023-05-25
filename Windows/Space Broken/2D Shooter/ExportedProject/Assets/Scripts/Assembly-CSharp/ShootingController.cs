using UnityEngine;

public class ShootingController : MonoBehaviour
{
	[Header("GameObject/Component References")]
	[Tooltip("The projectile to be fired.")]
	public GameObject projectilePrefab;

	[Tooltip("The transform in the heirarchy which holds projectiles if any")]
	public Transform projectileHolder;

	[Header("Input")]
	[Tooltip("Whether this shooting controller is controled by the player")]
	public bool isPlayerControlled;

	[Header("Firing Settings")]
	[Tooltip("The minimum time between projectiles being fired.")]
	public float fireRate = 0.05f;

	[Tooltip("The maximum diference between the direction the shooting controller is facing and the direction projectiles are launched.")]
	public float projectileSpread = 1f;

	private float lastFired = float.NegativeInfinity;

	[Header("Effects")]
	[Tooltip("The effect to create when this fires")]
	public GameObject fireEffect;

	private InputManager inputManager;

	private void Update()
	{
		ProcessInput();
	}

	private void Start()
	{
		SetupInput();
	}

	private void SetupInput()
	{
		if (isPlayerControlled)
		{
			if (inputManager == null)
			{
				inputManager = InputManager.instance;
			}
			if (inputManager == null)
			{
				Debug.LogError("Player Shooting Controller can not find an InputManager in the scene, there needs to be one in the scene for it to run");
			}
		}
	}

	private void ProcessInput()
	{
		if (isPlayerControlled && (inputManager.firePressed || inputManager.fireHeld))
		{
			Fire();
		}
	}

	public void Fire()
	{
		if (Time.timeSinceLevelLoad - lastFired > fireRate)
		{
			SpawnProjectile();
			if (fireEffect != null)
			{
				Object.Instantiate(fireEffect, base.transform.position, base.transform.rotation, null);
			}
			lastFired = Time.timeSinceLevelLoad;
		}
	}

	public void SpawnProjectile()
	{
		if (projectilePrefab != null)
		{
			GameObject gameObject = Object.Instantiate(projectilePrefab, base.transform.position, base.transform.rotation, null);
			Vector3 eulerAngles = gameObject.transform.rotation.eulerAngles;
			eulerAngles.z += Random.Range(0f - projectileSpread, projectileSpread);
			gameObject.transform.rotation = Quaternion.Euler(eulerAngles);
			if (projectileHolder != null)
			{
				gameObject.transform.SetParent(projectileHolder);
			}
		}
	}
}
