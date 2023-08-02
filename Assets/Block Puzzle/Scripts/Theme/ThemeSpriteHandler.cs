using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ThemeSpriteHandler : MonoBehaviour {

	public List<Color> ThemeColors;
	Image image;

	void Awake()
	{
		image = GetComponent<Image> ();

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
		image.color = (isDarkTheme) ? ThemeColors [0] : ThemeColors [1];
	}
}
