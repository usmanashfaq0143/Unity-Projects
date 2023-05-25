using UnityEngine;

public class Pickup : MonoBehaviour
{
	public GameObject target;

	private bool isDestroyed;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		isDestroyed = true;
		target.SetActive(value: false);
	}
}
