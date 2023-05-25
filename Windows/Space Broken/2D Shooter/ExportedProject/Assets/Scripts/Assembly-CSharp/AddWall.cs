using UnityEngine;

public class AddWall : MonoBehaviour
{
	public GameObject wall;

	public GameObject gameManager;

	public GameObject spawners;

	public int enemiesToKill;

	public GameObject player;

	private GameManager gameManagerScript;

	private Health health;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		wall.SetActive(value: true);
		spawners.SetActive(value: true);
		gameManager.GetComponent<GameManager>().enemiesToDefeat = enemiesToKill;
		player.GetComponent<Health>().SetRespawnPoint(new Vector3(base.transform.position.x, base.transform.position.y));
		GameObject[] array = GameObject.FindGameObjectsWithTag("Clone");
		for (int i = 0; i < array.Length; i++)
		{
			Object.Destroy(array[i]);
		}
	}
}
