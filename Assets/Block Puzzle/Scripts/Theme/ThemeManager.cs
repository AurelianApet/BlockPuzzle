using UnityEngine;
using System.Collections;
using System;

public class ThemeManager : MonoBehaviour
{
	public static event Action<bool> OnThemeChangedEvent;

	public bool isDarkTheme = false;

	private static ThemeManager _instance;

	public static ThemeManager instance {
		get {
			if (_instance == null) {
				_instance = GameObject.FindObjectOfType<ThemeManager> ();
			}
			return _instance;
		}
	}

	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake ()
	{
		if (_instance != null) {
			if (_instance.gameObject != gameObject) {
				Destroy (gameObject);
				return;
			}
		}
		_instance = GameObject.FindObjectOfType<ThemeManager> ();
	}

	/// <summary>
	/// Raises the enable event.
	/// </summary>
	void OnEnable ()
	{
		initThemeStatus ();
	}

	/// <summary>
	/// Inits the audio status.
	/// </summary>
	public void initThemeStatus ()
	{
		isDarkTheme = (PlayerPrefs.GetInt ("isDarkTheme", 0) == 0) ? true : false;

		if ((!isDarkTheme) && (OnThemeChangedEvent != null)) {
			OnThemeChangedEvent.Invoke (isDarkTheme);
		}
	}

	/// <summary>
	/// Toggles the sound status.
	/// </summary>
	/// <param name="state">If set to <c>true</c> state.</param>
	public void ToggleThemeStatus (bool state)
	{
		isDarkTheme = state;
		PlayerPrefs.SetInt ("isDarkTheme", (isDarkTheme) ? 0 : 1);

		if (OnThemeChangedEvent != null) {
			OnThemeChangedEvent.Invoke (isDarkTheme);
		}
	}
}
