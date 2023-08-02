using UnityEngine;
using System.Collections;

public class QuitconfirmGame : MonoBehaviour
{
	/// <summary>
	/// Raises the close button pressed event.
	/// </summary>
	public void OnCloseButtonPressed ()
	{
		if (InputManager.instance.canInput ()) {
			AudioManager.instance.PlayButtonClickSound ();
			GameController.instance.OnCloseButtonPressed ();
		}
	}

	/// <summary>
	/// Raises the ok button pressed event.
	/// </summary>
	public void OnOkButtonPressed ()
	{
		if (InputManager.instance.canInput ()) {
			AudioManager.instance.PlayButtonClickSound ();
			GameController.instance.OnCloseButtonPressed ();
			Invoke ("QuitGame", 0.4F);
		}
	}

	/// <summary>
	/// Quits the game.
	/// </summary>
	void QuitGame ()
	{
		print ("Quitting Game..");
		Application.Quit ();
	}
}
