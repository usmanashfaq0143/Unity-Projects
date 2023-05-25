using System.Collections;
using UnityEngine;

public class NewSpawner : MonoBehaviour
{
	public int enemiesToSpawn;

	public float timeBetweenSpawn;

	public float timeBetweenWaves;

	public float nextSpawnTime;

	private bool stopSpawning;

	private float timeLapsed;

	private int spawnedEnemyCount;

	private int holdEnemyCount;

	public Enemy enemy;

	private Coroutine test;

	private IEnumerator testEnumarator;

	private void Start()
	{
	}

	private void Update()
	{
		timeLapsed += Time.time;
		if (timeLapsed > timeBetweenSpawn)
		{
			stopSpawning = true;
			timeLapsed = 0f;
		}
		else if (timeLapsed < timeBetweenWaves)
		{
			stopSpawning = false;
			timeLapsed = 0f;
		}
		if (!stopSpawning)
		{
			Object.Instantiate(enemy, base.transform.position, base.transform.rotation);
			Object.Destroy(enemy, 15f);
		}
	}
}
