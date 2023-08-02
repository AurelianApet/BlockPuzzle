using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Audio manager.
/// </summary>
[RequireComponent (typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
	public static event Action<bool> OnSoundStatusChangedEvent;
	public static event Action<bool> OnMusicStatusChangedEvent;

	[HideInInspector] public bool isSoundEnabled = true;
	[HideInInspector] public bool isMusicEnabled = true;

	public AudioSource audioSource;
	public AudioClip SFX_ButtonClick;
	public AudioClip SFX_BlockPlace;
	public AudioClip SFX_GameOver;

	private static AudioManager _instance;

	public static AudioManager instance {
		get {
			if (_instance == null) {
				_instance = GameObject.FindObjectOfType<AudioManager> ();
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
		_instance = GameObject.FindObjectOfType<AudioManager> ();
	}

	/// <summary>
	/// Raises the enable event.
	/// </summary>
	void OnEnable ()
	{
		initAudioStatus ();
	}

	/// <summary>
	/// Inits the audio status.
	/// </summary>
	public void initAudioStatus ()
	{
		isSoundEnabled = (PlayerPrefs.GetInt ("isSoundEnabled", 0) == 0) ? true : false;
		isMusicEnabled = (PlayerPrefs.GetInt ("isMusicEnabled", 0) == 0) ? true : false;

		if ((!isSoundEnabled) && (OnSoundStatusChangedEvent != null)) {
			OnSoundStatusChangedEvent.Invoke (isSoundEnabled);
		}
		if ((!isMusicEnabled) && (OnMusicStatusChangedEvent != null)) {
			OnMusicStatusChangedEvent.Invoke (isMusicEnabled);
		}
	}

	/// <summary>
	/// Toggles the sound status.
	/// </summary>
	/// <param name="state">If set to <c>true</c> state.</param>
	public void ToggleSoundStatus (bool state)
	{
		isSoundEnabled = state;
		PlayerPrefs.SetInt ("isSoundEnabled", (isSoundEnabled) ? 0 : 1);

		if (OnSoundStatusChangedEvent != null) {
			OnSoundStatusChangedEvent.Invoke (isSoundEnabled);
		}
	}

	/// <summary>
	/// Toggles the music status.
	/// </summary>
	/// <param name="state">If set to <c>true</c> state.</param>
	public void ToggleMusicStatus (bool state)
	{
		isMusicEnabled = state;
		PlayerPrefs.SetInt ("isMusicEnabled", (isMusicEnabled) ? 0 : 1);

		if (OnMusicStatusChangedEvent != null) {
			OnMusicStatusChangedEvent.Invoke (isMusicEnabled);
		}
	}

	/// <summary>
	/// Plaies the button click sound.
	/// </summary>
	public void PlayButtonClickSound ()
	{
		if (isSoundEnabled) {
			audioSource.PlayOneShot (SFX_ButtonClick);
		}
	}

	/// <summary>
	/// Plaies the one shot clip.
	/// </summary>
	/// <param name="clip">Clip.</param>
	public void PlayOneShotClip (AudioClip clip)
	{
		if (isSoundEnabled) {
			audioSource.PlayOneShot (clip);
		}
	}
}
