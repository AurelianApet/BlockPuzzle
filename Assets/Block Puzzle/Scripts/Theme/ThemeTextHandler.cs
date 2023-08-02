using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ThemeTextHandler : MonoBehaviour {

	public List<Color> ThemeColors;
	Text text;

	void Awake()
	{
		text = GetComponent<Text> ();

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
		if (text != null) {
			text.color = (isDarkTheme) ? ThemeColors [0] : ThemeColors [1];
		}
	}
}
