using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UM_GameServiceManager : SA_Singleton<UM_GameServiceManager> {
	
	
	public static event Action OnPlayerConnected = delegate {};
	public static event Action OnPlayerDisconnected = delegate {};
	
	public static event Action<UM_LeaderboardResult> ActionScoreSubmitted = delegate {};
	public static event Action<UM_LeaderboardResult> ActionScoresListLoaded = delegate {};
	
	
	public static event Action<UM_Result> ActionFriendsListLoaded = delegate {};
	
	
	
	private bool _IsInitedCalled = false;
	private bool _IsDataLoaded = false;
	
	private int dataEventsCount = 0;
	private int currentEventsCount = 0;
	
	
	private UM_PlayerTemplate _Player = null ;
	private UM_ConnectionState _ConnectionSate = UM_ConnectionState.UNDEFINED;
	
	private static List<string> _FriendsList = new List<string>();
	
	
	//--------------------------------------
	// INITIALIZE
	//--------------------------------------
	
	void Awake() {
		DontDestroyOnLoad(gameObject);
	}
	
	
	private void Init() {
		
		_IsInitedCalled = true;
		
		switch(Application.platform) {
		case RuntimePlatform.IPhonePlayer:
			dataEventsCount = UltimateMobileSettings.Instance.Leaderboards.Count + 1;
			
			
			foreach(UM_Achievement achievment in UltimateMobileSettings.Instance.Achievements) {
				GameCenterManager.RegisterAchievement(achievment.IOSId);
			}
			
			GameCenterManager.OnAuthFinished += OnAuthFinished;
			GameCenterManager.OnAchievementsLoaded += OnGameCenterServiceDataLoaded;
			GameCenterManager.OnLeadrboardInfoLoaded += OnGameCenterServiceLeaderDataLoaded;
			
			GameCenterManager.OnScoreSubmitted += IOS_HandleOnScoreSubmitted;
			GameCenterManager.OnScoresListLoaded  += IOS_HandleOnScoresListLoaded;
			
			GameCenterManager.OnFriendsListLoaded += IOS_OnFriendsListLoaded;
			
			
			break;
		case RuntimePlatform.Android:
			dataEventsCount = 2;
			
			GooglePlayConnection.ActionPlayerConnected += OnAndroidPlayerConnected;
			GooglePlayConnection.ActionPlayerDisconnected += OnAndroidPlayerDisconnected;
			
			GooglePlayManager.ActionAchievementsLoaded += OnGooglePlayServiceDataLoaded;
			GooglePlayManager.ActionLeaderboardsLoaded += OnGooglePlayLeaderDataLoaded;
			
			
			GooglePlayManager.ActionScoreSubmited 		+= Android_HandleActionScoreSubmited;
			GooglePlayManager.ActionScoresListLoaded 	+= Android_HandleActionScoresListLoaded;
			
			
			GooglePlayManager.ActionFriendsListLoaded += Android_ActionFriendsListLoaded;
			
			break;
		}
		
		
		
		
	}
	
	
	
	
	
	
	
	
	
	//--------------------------------------
	// Connection
	//--------------------------------------
	
	public void Connect() {
		
		
		if(!_IsInitedCalled) {
			Init();
		}
		
		if(_ConnectionSate == UM_ConnectionState.CONNECTED || _ConnectionSate == UM_ConnectionState.CONNECTING) {
			return;
		}
		
		switch(Application.platform) {
		case RuntimePlatform.IPhonePlayer:
			GameCenterManager.Init();
			break;
		case RuntimePlatform.Android:
			GooglePlayConnection.Instance.Connect();
			break;
		}
		
		_ConnectionSate = UM_ConnectionState.CONNECTING;
	}
	
	public void Disconnect() {
		switch(Application.platform) {
		case RuntimePlatform.IPhonePlayer:
			
			break;
		case RuntimePlatform.Android:
			GooglePlayConnection.Instance.Disconnect();
			break;
		}
		
	}
	
	//--------------------------------------
	// Friends
	//--------------------------------------
	
	public void LoadFriends() {
		switch(Application.platform) {
		case RuntimePlatform.IPhonePlayer:
			GameCenterManager.RetrieveFriends();
			break;
		case RuntimePlatform.Android:
			GooglePlayManager.Instance.LoadFriends();
			break;
		}
	}
	
	public List<string> FriendsList {
		get {
			return _FriendsList;
		}
	}
	
	public UM_PlayerTemplate GetPlayer(string playerId) {
		
		UM_PlayerTemplate player =  null;
		
		switch(Application.platform) {
		case RuntimePlatform.IPhonePlayer:
			GK_Player gk_player =  GameCenterManager.GetPlayerById(playerId);
			if(gk_player != null) {
				player =  new UM_PlayerTemplate(gk_player, null);
			}
			break;
		case RuntimePlatform.Android:
			GooglePlayerTemplate gp_player = GooglePlayManager.Instance.GetPlayerById(playerId);
			if(gp_player != null) {
				player =  new UM_PlayerTemplate(null, gp_player);
			}
			break;
		}
		
		return player;
		
		
	}
	
	
	//--------------------------------------
	// Achievements
	//--------------------------------------
	
	
	public void ShowAchievementsUI() {
		switch(Application.platform) {
		case RuntimePlatform.IPhonePlayer:
			GameCenterManager.ShowAchievements();
			break;
		case RuntimePlatform.Android:
			GooglePlayManager.Instance.ShowAchievementsUI();
			break;
		}
	}
	
	public void RevealAchievement(string id) {
		RevealAchievement(UltimateMobileSettings.Instance.GetAchievementById(id));
	}
	
	public void RevealAchievement(UM_Achievement achievement) {
		switch(Application.platform) {
			
		case RuntimePlatform.Android:
			GooglePlayManager.Instance.RevealAchievementById(achievement.AndroidId);
			break;
		}
	}
	
	[Obsolete("ReportAchievement is deprecated, please use UnlockAchievement instead.")]
	public void ReportAchievement(string id) {
		UnlockAchievement(id);
	}
	
	[Obsolete("ReportAchievement is deprecated, please use UnlockAchievement instead.")]
	public void ReportAchievement(UM_Achievement achievement) {
		ReportAchievement(achievement);
	}
	
	
	public void UnlockAchievement(string id) {
		UM_Achievement achievement = UltimateMobileSettings.Instance.GetAchievementById(id);
		if (achievement == null) {
			Debug.LogError("Achievment not found with id: " + id);
			return;
		}
		
		UnlockAchievement(achievement);
	}
	
	
	private void UnlockAchievement(UM_Achievement achievement) {
		
		
		switch(Application.platform) {
		case RuntimePlatform.IPhonePlayer:
			GameCenterManager.SubmitAchievement(100f, achievement.IOSId);
			break;
		case RuntimePlatform.Android:
			GooglePlayManager.Instance.UnlockAchievementById(achievement.AndroidId);
			break;
		}
	}
	
	
	public void IncrementAchievement(string id,  float percentages) {
		UM_Achievement achievement = UltimateMobileSettings.Instance.GetAchievementById(id);
		if (achievement == null) {
			Debug.LogError("Achievment not found with id: " + id);
			return;
		}
		
		IncrementAchievement(achievement, percentages);
	}
	
	
	public void IncrementAchievement(UM_Achievement achievement, float percentages) {
		switch(Application.platform) {
		case RuntimePlatform.IPhonePlayer:
			GameCenterManager.SubmitAchievement(percentages, achievement.IOSId);
			break;
		case RuntimePlatform.Android:
			
			GPAchievement a = GooglePlayManager.Instance.GetAchievement(achievement.AndroidId);
			if(a != null) {
				if(a.Type == GPAchievementType.TYPE_INCREMENTAL) {
					int steps = Mathf.CeilToInt(( (float) a.TotalSteps / 100f) * percentages);
					GooglePlayManager.Instance.IncrementAchievementById(achievement.AndroidId, steps - a.CurrentSteps);
				} else {
					GooglePlayManager.Instance.UnlockAchievementById(achievement.AndroidId);
				}
			}
			break;
		}
	}
	
	
	public void ResetAchievements() {
		switch(Application.platform) {
		case RuntimePlatform.IPhonePlayer:
			GameCenterManager.ResetAchievements();
			break;
		case RuntimePlatform.Android:
			GooglePlayManager.Instance.ResetAllAchievements();
			break;
		}
	}
	
	
	public float GetAchievementProgress(string id) {
		UM_Achievement achievement = UltimateMobileSettings.Instance.GetAchievementById(id);
		if (achievement == null) {
			Debug.LogError("Achievment not found with id: " + id);
			return 0f;
		}
		
		return GetAchievementProgress(achievement);
	}
	
	public float GetAchievementProgress(UM_Achievement achievement) {
		if(achievement == null) {
			return 0f;
		}
		
		switch(Application.platform) {
		case RuntimePlatform.IPhonePlayer:
			return GameCenterManager.GetAchievementProgress(achievement.IOSId);
		case RuntimePlatform.Android:
			
			GPAchievement a = GooglePlayManager.Instance.GetAchievement(achievement.AndroidId);
			if(a != null) {
				if(a.Type == GPAchievementType.TYPE_INCREMENTAL) {
					return  ((float)a.CurrentSteps / a.TotalSteps) * 100f;
				} else {
					if(a.State == GPAchievementState.STATE_UNLOCKED) {
						return 100f;
					} else {
						return 0f;
					}
				}
			}
			break;
		}
		
		return 0f;
	}
	
	
	
	//--------------------------------------
	// Leader-Boards
	//--------------------------------------
	
	public UM_Leaderboard GetLeaderboard(string leaderboardId) {
		return UltimateMobileSettings.Instance.GetLeaderboardById(leaderboardId);
	}
	
	
	public void ShowLeaderBoardsUI() {
		switch(Application.platform) {
		case RuntimePlatform.IPhonePlayer:
			GameCenterManager.ShowLeaderboards();
			break;
		case RuntimePlatform.Android:
			GooglePlayManager.Instance.ShowLeaderBoardsUI();
			break;
		}
	}
	
	
	public void ShowLeaderBoardUI(string id) {
		ShowLeaderBoardUI(UltimateMobileSettings.Instance.GetLeaderboardById(id));
	}
	
	public void ShowLeaderBoardUI(UM_Leaderboard leaderboard) {
		if(leaderboard == null) {
			return;
		}
		switch(Application.platform) {
		case RuntimePlatform.IPhonePlayer:
			GameCenterManager.ShowLeaderboard(leaderboard.IOSId);
			break;
		case RuntimePlatform.Android:
			GooglePlayManager.Instance.ShowLeaderBoardById(leaderboard.AndroidId);
			break;
		}
	}
	
	
	public void SubmitScore(string LeaderboardId, long score) {
		SubmitScore(UltimateMobileSettings.Instance.GetLeaderboardById(LeaderboardId), score);
	}
	
	public void SubmitScore(UM_Leaderboard leaderboard, long score) {
		switch(Application.platform) {
		case RuntimePlatform.IPhonePlayer:
			GameCenterManager.ReportScore(score, leaderboard.IOSId);
			break;
		case RuntimePlatform.Android:
			GooglePlayManager.Instance.SubmitScoreById(leaderboard.AndroidId, score);
			break;
		}
	}
	
	
	public UM_Score GetCurrentPlayerScore(string leaderBoardId, UM_TimeSpan timeSpan = UM_TimeSpan.ALL_TIME, UM_CollectionType collection = UM_CollectionType.GLOBAL) {
		return GetCurrentPlayerScore(UltimateMobileSettings.Instance.GetLeaderboardById(leaderBoardId), timeSpan, collection);
	} 
	
	public UM_Score GetCurrentPlayerScore(UM_Leaderboard leaderboard, UM_TimeSpan timeSpan = UM_TimeSpan.ALL_TIME, UM_CollectionType collection = UM_CollectionType.GLOBAL) {
		
		if(leaderboard != null) {
			return leaderboard.GetCurrentPlayerScore(timeSpan, collection);
		}
		
		return null;
	} 
	
	
	public void LoadPlayerCenteredScores(string leaderboardId, int maxResults, UM_TimeSpan timeSpan = UM_TimeSpan.ALL_TIME, UM_CollectionType collection = UM_CollectionType.GLOBAL) {
		UM_Leaderboard leaderboard = UltimateMobileSettings.Instance.GetLeaderboardById(leaderboardId);
		LoadPlayerCenteredScores(leaderboard, maxResults, timeSpan, collection);
	}
	
	public void LoadPlayerCenteredScores(UM_Leaderboard leaderboard, int maxResults, UM_TimeSpan timeSpan = UM_TimeSpan.ALL_TIME, UM_CollectionType collection = UM_CollectionType.GLOBAL) {
		if(leaderboard == null) {
			return;
		}
		
		switch(Application.platform) {
		case RuntimePlatform.IPhonePlayer:
			
			UM_Score score = GetCurrentPlayerScore(leaderboard, timeSpan, collection);
			int rank = 0;
			if(score != null) {
				rank = score.Rank;
			}
			
			int startIndex = Math.Max(0, rank - maxResults / 2);
			GameCenterManager.LoadScore(leaderboard.IOSId, startIndex, maxResults, timeSpan.Get_GK_TimeSpan(), collection.Get_GK_CollectionType());
			
			
			
			break;
		case RuntimePlatform.Android:
			GooglePlayManager.Instance.LoadPlayerCenteredScores(leaderboard.AndroidId, timeSpan.Get_GP_TimeSpan(), collection.Get_GP_CollectionType(), maxResults);
			break;
			
		}
	}
	
	
	public void LoadTopScores(string leaderboardId, int maxResults, UM_TimeSpan timeSpan = UM_TimeSpan.ALL_TIME, UM_CollectionType collection = UM_CollectionType.GLOBAL) {
		UM_Leaderboard leaderboard = UltimateMobileSettings.Instance.GetLeaderboardById(leaderboardId);
		LoadTopScores(leaderboard, maxResults, timeSpan, collection);
	}
	
	public void LoadTopScores(UM_Leaderboard leaderboard, int maxResults, UM_TimeSpan timeSpan = UM_TimeSpan.ALL_TIME, UM_CollectionType collection = UM_CollectionType.GLOBAL) {
		
		if(leaderboard == null) {
			return;
		}
		
		switch(Application.platform) {
		case RuntimePlatform.IPhonePlayer:
			GameCenterManager.LoadScore(leaderboard.IOSId, 1, maxResults, timeSpan.Get_GK_TimeSpan(), collection.Get_GK_CollectionType());
			break;
		case RuntimePlatform.Android:
			GooglePlayManager.Instance.LoadTopScores(leaderboard.AndroidId, timeSpan.Get_GP_TimeSpan(), collection.Get_GP_CollectionType(), maxResults);
			break;
			
		}
	}
	
	
	
	
	//--------------------------------------
	// Get / Set
	//--------------------------------------
	
	
	public UM_ConnectionState ConnectionSate {
		get {
			return _ConnectionSate;
		}
	}
	
	public bool IsConnected {
		get {
			return ConnectionSate == UM_ConnectionState.CONNECTED;
		}
	}
	
	[System.Obsolete("player is deprectaed, plase use Player instead")]
	public UM_PlayerTemplate player {
		get {
			return _Player;
		}
	}
	
	public UM_PlayerTemplate Player {
		get {
			return _Player;
		}
	}
	
	
	
	//--------------------------------------
	// Events
	//--------------------------------------
	
	private void OnServiceConnected() {
		
		if(_IsDataLoaded) {
			OnAllLoaded();
			return;
		}
		
		
		switch(Application.platform) {
		case RuntimePlatform.IPhonePlayer:
			foreach(UM_Leaderboard leaderboard in UltimateMobileSettings.Instance.Leaderboards) {
				GameCenterManager.LoadLeaderboardInfo(leaderboard.IOSId);
			}
			break;
		case RuntimePlatform.Android:
			GooglePlayManager.Instance.LoadAchievements();
			GooglePlayManager.Instance.LoadLeaderBoards();
			break;
		}
	}
	
	private void OnGooglePlayServiceDataLoaded(GooglePlayResult result) {
		if (result.isSuccess) {
			currentEventsCount++;
			if(currentEventsCount == dataEventsCount) {
				OnAllLoaded();
			}
		}		
	}
	
	
	void OnGooglePlayLeaderDataLoaded (GooglePlayResult res) {
		
		foreach(GPLeaderBoard lb in GooglePlayManager.Instance.LeaderBoards)  {
			UM_Leaderboard leaderboard =  UltimateMobileSettings.Instance.GetLeaderboardByAndroidId(lb.Id);
			if(leaderboard != null) {
				leaderboard.Setup(lb);
			}
		}
		
		OnGooglePlayServiceDataLoaded(res);
		
	}
	
	
	private void OnGameCenterServiceDataLoaded(ISN_Result e) {
		//if (e.IsSucceeded) {
		currentEventsCount++;
		if(currentEventsCount == dataEventsCount) {
			OnAllLoaded();
		}
		//}		
	}
	
	private void OnGameCenterServiceLeaderDataLoaded(GK_LeaderboardResult res) {
		if (res.IsSucceeded && res.Leaderboard != null) {
			UM_Leaderboard leaderboard =  UltimateMobileSettings.Instance.GetLeaderboardByIOSId(res.Leaderboard.Id);
			if(leaderboard != null) {
				leaderboard.Setup(res.Leaderboard);
			}
		}
		
		OnGameCenterServiceDataLoaded(res);
	}
	
	
	
	private void OnAllLoaded() {
		_IsDataLoaded = true;
		_ConnectionSate = UM_ConnectionState.CONNECTED;
		_Player =  new UM_PlayerTemplate(GameCenterManager.Player, GooglePlayManager.Instance.player);
		
		
		OnPlayerConnected();
	}
	
	
	//--------------------------------------
	// IOS Events
	//--------------------------------------
	
	
	
	private void OnAuthFinished (ISN_Result res) {
		if(res.IsSucceeded) {
			OnServiceConnected();
		} else {
			_ConnectionSate = UM_ConnectionState.DISCONNECTED;
			OnPlayerDisconnected();
		}
	}
	
	
	void IOS_HandleOnScoreSubmitted (GK_LeaderboardResult res) {
		
		UM_Leaderboard leaderboard =  UltimateMobileSettings.Instance.GetLeaderboardByIOSId(res.Leaderboard.Id);
		if(leaderboard != null) {
			leaderboard.Setup(res.Leaderboard);
			
			UM_LeaderboardResult result =  new UM_LeaderboardResult(leaderboard, res);
			ActionScoreSubmitted(result);
		}
		
		
	}
	
	
	void IOS_HandleOnScoresListLoaded (GK_LeaderboardResult res) {
		UM_Leaderboard leaderboard =  UltimateMobileSettings.Instance.GetLeaderboardByIOSId(res.Leaderboard.Id);
		if(leaderboard != null) {
			leaderboard.Setup(res.Leaderboard);
			
			UM_LeaderboardResult result =  new UM_LeaderboardResult(leaderboard, res);
			ActionScoresListLoaded(result);
		}
	}
	
	void IOS_OnFriendsListLoaded (ISN_Result res) {
		_FriendsList = GameCenterManager.FriendsList;
		ActionFriendsListLoaded( new UM_Result(res));
	}
	
	
	//--------------------------------------
	// Android Events
	//--------------------------------------
	
	private void OnAndroidPlayerConnected() {
		OnServiceConnected();
	}
	
	private void OnAndroidPlayerDisconnected() {
		_ConnectionSate = UM_ConnectionState.DISCONNECTED;
		OnPlayerDisconnected();
	}
	
	
	void Android_HandleActionScoresListLoaded (GP_LeaderboardResult res) {
		UM_Leaderboard leaderboard =  UltimateMobileSettings.Instance.GetLeaderboardByAndroidId(res.Leaderboard.Id);
		if(leaderboard != null) {
			leaderboard.Setup(res.Leaderboard);
			
			UM_LeaderboardResult result =  new UM_LeaderboardResult(leaderboard, res);
			ActionScoreSubmitted(result);
		}
	}
	
	
	
	void Android_HandleActionScoreSubmited (GP_LeaderboardResult res) {
		UM_Leaderboard leaderboard =  UltimateMobileSettings.Instance.GetLeaderboardByAndroidId(res.Leaderboard.Id);
		if(leaderboard != null) {
			leaderboard.Setup(res.Leaderboard);
			
			UM_LeaderboardResult result =  new UM_LeaderboardResult(leaderboard, res);
			ActionScoreSubmitted(result);
		}
	}
	
	
	void Android_ActionFriendsListLoaded (GooglePlayResult res) {
		_FriendsList = GooglePlayManager.Instance.friendsList;
		ActionFriendsListLoaded(new UM_Result(res));
	}
	
	//--------------------------------------
	// Utils
	//--------------------------------------
	
	
	
}
