using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Common dialog.
/// </summary>
public class CommonDialog : MonoBehaviour
{
	public Text MessageText;

	/// <summary>
	/// Raises the close button pressed event.
	/// </summary>
	public void OnCloseButtonPressed ()
	{
		if (InputManager.instance.canInput ()) {
			AudioManager.instance.PlayButtonClickSound ();
			GameController.instance.OnCloseButtonPressed ();
			InputManager.instance.AddButtonTouchEffect ();
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
			InputManager.instance.AddButtonTouchEffect ();
		}
	}
}
