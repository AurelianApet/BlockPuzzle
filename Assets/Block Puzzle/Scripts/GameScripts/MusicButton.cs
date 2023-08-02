using UnityEngine;
using System.Collections;

public class MusicButton : ToggleButton
{
	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
		btnToggleGraphicsPosition = btnToggleGraphics.rectTransform.anchoredPosition;
	}

	/// <summary>
	/// Raises the enable event.
	/// </summary>
	void OnEnable ()
	{
		Invoke ("initMusicStatus", 0.1F);
	}

	/// <summary>
	/// Inits the music status.
	/// </summary>
	void initMusicStatus ()
	{
		bool isMusicEnabled = AudioManager.instance.isMusicEnabled;
		Vector2 points = btnToggleGraphics.rectTransform.anchoredPosition;
		btnToggleGraphics.rectTransform.anchoredPosition = new Vector2 (((isMusicEnabled) ? (Mathf.Abs (points.x)) : -(Mathf.Abs (points.x))), points.y);
	}

	/// <summary>
	/// Raises the toggle status changed event.
	/// </summary>
	/// <param name="status">If set to <c>true</c> status.</param>
	public override void OnToggleStatusChanged (bool status)
	{
		AudioManager.instance.PlayButtonClickSound ();
		AudioManager.instance.ToggleMusicStatus (status);
	}
}
