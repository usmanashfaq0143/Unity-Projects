using UnityEngine;

public class PortalSpawn : MonoBehaviour
{
	public GameObject target;

	public GameObject spawn;

	private int timer;

	private void Start()
	{
		spawn.SetActive(value: false);
	}

	private void Update()
	{
		if (!target.activeInHierarchy)
		{
			spawn.SetActive(value: true);
		}
	}
}
