using UnityEngine;

public class PortalWin : MonoBehaviour
{
	public GameObject gameManager;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		gameManager.GetComponent<GameManager>().LevelCleared();
	}
}
