using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
	public static InputManager instance;

	[Header("Player Movement Input")]
	[Tooltip("The move input along the horizontal")]
	public float horizontalMoveAxis;

	[Tooltip("The move input along the vertical")]
	public float verticalMoveAxis;

	[Header("Look Around input")]
	public float horizontalLookAxis;

	public float verticalLookAxis;

	[Header("Player Fire Input")]
	[Tooltip("Whether or not the fire button was pressed this frame")]
	public bool firePressed;

	[Tooltip("Whether or not the fire button is being held")]
	public bool fireHeld;

	[Header("Pause Input")]
	public bool pausePressed;

	private void Awake()
	{
		ResetValuesToDefault();
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void ResetValuesToDefault()
	{
		horizontalMoveAxis = 0f;
		verticalMoveAxis = 0f;
		horizontalLookAxis = 0f;
		verticalLookAxis = 0f;
		firePressed = false;
		fireHeld = false;
		pausePressed = false;
	}

	public void ReadMovementInput(InputAction.CallbackContext context)
	{
		Vector2 vector = context.ReadValue<Vector2>();
		horizontalMoveAxis = vector.x;
		verticalMoveAxis = vector.y;
	}

	public void ReadMousePositionInput(InputAction.CallbackContext context)
	{
		Vector2 vector = context.ReadValue<Vector2>();
		if (Mathf.Abs(vector.x) > 1f && Mathf.Abs(vector.y) > 1f)
		{
			horizontalLookAxis = vector.x;
			verticalLookAxis = vector.y;
		}
	}

	public void ReadFireInput(InputAction.CallbackContext context)
	{
		firePressed = !context.canceled;
		fireHeld = !context.canceled;
		StartCoroutine("ResetFireStart");
	}

	private IEnumerator ResetFireStart()
	{
		yield return new WaitForEndOfFrame();
		firePressed = false;
	}

	public void ReadPauseInput(InputAction.CallbackContext context)
	{
		pausePressed = !context.canceled;
		StartCoroutine(ResetPausePressed());
	}

	private IEnumerator ResetPausePressed()
	{
		yield return new WaitForEndOfFrame();
		pausePressed = false;
	}
}
