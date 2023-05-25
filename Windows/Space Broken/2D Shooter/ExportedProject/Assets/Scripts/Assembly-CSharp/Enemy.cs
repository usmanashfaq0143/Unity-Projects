using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	public enum ShootMode
	{
		None = 0,
		ShootAll = 1
	}

	public enum MovementModes
	{
		NoMovement = 0,
		FollowTarget = 1,
		Scroll = 2
	}

	[Header("Settings")]
	[Tooltip("The speed at which the enemy moves.")]
	public float moveSpeed = 5f;

	[Tooltip("The score value for defeating this enemy")]
	public int scoreValue = 5;

	[Header("Following Settings")]
	[Tooltip("The transform of the object that this enemy should follow.")]
	public Transform followTarget;

	[Tooltip("The distance at which the enemy begins following the follow target.")]
	public float followRange = 10f;

	[Header("Shooting")]
	[Tooltip("The enemy's gun components")]
	public List<ShootingController> guns = new List<ShootingController>();

	[Tooltip("The way the enemy shoots:\nNone: Enemy does not shoot.\nShootAll: Enemy fires all guns whenever it can.")]
	public ShootMode shootMode = ShootMode.ShootAll;

	[Tooltip("The way this enemy will move\nNoMovement: This enemy will not move.\nFollowTarget: This enemy will follow the assigned target.\nScroll: This enemy will move in one horizontal direction only.")]
	public MovementModes movementMode = MovementModes.FollowTarget;

	[SerializeField]
	private Vector3 scrollDirection = Vector3.right;

	private void LateUpdate()
	{
		HandleBehaviour();
	}

	private void Start()
	{
		if (movementMode == MovementModes.FollowTarget && followTarget == null && GameManager.instance != null && GameManager.instance.player != null)
		{
			followTarget = GameManager.instance.player.transform;
		}
	}

	private void HandleBehaviour()
	{
		if (followTarget != null && (followTarget.position - base.transform.position).magnitude < followRange)
		{
			MoveEnemy();
		}
		TryToShoot();
	}

	public void DoBeforeDestroy()
	{
		AddToScore();
		IncrementEnemiesDefeated();
	}

	private void AddToScore()
	{
		if (GameManager.instance != null && !GameManager.instance.gameIsOver)
		{
			GameManager.AddScore(scoreValue);
		}
	}

	private void IncrementEnemiesDefeated()
	{
		if (GameManager.instance != null && !GameManager.instance.gameIsOver)
		{
			GameManager.instance.IncrementEnemiesDefeated();
		}
	}

	private void MoveEnemy()
	{
		Vector3 desiredMovement = GetDesiredMovement();
		Quaternion desiredRotation = GetDesiredRotation();
		base.transform.position = base.transform.position + desiredMovement;
		base.transform.rotation = desiredRotation;
	}

	protected virtual Vector3 GetDesiredMovement()
	{
		return movementMode switch
		{
			MovementModes.FollowTarget => GetFollowPlayerMovement(), 
			MovementModes.Scroll => GetScrollingMovement(), 
			_ => Vector3.zero, 
		};
	}

	protected virtual Quaternion GetDesiredRotation()
	{
		return movementMode switch
		{
			MovementModes.FollowTarget => GetFollowPlayerRotation(), 
			MovementModes.Scroll => GetScrollingRotation(), 
			_ => base.transform.rotation, 
		};
	}

	private void TryToShoot()
	{
		ShootMode shootMode = this.shootMode;
		if (shootMode == ShootMode.None || shootMode != ShootMode.ShootAll)
		{
			return;
		}
		foreach (ShootingController gun in guns)
		{
			gun.Fire();
		}
	}

	private Vector3 GetFollowPlayerMovement()
	{
		return (followTarget.position - base.transform.position).normalized * moveSpeed * Time.deltaTime;
	}

	private Quaternion GetFollowPlayerRotation()
	{
		float z = Vector3.SignedAngle(Vector3.down, (followTarget.position - base.transform.position).normalized, Vector3.forward);
		return Quaternion.Euler(0f, 0f, z);
	}

	private Vector3 GetScrollingMovement()
	{
		scrollDirection = GetScrollDirection();
		return scrollDirection * moveSpeed * Time.deltaTime;
	}

	private Quaternion GetScrollingRotation()
	{
		return Quaternion.identity;
	}

	private Vector3 GetScrollDirection()
	{
		Camera main = Camera.main;
		if (main != null)
		{
			Vector2 point = main.WorldToScreenPoint(base.transform.position);
			if (!main.pixelRect.Contains(point))
			{
				return scrollDirection * -1f;
			}
		}
		return scrollDirection;
	}
}
