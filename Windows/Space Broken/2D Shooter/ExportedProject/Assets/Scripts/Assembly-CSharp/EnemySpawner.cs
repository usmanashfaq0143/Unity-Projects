using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
	[Header("GameObject References")]
	[Tooltip("The enemy prefab to use when spawning enemies")]
	public GameObject enemyPrefab;

	[Tooltip("The target of the spwaned enemies")]
	public Transform target;

	[Header("Spawn Position")]
	[Tooltip("The distance within which enemies can spawn in the X direction")]
	[Min(0f)]
	public float spawnRangeX = 10f;

	[Tooltip("The distance within which enemies can spawn in the Y direction")]
	[Min(0f)]
	public float spawnRangeY = 10f;

	[Header("Spawn Variables")]
	[Tooltip("The maximum number of enemies that can be spawned from this spawner")]
	public int maxSpawn = 20;

	[Tooltip("Ignores the max spawn limit if true")]
	public bool spawnInfinite = true;

	private int currentlySpawned;

	[Tooltip("The time delay between spawning enemies")]
	public float spawnDelay = 2.5f;

	private float lastSpawnTime = float.NegativeInfinity;

	[Tooltip("The object to make projectiles child objects of.")]
	public Transform projectileHolder;

	private void Update()
	{
		CheckSpawnTimer();
	}

	private void CheckSpawnTimer()
	{
		if (Time.timeSinceLevelLoad > lastSpawnTime + spawnDelay && (currentlySpawned < maxSpawn || spawnInfinite))
		{
			Vector3 spawnLocation = GetSpawnLocation();
			SpawnEnemy(spawnLocation);
		}
	}

	private void SpawnEnemy(Vector3 spawnLocation)
	{
		if (enemyPrefab != null)
		{
			GameObject obj = Object.Instantiate(enemyPrefab, spawnLocation, enemyPrefab.transform.rotation, null);
			Enemy component = obj.GetComponent<Enemy>();
			ShootingController[] componentsInChildren = obj.GetComponentsInChildren<ShootingController>();
			if (component != null)
			{
				component.followTarget = target;
			}
			ShootingController[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].projectileHolder = projectileHolder;
			}
			currentlySpawned++;
			lastSpawnTime = Time.timeSinceLevelLoad;
		}
	}

	protected virtual Vector3 GetSpawnLocation()
	{
		float num = Random.Range(0f - spawnRangeX, spawnRangeX);
		float num2 = Random.Range(0f - spawnRangeY, spawnRangeY);
		return new Vector3(base.transform.position.x + num, base.transform.position.y + num2, 0f);
	}
}
