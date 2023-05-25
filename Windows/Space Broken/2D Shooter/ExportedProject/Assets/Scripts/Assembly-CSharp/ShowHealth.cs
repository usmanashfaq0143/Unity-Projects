using UnityEngine;

public class ShowHealth : MonoBehaviour
{
	public GameObject health1;

	public GameObject health2;

	public GameObject health3;

	public Health health;

	private void Start()
	{
		health = GameObject.Find("Player").GetComponent<Health>();
		if (health.currentLives == 3)
		{
			health1.SetActive(value: true);
			health2.SetActive(value: true);
			health3.SetActive(value: true);
		}
	}

	private void Update()
	{
		if (health.currentHealth < 3)
		{
			if (health.currentLives == 2)
			{
				health3.SetActive(value: false);
			}
			if (health.currentLives == 1)
			{
				health2.SetActive(value: false);
			}
			if (health.currentLives == 0)
			{
				health1.SetActive(value: false);
			}
		}
		else
		{
			health1.SetActive(value: true);
			health2.SetActive(value: true);
			health3.SetActive(value: true);
		}
	}
}
