using UnityEngine;

public class Controller : MonoBehaviour
{
	public enum AimModes
	{
		AimTowardsMouse = 0,
		AimForwards = 1
	}

	public enum MovementModes
	{
		MoveHorizontally = 0,
		MoveVertically = 1,
		FreeRoam = 2,
		Astroids = 3
	}

	[Header("GameObject/Component References")]
	[Tooltip("The animator controller used to animate the player.")]
	public RuntimeAnimatorController animator;

	[Tooltip("The Rigidbody2D component to use in \"Astroids Mode\".")]
	public Rigidbody2D myRigidbody;

	[Header("Movement Variables")]
	[Tooltip("The speed at which the player will move.")]
	public float moveSpeed = 10f;

	[Tooltip("The speed at which the player rotates in asteroids movement mode")]
	public float rotationSpeed = 60f;

	private InputManager inputManager;

	[Tooltip("The aim mode in use by this player:\nAim Towards Mouse: Player rotates to face the mouse\nAim Forwards: Player aims the direction they face (doesn't face towards the mouse)")]
	public AimModes aimMode;

	[Tooltip("The movmeent mode used by this controller:\nMove Horizontally: Player can only move left/right\nMove Vertically: Player can only move up/down\nFreeRoam: Player can move in any direction and can aim\nAstroids: Player moves forward/back in the direction they are facing and rotates with horizontal input")]
	public MovementModes movementMode = MovementModes.FreeRoam;

	private bool canAimWithMouse => aimMode == AimModes.AimTowardsMouse;

	private bool lockXCoordinate => movementMode == MovementModes.MoveVertically;

	public bool lockYCoordinate => movementMode == MovementModes.MoveHorizontally;

	private void Start()
	{
		SetupInput();
	}

	private void Update()
	{
		HandleInput();
		SignalAnimator();
	}

	private void SetupInput()
	{
		if (inputManager == null)
		{
			inputManager = InputManager.instance;
		}
		if (inputManager == null)
		{
			Debug.LogWarning("There is no player input manager in the scene, there needs to be one for the Controller to work");
		}
	}

	private void HandleInput()
	{
		Vector2 lookPosition = GetLookPosition();
		Vector3 movement = new Vector3(inputManager.horizontalMoveAxis, inputManager.verticalMoveAxis, 0f);
		MovePlayer(movement);
		LookAtPoint(lookPosition);
	}

	private void SignalAnimator()
	{
		_ = animator != null;
	}

	public Vector2 GetLookPosition()
	{
		Vector2 vector = base.transform.up;
		return (aimMode == AimModes.AimForwards) ? ((Vector2)base.transform.up) : new Vector2(inputManager.horizontalLookAxis, inputManager.verticalLookAxis);
	}

	private void MovePlayer(Vector3 movement)
	{
		if (movementMode == MovementModes.Astroids)
		{
			if (myRigidbody == null)
			{
				myRigidbody = GetComponent<Rigidbody2D>();
			}
			Vector2 vector = base.transform.up * movement.y * Time.deltaTime * moveSpeed;
			Debug.Log(vector);
			myRigidbody.AddForce(vector);
			Vector3 eulerAngles = base.transform.rotation.eulerAngles;
			float z = base.transform.rotation.eulerAngles.z - rotationSpeed * movement.x * Time.deltaTime;
			eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, z);
			base.transform.rotation = Quaternion.Euler(eulerAngles);
		}
		else
		{
			if (lockXCoordinate)
			{
				movement.x = 0f;
			}
			if (lockYCoordinate)
			{
				movement.y = 0f;
			}
			base.transform.position = base.transform.position + movement * Time.deltaTime * moveSpeed;
		}
	}

	private void LookAtPoint(Vector3 point)
	{
		if (Time.timeScale > 0f)
		{
			Vector2 vector = Camera.main.ScreenToWorldPoint(point) - base.transform.position;
			if (canAimWithMouse)
			{
				base.transform.up = vector;
			}
			else if (myRigidbody != null)
			{
				myRigidbody.freezeRotation = true;
			}
		}
	}
}
