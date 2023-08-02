using UnityEngine;
using System.Collections;

/// <summary>
/// Extention methods.
/// </summary>
public static class ExtentionMethods
{
	/// <summary>
	/// This is used for windows transition animation when new screen is entered.
	/// </summary>
	/// <param name="target">Target.</param>
	public static bool OnWindowLoad (this GameObject target)
	{
		WindowTransition transition = target.GetComponent<WindowTransition> ();
		if (transition != null) {
			transition.OnWindowAdded ();
			return true;
		}
		return false;
	}

	/// <summary>
	/// This is used for windows transition animation when new current screen is getting removed.
	/// </summary>
	/// <param name="target">Target.</param>
	public static bool OnWindowRemove (this GameObject target)
	{
		WindowTransition transition = target.GetComponent<WindowTransition> ();
		if (transition != null) {
			transition.OnWindowRemove ();
			return true;
		}
		return false;
	}

	/// <summary>
	/// Tries to parse int.
	/// </summary>
	/// <returns>The parse int.</returns>
	/// <param name="text">Text.</param>
	/// <param name="defaultValue">Default value.</param>
	public static int TryParseInt (this string text, int defaultValue)
	{
		int.TryParse (text, out defaultValue);
		return defaultValue;
	}
}
