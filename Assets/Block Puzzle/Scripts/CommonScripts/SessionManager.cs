using UnityEngine;
using System.Collections;
using System;

public class SessionManager : MonoBehaviour 
{
	public static SessionManager instance;
	public static int currentsessioncount = 0;
	public static event Action<int> OnSessionChangedEvent;


	void Awake()
	{
		if (instance == null) {
			instance = this;
			return;
		}
		Destroy (gameObject);
	}


	void Start() {
		UpdateSession ();
	}

	void OnApplicatioPause(bool pause)
	{
		if (!pause) {
			UpdateSession ();
		}

		Application.targetFrameRate = 60;
	}


	void OnApplicationFocus (bool focus)
	{
		Application.targetFrameRate = 60;
	}

	public void UpdateSession()
	{
		currentsessioncount = PlayerPrefs.GetInt ("currentsessioncount", 0);
		currentsessioncount++;
		PlayerPrefs.SetInt ("currentsessioncount", currentsessioncount);

		if (OnSessionChangedEvent != null) {
			OnSessionChangedEvent (currentsessioncount);
		}
	}
}
