using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
	public enum CameraStyles
	{
		Locked = 0,
		Overhead = 1,
		Free = 2
	}

	[HideInInspector]
	private Camera playerCamera;

	[Header("GameObject References")]
	[Tooltip("The target to follow with this camera")]
	public Transform target;

	[Header("CameraMovement")]
	[Tooltip("The way this camera moves:\n\tLocked: Camera cannot follow mouse, it stays locked onto the target.\n\tScroll: Camera stays within the max scroll distance of the target, but follows the mouse\n\tFree: Camera follows the mouse, regardless of the target position")]
	public CameraStyles cameraMovementStyle;

	[Tooltip("The distance between the target position and the mouse to move the camera to in \"Free\" mode.")]
	[Range(0f, 0.75f)]
	public float freeCameraMouseTracking = 0.5f;

	[Tooltip("The maximum distance away from the target that the camera can move")]
	public float maxDistanceFromTarget = 5f;

	[Tooltip("The z coordinate to use for the camera position")]
	public float cameraZCoordinate = -10f;

	private InputManager inputManager;

	private void Start()
	{
		playerCamera = GetComponent<Camera>();
		if (inputManager == null)
		{
			inputManager = InputManager.instance;
		}
		if (inputManager == null)
		{
			Debug.LogWarning("The Camera Controller can not find an Input Manager in the scene");
		}
	}

	private void Update()
	{
		SetCameraPosition();
	}

	private void SetCameraPosition()
	{
		if (target != null)
		{
			Vector3 targetPosition = GetTargetPosition();
			Vector3 playerMousePosition = GetPlayerMousePosition();
			Vector3 position = ComputeCameraPosition(targetPosition, playerMousePosition);
			base.transform.position = position;
		}
	}

	public Vector3 GetTargetPosition()
	{
		if (target != null)
		{
			return target.position;
		}
		return base.transform.position;
	}

	public Vector3 GetPlayerMousePosition()
	{
		if (cameraMovementStyle == CameraStyles.Locked)
		{
			return Vector3.zero;
		}
		return playerCamera.ScreenToWorldPoint(new Vector2(inputManager.horizontalLookAxis, inputManager.verticalLookAxis));
	}

	public Vector3 ComputeCameraPosition(Vector3 targetPosition, Vector3 mousePosition)
	{
		Vector3 result = Vector3.zero;
		switch (cameraMovementStyle)
		{
		case CameraStyles.Locked:
			result = base.transform.position;
			break;
		case CameraStyles.Overhead:
			result = targetPosition;
			break;
		case CameraStyles.Free:
		{
			Vector3 vector = Vector3.Lerp(targetPosition, mousePosition, freeCameraMouseTracking) - targetPosition;
			vector = Vector3.ClampMagnitude(vector, maxDistanceFromTarget);
			result = targetPosition + vector;
			break;
		}
		}
		result.z = cameraZCoordinate;
		return result;
	}
}
