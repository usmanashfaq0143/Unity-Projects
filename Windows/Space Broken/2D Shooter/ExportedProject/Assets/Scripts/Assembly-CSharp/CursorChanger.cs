using UnityEngine;

public class CursorChanger : MonoBehaviour
{
	[Header("Settings:")]
	[Tooltip("The cursor to change to")]
	public Texture2D newCursorSprite;

	private void Start()
	{
		ChangeCursor();
	}

	private void ChangeCursor()
	{
		Cursor.lockState = CursorLockMode.Confined;
		Vector2 hotspot = default(Vector2);
		hotspot.x = newCursorSprite.width / 2;
		hotspot.y = newCursorSprite.height / 2;
		Cursor.SetCursor(newCursorSprite, hotspot, CursorMode.Auto);
	}
}
