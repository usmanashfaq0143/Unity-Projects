using System.Collections;
using UnityEngine;

public class NewSpawnerCoroutine : MonoBehaviour
{
	private Coroutine test;

	private IEnumerator testEnumarator;

	public GameObject enemy;

	private void Start()
	{
	}

	private void Update()
	{
		test = StartCoroutine(TestCoroutine());
	}

	private IEnumerator TestCoroutine()
	{
		for (int i = 0; i < 3; i++)
		{
			Object.Instantiate(enemy, base.transform.position, base.transform.rotation);
			Object.Destroy(enemy, 10f);
			yield return new WaitForSeconds(1f);
		}
		yield return new WaitForSeconds(3f);
		yield return null;
	}
}
