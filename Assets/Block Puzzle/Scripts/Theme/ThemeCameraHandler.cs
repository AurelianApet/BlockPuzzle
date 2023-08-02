using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class ThemeCameraHandler : MonoBehaviour {

	public List<Color> ThemeColors;

	void Awake()
	{
		if ((PlayerPrefs.GetInt ("isDarkTheme", 0) == 0)) {
			bool isDarkTheme = true;
			OnThemeChangedEvent (isDarkTheme);
		}
	}

	void OnEnable()
	{
		ThemeManager.OnThemeChangedEvent += OnThemeChangedEvent;
	}

	void OnDisable()
	{
		ThemeManager.OnThemeChangedEvent -= OnThemeChangedEvent;
	}

	void OnThemeChangedEvent (bool isDarkTheme)
	{
		GetComponent<Camera>().backgroundColor = (isDarkTheme) ? ThemeColors [0] : ThemeColors [1];
	}
}
