using UnityEngine;

public class Damage : MonoBehaviour
{
	[Header("Team Settings")]
	[Tooltip("The team associated with this damage")]
	public int teamId;

	[Header("Damage Settings")]
	[Tooltip("How much damage to deal")]
	public int damageAmount = 1;

	[Tooltip("Prefab to spawn after doing damage")]
	public GameObject hitEffect;

	[Tooltip("Whether or not to destroy the attached game object after dealing damage")]
	public bool destroyAfterDamage = true;

	[Tooltip("Whether or not to apply damage when triggers collide")]
	public bool dealDamageOnTriggerEnter;

	[Tooltip("Whether or not to apply damage when triggers stay, for damage over time")]
	public bool dealDamageOnTriggerStay;

	[Tooltip("Whether or not to apply damage on non-trigger collider collisions")]
	public bool dealDamageOnCollision;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (dealDamageOnTriggerEnter)
		{
			DealDamage(collision.gameObject);
		}
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (dealDamageOnTriggerStay)
		{
			DealDamage(collision.gameObject);
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (dealDamageOnCollision)
		{
			DealDamage(collision.gameObject);
		}
	}

	private void DealDamage(GameObject collisionGameObject)
	{
		Health component = collisionGameObject.GetComponent<Health>();
		if (!(component != null) || component.teamId == teamId)
		{
			return;
		}
		component.TakeDamage(damageAmount);
		if (hitEffect != null)
		{
			Object.Instantiate(hitEffect, base.transform.position, base.transform.rotation, null);
		}
		if (destroyAfterDamage)
		{
			if (base.gameObject.GetComponent<Enemy>() != null)
			{
				base.gameObject.GetComponent<Enemy>().DoBeforeDestroy();
			}
			Object.Destroy(base.gameObject);
		}
	}
}
