using UnityEngine;

public class NewSpawnerInvoke : MonoBehaviour
{
	public GameObject enemyInvoke;

	public GameObject check;

	public bool stopSpawning;

	public float spawnTime;

	public float spawnDelay;

	public float timeBetweenWaves;

	private float counter;

	private void Start()
	{
		InvokeRepeating("SpawnObject", spawnTime, spawnDelay);
	}

	private void Update()
	{
		if (!check.activeInHierarchy)
		{
			stopSpawning = true;
		}
	}

	public void SpawnObject()
	{
		Object.Instantiate(enemyInvoke, base.transform.position, base.transform.rotation);
		if (stopSpawning)
		{
			CancelInvoke("SpawnObject");
		}
	}
}
