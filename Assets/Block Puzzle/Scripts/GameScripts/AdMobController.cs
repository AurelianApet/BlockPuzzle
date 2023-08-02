using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;
using System;

public enum AdType {
	Interstitial,
	Video,
	Incentivized,
	Banner
}
	
public class AdMobController : MonoBehaviour 
{
	#if UNITY_ANDROID || UNITY_IOS
	public static AdMobController instance;
	private GoogleMobileAdBanner banner1;
	private AdType _selectedAdType;

	void Awake()
	{
		if (instance != null) 
		{
			Destroy(gameObject);
			return;
		}
		instance = this;
	}

	void Start()
	{
		init ();
	}

	public void init()
	{
		GoogleMobileAd.Init();
		GoogleMobileAd.TagForChildDirectedTreatment(false);
	}

	public void FetchAds(AdType adType) {
		switch(adType) {
		case AdType.Interstitial:
			GoogleMobileAd.LoadInterstitialAd();
			break;
		case AdType.Video:
			break;
		case AdType.Incentivized:
			break;
		}
	}

	public bool isAdAvailable(AdType adType) {
		bool available = false;

		switch (adType) {
		case AdType.Interstitial:
			return GoogleMobileAd.IsInterstitialReady;
			break;
		case AdType.Video:
			break;
		case AdType.Incentivized:
			break;
		}
		return available;
	}
	
	public void ShowAds(AdType adType)
	{
		switch (adType) {
		case AdType.Interstitial:
			GoogleMobileAd.StartInterstitialAd ();
			break;
		case AdType.Video:
			break;
		case AdType.Incentivized:
			break;
		case AdType.Banner:
			banner1 = GoogleMobileAd.CreateAdBanner(TextAnchor.LowerCenter, GADBannerSize.SMART_BANNER);
			banner1.ShowOnLoad = true;
			break;
		}
	}
	#endif
}
