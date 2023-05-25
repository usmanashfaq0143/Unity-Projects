using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : UIelement
{
	[Tooltip("The text UI to use for display")]
	public Text displayText;

	public void DisplayScore()
	{
		if (displayText != null)
		{
			displayText.text = "Score: " + GameManager.score;
		}
	}

	public override void UpdateUI()
	{
		base.UpdateUI();
		DisplayScore();
	}
}
