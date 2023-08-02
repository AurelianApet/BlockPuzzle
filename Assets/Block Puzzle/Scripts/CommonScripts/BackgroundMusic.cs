using UnityEngine;
using System.Collections;

[RequireComponent (typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake ()
	{
		//Check whether the music is enable or not.
		if ((PlayerPrefs.GetInt ("isMusicEnabled", 0) == 0)) {
			GetComponent<AudioSource> ().Play ();
		}
	}

	/// <summary>
	/// Registers the event for music status change.
	/// </summary>
	void OnEnable ()
	{
		AudioManager.OnMusicStatusChangedEvent += AudioManager_OnMusicStatusChangedEvent;		
	}

	/// <summary>
	/// Unregisters the event for music status change.
	/// </summary>
	void OnDisable ()
	{
		AudioManager.OnMusicStatusChangedEvent -= AudioManager_OnMusicStatusChangedEvent;		
	}

	/// <summary>
	/// Update the background music status based on changes state.
	/// </summary>
	/// <param name="status">If set to <c>true</c> status.</param>
	void AudioManager_OnMusicStatusChangedEvent (bool status)
	{
		if (status) {
			GetComponent<AudioSource> ().Play ();
		} else {
			GetComponent<AudioSource> ().Stop ();
		}
	}
}
