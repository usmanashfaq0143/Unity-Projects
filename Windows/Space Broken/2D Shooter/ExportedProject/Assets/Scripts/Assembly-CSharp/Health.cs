using UnityEngine;

public class Health : MonoBehaviour
{
	[Header("Team Settings")]
	[Tooltip("The team associated with this damage")]
	public int teamId;

	[Header("Health Settings")]
	[Tooltip("The default health value")]
	public int defaultHealth = 1;

	[Tooltip("The maximum health value")]
	public int maximumHealth = 1;

	[Tooltip("The current in game health value")]
	public int currentHealth = 1;

	[Tooltip("Invulnerability duration, in seconds, after taking damage")]
	public float invincibilityTime = 3f;

	[Tooltip("Whether or not this health is always invincible")]
	public bool isAlwaysInvincible;

	[Header("Lives settings")]
	[Tooltip("Whether or not to use lives")]
	public bool useLives;

	[Tooltip("Current number of lives this health has")]
	public int currentLives = 3;

	[Tooltip("The maximum number of lives this health can have")]
	public int maximumLives = 5;

	private float timeToBecomeDamagableAgain;

	private bool isInvincableFromDamage;

	private Vector3 respawnPosition;

	[Header("Effects & Polish")]
	[Tooltip("The effect to create when this health dies")]
	public GameObject deathEffect;

	[Tooltip("The effect to create when this health is damaged")]
	public GameObject hitEffect;

	private void Start()
	{
		SetRespawnPoint(base.transform.position);
	}

	private void Update()
	{
		InvincibilityCheck();
	}

	private void InvincibilityCheck()
	{
		if (timeToBecomeDamagableAgain <= Time.time)
		{
			isInvincableFromDamage = false;
		}
	}

	public void SetRespawnPoint(Vector3 newRespawnPosition)
	{
		respawnPosition = newRespawnPosition;
	}

	private void Respawn()
	{
		base.transform.position = respawnPosition;
		currentHealth = defaultHealth;
	}

	public void TakeDamage(int damageAmount)
	{
		if (!isInvincableFromDamage && !isAlwaysInvincible)
		{
			if (hitEffect != null)
			{
				Object.Instantiate(hitEffect, base.transform.position, base.transform.rotation, null);
			}
			timeToBecomeDamagableAgain = Time.time + invincibilityTime;
			isInvincableFromDamage = true;
			currentHealth -= damageAmount;
			CheckDeath();
		}
	}

	public void ReceiveHealing(int healingAmount)
	{
		currentHealth += healingAmount;
		if (currentHealth > maximumHealth)
		{
			currentHealth = maximumHealth;
		}
		CheckDeath();
	}

	private bool CheckDeath()
	{
		if (currentHealth <= 0)
		{
			Die();
			return true;
		}
		return false;
	}

	public void Die()
	{
		if (deathEffect != null)
		{
			Object.Instantiate(deathEffect, base.transform.position, base.transform.rotation, null);
		}
		if (useLives)
		{
			HandleDeathWithLives();
		}
		else
		{
			HandleDeathWithoutLives();
		}
	}

	private void HandleDeathWithLives()
	{
		currentLives--;
		if (currentLives > 0)
		{
			Respawn();
			return;
		}
		if (base.gameObject.tag == "Player" && GameManager.instance != null)
		{
			GameManager.instance.GameOver();
		}
		if (base.gameObject.GetComponent<Enemy>() != null)
		{
			base.gameObject.GetComponent<Enemy>().DoBeforeDestroy();
		}
		Object.Destroy(base.gameObject);
	}

	private void HandleDeathWithoutLives()
	{
		if (base.gameObject.tag == "Player" && GameManager.instance != null)
		{
			GameManager.instance.GameOver();
		}
		if (base.gameObject.GetComponent<Enemy>() != null)
		{
			base.gameObject.GetComponent<Enemy>().DoBeforeDestroy();
		}
		Object.Destroy(base.gameObject);
	}
}
