using UnityEngine;
using System.Collections;

public class SettingsContent : MonoBehaviour {

	public Transform settings_main;
	public Transform settings_gameplay;

	public static SettingsContent instance;

	void Awake()
	{
		if (instance == null) {
			instance = this;
			return;
		}
		Destroy (gameObject);
	}
}
