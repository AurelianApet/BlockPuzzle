using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

[CustomEditor(typeof(UltimateMobileSettings))]
public class UMSettingEditor : Editor {
	
	
	GUIContent SdkVersion   = new GUIContent("Plugin Version [?]", "This is Plugin version.  If you have problems or compliments please include this so we know exactly what version to look out for.");
	GUIContent SupportEmail = new GUIContent("Support [?]", "If you have any technical quastion, feel free to drop an e-mail");
	
	GUIContent AutoLoadSmallImagesLoadTitle  = new GUIContent("Autoload Small Player Photo[?]:", "As soon as player info received, small player photo will be requested automatically");
	GUIContent AutoLoadBigmagesLoadTitle  = new GUIContent("Autoload Big Player Photo[?]:", "As soon as player info received, big player photo will be requested automatically");

	
	private UltimateMobileSettings settings;
	
	
	void Awake() {
		
		
		if(IsInstalled && IsUpToDate) {
			AndroidNativeSettingsEditor.UpdateManifest();
		}
		
		#if !UNITY_WEBPLAYER
		UpdatePluginSettings();
		#endif
		
	}
	
	
	public override void OnInspectorGUI() {
		settings = UltimateMobileSettings.Instance;
		
		GUI.changed = false;
		
		
		
		//EditorGUILayout.HelpBox("Ultimate Mobile Pluging", MessageType.None);
		
		GeneralOptions();
		//EditorGUILayout.Space();
		EditorGUILayout.HelpBox("Unified APi Settings", MessageType.None);
		InAppSettings();
		EditorGUILayout.Space();
		GameServiceSettings();
		EditorGUILayout.Space();
		AdSettings();
		EditorGUILayout.Space();
		OtherSettings();
		EditorGUILayout.Space();
		AboutGUI();
		
		
		
		if(GUI.changed) {
			DirtyEditor();
		}
	}
	
	
	GUIContent LID = new GUIContent("Leaderboard Id[?]:", "Uniquie Leaderboard Id");
	GUIContent IOSLID = new GUIContent("IOS Leaderboard Id[?]:", "IOS Leaderboard Id");
	GUIContent ANDROIDLID = new GUIContent("Android Leaderboard Id[?]:", "Android Leaderboard Id");
	
	
	
	GUIContent AID = new GUIContent("Achievement Id[?]:", "Uniquie Leaderboard Id");
	GUIContent ALID = new GUIContent("IOS Achievement Id[?]:", "IOS Leaderboard Id");
	GUIContent ANDROIDAID = new GUIContent("Android Achievement Id[?]:", "Android Leaderboard Id");
	
	
	GUIContent Base64KeyLabel = new GUIContent("Base64 Key[?]:", "Base64 Key app key.");
	GUIContent InAppID = new GUIContent("ProductId[?]:", "UniquieProductId");
	GUIContent IsCons = new GUIContent("Is Consumable[?]:", "Is prodcut allowed to be purchased more than once?");
	
	GUIContent IOSSKU = new GUIContent("IOS SKU[?]:", "IOS SKU");
	GUIContent AndroidSKU = new GUIContent("Android SKU[?]:", "Android SKU");
	GUIContent WP8SKU = new GUIContent("WP8 SKU[?]:", "WP8 SKU");
	GUIContent DefaultPrice = new GUIContent("Default Price[?] :", "This Price is used, when Product Templates are NOT loaded and there is NO Internet connection");
	
	
	private const string version_info_file = "Plugins/StansAssets/Versions/UM_VersionInfo.txt"; 
	private const string ios_install_mark_file = PluginsInstalationUtil.IOS_DESTANATION_PATH + "UM_IOS_INSTALATION_MARK.txt";
	
	
	public static bool IsInstalled {
		get {
			
			if(FileStaticAPI.IsFileExists(PluginsInstalationUtil.ANDROID_DESTANATION_PATH + "androidnative.jar") && FileStaticAPI.IsFileExists(ios_install_mark_file)) {
				return true;
			} else {
				return false;
			}
			
		}
	}
	
	public static bool IsUpToDate {
		get {
			if(UltimateMobileSettings.VERSION_NUMBER.Equals(DataVersion)) {
				return true;
			} else {
				return false;
			}
		}
	}

	public static int CurrentVersion {
		get {
			return SA_VersionsManager.ParceMagorVersion(UltimateMobileSettings.VERSION_NUMBER);
		}
	}
	
	public static string DataVersion {
		get {
			if(FileStaticAPI.IsFileExists(version_info_file)) {
				return FileStaticAPI.Read(version_info_file);
			} else {
				return "Unknown";
			}
		}
	}
	
	public static void UpdatePluginVersion() {
		
		PluginsInstalationUtil.Android_InstallPlugin();
		PluginsInstalationUtil.IOS_InstallPlugin();
		UpdateGoogleAdIOSAPI();
		
		
		FileStaticAPI.Write(version_info_file, UltimateMobileSettings.VERSION_NUMBER);
		AndroidNativeSettingsEditor.UpdateVersionInfo();
		GoogleMobileAdSettingsEditor.UpdateVersionInfo ();
		IOSNativeSettingsEditor.UpdateVersionInfo ();
		
	}
	
	
	
	private void GeneralOptions() {
		
		if(!IsInstalled) {
			EditorGUILayout.HelpBox("Install Required ", MessageType.Error);
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			Color c = GUI.color;
			GUI.color = Color.cyan;
			if(GUILayout.Button("Install Plugin",  GUILayout.Width(120))) {
				
				FileStaticAPI.CreateFile(ios_install_mark_file);
				UpdatePluginVersion();
			}
			GUI.color = c;
			EditorGUILayout.EndHorizontal();
		}
		
		if(IsInstalled) {
			if(!IsUpToDate) {
				EditorGUILayout.HelpBox("Update Required \nResources version: " + SA_VersionsManager.UM_StringVersionId + " Plugin version: " + UltimateMobileSettings.VERSION_NUMBER, MessageType.Warning);
				
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				Color c = GUI.color;
				GUI.color = Color.cyan;


				if(CurrentVersion != SA_VersionsManager.UM_MagorVersion) {
					if(GUILayout.Button("How to update",  GUILayout.Width(250))) {
						Application.OpenURL("https://goo.gl/Csu3zQ");
					}
				} else {
					if(GUILayout.Button("Upgrade Resources",  GUILayout.Width(250))) {
						UpdatePluginVersion();
					}
				}
				
				GUI.color = c;
				EditorGUILayout.Space();
				EditorGUILayout.EndHorizontal();

				Actions();				
			} else {
				EditorGUILayout.HelpBox("Ultimate Mobile Plugin v" + UltimateMobileSettings.VERSION_NUMBER + " is installed", MessageType.Info);
				PluginSettings();
			}
		}
		
		
		EditorGUILayout.Space();
		
	}

	private void PluginSettings() {
		EditorGUILayout.Space();		
		settings.IsMoreSettingsOpen = EditorGUILayout.Foldout(settings.IsMoreSettingsOpen, "More Settings");
		if(settings.IsMoreSettingsOpen) {
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			
			if(GUILayout.Button("Android Native Settings ",  GUILayout.Width(140))) {
				Selection.activeObject = AndroidNativeSettings.Instance;
			}
			
			if(GUILayout.Button("IOS Native Settings ",  GUILayout.Width(140))) {
				Selection.activeObject = IOSNativeSettings.Instance;
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			
			
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			if(GUILayout.Button("Analytics Settings ",  GUILayout.Width(140))) {
				Selection.activeObject = GoogleAnalyticsSettings.Instance;
			}
			
			if(GUILayout.Button("Google Ad Settings ",  GUILayout.Width(140))) {
				Selection.activeObject = GoogleMobileAdSettings.Instance;
			}
			
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			
			
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}

		Actions();
	}
	
	private void Actions() {
		settings.IsMoreActionsOpen = EditorGUILayout.Foldout(settings.IsMoreActionsOpen, "More Actions");
		if(settings.IsMoreActionsOpen) {
			
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			
			if(GUILayout.Button("Open Manifest ",  GUILayout.Width(140))) {
				UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal("Assets" + AN_ManifestManager.MANIFEST_FILE_PATH, 1);
			}
			
			if(GUILayout.Button("Reinstall ",  GUILayout.Width(140))) {
				UpdatePluginVersion();
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			
			if(GUILayout.Button("Load Example Settings",  GUILayout.Width(140))) {
				
				UltimateMobileSettings.Instance.Leaderboards.Clear();
				
				UM_Leaderboard lb = new UM_Leaderboard();
				lb.id = "LeaderBoardSample_1";
				lb.AndroidId = "CgkIipfs2qcGEAIQAA";
				UltimateMobileSettings.Instance.Leaderboards.Add(lb);
				
				
				lb = new UM_Leaderboard();
				lb.id = "LeaderBoardSample_2";
				lb.AndroidId = "CgkIipfs2qcGEAIQFQ";
				UltimateMobileSettings.Instance.Leaderboards.Add(lb);
				
				
				settings.InAppProducts.Clear();
				
				UM_InAppProduct p;
				
				p =  new UM_InAppProduct();
				p.id = "coins_bonus";
				p.IOSId = "purchase.example.coins_bonus";
				p.AndroidId = "coins_bonus";
				p.IsConsumable = false;
				
				settings.AddProduct(p);
				
				
				p =  new UM_InAppProduct();
				p.id = "coins_pack";
				p.IOSId = "purchase.example.small_coins_bag";
				p.AndroidId = "pm_coins";
				p.IsConsumable = true;
				
				settings.AddProduct(p);
				
				
				#if UNITY_IOS || UNITY_IPHONE
				
				PlayerSettings.bundleIdentifier = "com.iosnative.preview";
				
				
				#endif
				
				#if UNITY_ANDROID
				
				PlayerSettings.bundleIdentifier = "com.unionassets.android.plugin.preview";
				
				
				#endif
				
				AndroidNativeSettingsEditor.LoadExampleSettings();				
			}		

			if(GUILayout.Button("Remove",  GUILayout.Width(140))) {
				SA_RemoveTool.RemovePlugins();				
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}
	}
	
	
	public static void UpdateGoogleAdIOSAPI(bool forseDisable = false) {
		
		bool IsEnabled = false; 
		
		if(!forseDisable) {
			if(UltimateMobileSettings.Instance.IOSAdEdngine == UM_IOSAdEngineOprions.GoogleMobileAd) {
				IsEnabled = true;
			}
		}
		
		
		
		
		
		string IOSADBannerContent = FileStaticAPI.Read("Extensions/GoogleMobileAd/Scripts/IOS/IOSADBanner.cs");
		string IOSAdMobControllerContent = FileStaticAPI.Read("Extensions/GoogleMobileAd/Scripts/IOS/IOSAdMobController.cs");
		string GoogleMobileAdPostProcessContent = FileStaticAPI.Read("Extensions/GoogleMobileAd/Scripts/Editor/GoogleMobileAdPostProcess.cs");
		string ScarletPostProcessorContent = FileStaticAPI.Read("Extensions/GoogleMobileAd/Scripts/Editor/ScarletPostProcessor.cs");


		
		if(IsEnabled)  {
			IOSADBannerContent = IOSADBannerContent.Replace("#define CODE_DISABLED", "//#define CODE_DISABLED");
			IOSAdMobControllerContent = IOSAdMobControllerContent.Replace("#define CODE_DISABLED", "//#define CODE_DISABLED");
			GoogleMobileAdPostProcessContent = GoogleMobileAdPostProcessContent.Replace("#define CODE_DISABLED", "//#define CODE_DISABLED");
			ScarletPostProcessorContent = ScarletPostProcessorContent.Replace("#define CODE_DISABLED", "//#define CODE_DISABLED");

			PluginsInstalationUtil.InstallGMAPart();

		} else {
			IOSADBannerContent = IOSADBannerContent.Replace("//#define CODE_DISABLED", "#define CODE_DISABLED");
			IOSAdMobControllerContent = IOSAdMobControllerContent.Replace("//#define CODE_DISABLED", "#define CODE_DISABLED");
			GoogleMobileAdPostProcessContent = GoogleMobileAdPostProcessContent.Replace("//#define CODE_DISABLED", "#define CODE_DISABLED");
			ScarletPostProcessorContent = ScarletPostProcessorContent.Replace("//#define CODE_DISABLED", "#define CODE_DISABLED");


			FileStaticAPI.DeleteFile(PluginsInstalationUtil.IOS_DESTANATION_PATH + "GMA_SA_Lib_Proxy.mm");
			FileStaticAPI.DeleteFile(PluginsInstalationUtil.IOS_DESTANATION_PATH + "GMA_SA_Lib.h");
			FileStaticAPI.DeleteFile(PluginsInstalationUtil.IOS_DESTANATION_PATH + "GMA_SA_Lib.m");
		}
		
		FileStaticAPI.Write("Extensions/GoogleMobileAd/Scripts/IOS/IOSADBanner.cs", IOSADBannerContent);
		FileStaticAPI.Write("Extensions/GoogleMobileAd/Scripts/IOS/IOSAdMobController.cs", IOSAdMobControllerContent);
		FileStaticAPI.Write("Extensions/GoogleMobileAd/Scripts/Editor/GoogleMobileAdPostProcess.cs", GoogleMobileAdPostProcessContent);
		FileStaticAPI.Write("Extensions/GoogleMobileAd/Scripts/Editor/ScarletPostProcessor.cs", ScarletPostProcessorContent);
		


	}
	
	
	private string GetKeyForValue(Dictionary<string, string> dictionary, string value) {
		foreach (KeyValuePair<string, string> pair in dictionary) {
			if (pair.Value.Equals(value)) {
				return pair.Key;
			}
		}
		return string.Empty;
	}

	GUIContent LeaderboardDescriptionLabel 	= new GUIContent("Description[?]:", "This is the description of the Leaderboard. The description cannot be longer than 255 bytes.");
	GUIContent AchievementDescriptionLabel 	= new GUIContent("Description[?]:", "This is the description of the Achievement. The description cannot be longer than 255 bytes.");

	private GK_AchievementTemplate GetIOSAchievement(string id) {
		foreach (GK_AchievementTemplate ach in IOSNativeSettings.Instance.Achievements) {
			if (ach.Id.Equals(id)) {
				return ach;
			}
		}
		GK_AchievementTemplate a = new GK_AchievementTemplate(){Id = id};
		IOSNativeSettings.Instance.Achievements.Add(a);
		return a;
	}

	private GPAchievement GetAndroidAchievement(string id) {
		foreach (GPAchievement ach in AndroidNativeSettings.Instance.Achievements) {
			if (ach.Id.Equals(id)) {
				return ach;
			}
		}
		GPAchievement a = new GPAchievement(id, string.Empty);
		AndroidNativeSettings.Instance.Achievements.Add(a);
		return a;
	}

	private GK_Leaderboard GetIOSLeaderboard(string id) {
		foreach (GK_Leaderboard lb in IOSNativeSettings.Instance.Leaderboards) {
			if (lb.Id.Equals(id)) {
				return lb;
			}
		}
		GK_Leaderboard l = new GK_Leaderboard(id);
		IOSNativeSettings.Instance.Leaderboards.Add(l);
		return l;
	}
	
	private GPLeaderBoard GetAndroidLeaderboard(string id) {
		foreach (GPLeaderBoard lb in AndroidNativeSettings.Instance.Leaderboards) {
			if (lb.Id.Equals(id)) {
				return lb;
			}
		}
		GPLeaderBoard l = new GPLeaderBoard(id, string.Empty);
		AndroidNativeSettings.Instance.Leaderboards.Add(l);
		return l;
	}

	private bool DrawSortingButtons(object currentObject, IList ObjectsList,
	                                object androidObject, IList androidObjectsList,
	                                object iosObject, IList iosObjectsList) {
		
		int ObjectIndex = ObjectsList.IndexOf(currentObject);
		if(ObjectIndex == 0) {
			GUI.enabled = false;
		} 
		
		bool up 		= GUILayout.Button("↑", EditorStyles.miniButtonLeft, GUILayout.Width(20));
		if(up) {
			object c = currentObject;
			ObjectsList[ObjectIndex]  		= ObjectsList[ObjectIndex - 1];
			ObjectsList[ObjectIndex - 1] 	=  c;

			c = androidObject;
			androidObjectsList[ObjectIndex]  		= androidObjectsList[ObjectIndex - 1];
			androidObjectsList[ObjectIndex - 1] 	=  c;

			c = iosObject;
			iosObjectsList[ObjectIndex]  		= iosObjectsList[ObjectIndex - 1];
			iosObjectsList[ObjectIndex - 1] 	=  c;
		}
		
		
		if(ObjectIndex >= ObjectsList.Count -1) {
			GUI.enabled = false;
		} else {
			GUI.enabled = true;
		}
		
		bool down 		= GUILayout.Button("↓", EditorStyles.miniButtonMid, GUILayout.Width(20));
		if(down) {
			object c = currentObject;
			ObjectsList[ObjectIndex] =  ObjectsList[ObjectIndex + 1];
			ObjectsList[ObjectIndex + 1] = c;

			c = androidObject;
			androidObjectsList[ObjectIndex] =  androidObjectsList[ObjectIndex + 1];
			androidObjectsList[ObjectIndex + 1] = c;

			c = iosObject;
			iosObjectsList[ObjectIndex] =  iosObjectsList[ObjectIndex + 1];
			iosObjectsList[ObjectIndex + 1] = c;
		}
		
		
		GUI.enabled = true;
		bool r 			= GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(20));
		if(r) {
			ObjectsList.Remove(currentObject);
			androidObjectsList.Remove(androidObject);
			iosObjectsList.Remove(iosObject);
		}
		
		return r;
	}

	private void GameServiceSettings() {
		
		EditorGUI.indentLevel = 0;
		settings.IsGameServiceOpen = EditorGUILayout.Foldout(settings.IsGameServiceOpen, "Game Service");
		if(settings.IsGameServiceOpen) {
			EditorGUI.indentLevel++; {
				
				Dictionary<string, string> resources = new Dictionary<string, string>();
				if (FileStaticAPI.IsFileExists ("Plugins/Android/res/values/ids.xml")) {
					//Parse XML file with PlayService Settings ID's
					XmlDocument doc = new XmlDocument();
					doc.Load(Application.dataPath + "/Plugins/Android/res/values/ids.xml");
					
					XmlNode rootResourcesNode = doc.DocumentElement;						
					foreach(XmlNode chn in rootResourcesNode.ChildNodes) {
						if (chn.Name.Equals("string")) {
							if (chn.Attributes["name"] != null) {
								if (!chn.Attributes["name"].Value.Equals("app_id")) {
									resources.Add(chn.Attributes["name"].Value, chn.InnerText);
								}
							}
						}
					}
				}
				
				settings.IsLeaderBoardsOpen = EditorGUILayout.Foldout(settings.IsLeaderBoardsOpen, "Leaderboards");
				if(settings.IsLeaderBoardsOpen) {
					if(settings.Leaderboards.Count == 0) {
						EditorGUILayout.HelpBox("No Leaderboards Added", MessageType.Warning);
					}					
					foreach(UM_Leaderboard leaderbaord in settings.Leaderboards) {
						GPLeaderBoard gpLb = GetAndroidLeaderboard(leaderbaord.AndroidId);
						GK_Leaderboard gkLb = GetIOSLeaderboard(leaderbaord.IOSId);

						EditorGUILayout.BeginVertical (GUI.skin.box);
						EditorGUILayout.BeginHorizontal();

						GUIStyle s =  new GUIStyle();
						s.padding =  new RectOffset();
						s.margin =  new RectOffset();
						s.border =  new RectOffset();
						
						if(leaderbaord.Texture != null) {
							GUILayout.Box(leaderbaord.Texture, s, new GUILayoutOption[]{GUILayout.Width(18), GUILayout.Height(18)});
						}

						leaderbaord.IsOpen = EditorGUILayout.Foldout(leaderbaord.IsOpen, leaderbaord.id);
						bool ItemWasRemoved = DrawSortingButtons((object) leaderbaord, settings.Leaderboards,
						                                         (object) gpLb, AndroidNativeSettings.Instance.Leaderboards,
						                                         (object) gkLb, IOSNativeSettings.Instance.Leaderboards);
						if(ItemWasRemoved) {
							return;
						}
						EditorGUILayout.EndHorizontal();

						if(leaderbaord.IsOpen) {
							EditorGUI.indentLevel++; {
								
								EditorGUILayout.BeginHorizontal();
								EditorGUILayout.LabelField(LID);
								leaderbaord.id	 	= EditorGUILayout.TextField(leaderbaord.id);
								if(leaderbaord.id.Length > 0) {
									leaderbaord.id		= leaderbaord.id.Trim();
								}
								gkLb.Info.Title = leaderbaord.id;
								gpLb.Name = leaderbaord.id;
								EditorGUILayout.EndHorizontal();
								
								EditorGUILayout.BeginHorizontal();
								EditorGUILayout.LabelField(IOSLID);
								leaderbaord.IOSId	 	= EditorGUILayout.TextField(leaderbaord.IOSId);
								if(leaderbaord.IOSId.Length > 0) {
									leaderbaord.IOSId 		= leaderbaord.IOSId.Trim();
								}
								EditorGUILayout.EndHorizontal();
								gkLb.Info.Identifier = leaderbaord.IOSId;
								
								EditorGUILayout.BeginHorizontal();
								EditorGUILayout.LabelField(ANDROIDLID);
								
								EditorGUI.BeginChangeCheck();
								
								bool doesntMatch = false;
								bool fileDoesntExists = false;
								
								string name = string.Empty;
								string[] names = new string[resources.Keys.Count + 1];
								names[0] = "[None]";
								resources.Keys.CopyTo(names, 1);
								List<string> listNames = new List<string>(names);
								if (leaderbaord.AndroidId.Equals(string.Empty)) {
									name = names[EditorGUILayout.Popup(0, names)];
								} else {
									if (FileStaticAPI.IsFileExists ("Plugins/Android/res/values/ids.xml")) {
										if (resources.ContainsValue(leaderbaord.AndroidId)) {
											name = names[EditorGUILayout.Popup(listNames.IndexOf(GetKeyForValue(resources, leaderbaord.AndroidId)), names)];
										} else {
											doesntMatch = true;
											name = names[EditorGUILayout.Popup(0, names)];
										}
									} else {
										fileDoesntExists = true;
									}
								}
								
								if (EditorGUI.EndChangeCheck()){
									if (!name.Equals("[None]")) {
										leaderbaord.AndroidId = resources[name];
									}
								}
								
								if(leaderbaord.AndroidId.Length > 0) {
									leaderbaord.AndroidId 		= leaderbaord.AndroidId.Trim();
								}
								gpLb.Id = leaderbaord.AndroidId;
								EditorGUILayout.EndHorizontal();
								
								EditorGUILayout.BeginHorizontal();
								if (fileDoesntExists) {
									EditorGUILayout.HelpBox("XML file with PlayService ID's DOESN'T exist", MessageType.Warning);
								}
								if (doesntMatch) {
									EditorGUILayout.HelpBox("Leaderboard ID doesn't match any PlayService ID of ids.xml file", MessageType.Warning);
								}
								EditorGUILayout.EndHorizontal();
								
								EditorGUILayout.Space();
								EditorGUILayout.Space();
								
								EditorGUILayout.BeginHorizontal();
								EditorGUILayout.LabelField(LeaderboardDescriptionLabel);
								EditorGUILayout.EndHorizontal();
								
								EditorGUILayout.BeginHorizontal();
								leaderbaord.Description	 = EditorGUILayout.TextArea(leaderbaord.Description,  new GUILayoutOption[]{GUILayout.Height(60), GUILayout.Width(200)} );
								gkLb.Info.Description = leaderbaord.Description;
								gpLb.Description = leaderbaord.Description;
								leaderbaord.Texture = (Texture2D) EditorGUILayout.ObjectField("", leaderbaord.Texture, typeof (Texture2D), false);
								gkLb.Info.Texture = leaderbaord.Texture;
								gpLb.Texture = leaderbaord.Texture;
								EditorGUILayout.EndHorizontal();
								EditorGUILayout.Space();
							} EditorGUI.indentLevel--;
						}
						EditorGUILayout.EndVertical();
					}
					
					
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.Space();
					if(GUILayout.Button("Add new",  GUILayout.Width(80))) {
						UM_Leaderboard lb = new UM_Leaderboard();

						int index = 0;
						do {
							index++;
						} while (IsLeaderboardExists(lb.id + index.ToString()));
						lb.id = lb.id + index.ToString();
						lb.AndroidId = lb.id;
						lb.IOSId = lb.id;

						settings.AddLeaderboard(lb);
						AndroidNativeSettings.Instance.Leaderboards.Add(new GPLeaderBoard(lb.AndroidId, lb.id));
						GK_Leaderboard iOSLb = new GK_Leaderboard(lb.IOSId);
						iOSLb.Info.Title = lb.id;
						IOSNativeSettings.Instance.Leaderboards.Add(iOSLb);
					}
					EditorGUILayout.Space();
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.Space();
				}
				
				settings.IsAchievementsOpen = EditorGUILayout.Foldout(settings.IsAchievementsOpen, "Achievements");
				if(settings.IsAchievementsOpen) {
					if(settings.Achievements.Count == 0) {
						EditorGUILayout.HelpBox("No Achievements Added", MessageType.Warning);
					}
					
					foreach(UM_Achievement achievement in settings.Achievements) {
						GPAchievement gpAch = GetAndroidAchievement(achievement.AndroidId);
						GK_AchievementTemplate gkAch = GetIOSAchievement(achievement.IOSId);

						EditorGUILayout.BeginVertical (GUI.skin.box);

						EditorGUILayout.BeginHorizontal();
						
						GUIStyle s =  new GUIStyle();
						s.padding =  new RectOffset();
						s.margin =  new RectOffset();
						s.border =  new RectOffset();
						
						if(achievement.Texture != null) {
							GUILayout.Box(achievement.Texture, s, new GUILayoutOption[]{GUILayout.Width(18), GUILayout.Height(18)});
						}

						achievement.IsOpen = EditorGUILayout.Foldout(achievement.IsOpen, achievement.id);
						bool ItemWasRemoved = DrawSortingButtons((object) achievement, settings.Achievements,
						                                         (object) gpAch, AndroidNativeSettings.Instance.Achievements,
						                                         (object) gkAch, IOSNativeSettings.Instance.Achievements);
						if(ItemWasRemoved) {
							return;
						}
						EditorGUILayout.EndHorizontal();

						if(achievement.IsOpen) {
							EditorGUI.indentLevel++; {
								
								EditorGUILayout.BeginHorizontal();
								EditorGUILayout.LabelField(AID);
								achievement.id	 	= EditorGUILayout.TextField(achievement.id);
								if(achievement.id.Length > 0) {
									achievement.id 		= achievement.id.Trim();
								}
								gkAch.Title = achievement.id;
								gpAch.Name = achievement.id;
								EditorGUILayout.EndHorizontal();
								
								
								EditorGUILayout.BeginHorizontal();
								EditorGUILayout.LabelField(ALID);
								achievement.IOSId	 	= EditorGUILayout.TextField(achievement.IOSId);
								if(achievement.IOSId.Length > 0) {
									achievement.IOSId 		= achievement.IOSId.Trim();
								}
								gkAch.Id = achievement.IOSId;
								EditorGUILayout.EndHorizontal();								
								
								EditorGUILayout.BeginHorizontal();
								EditorGUILayout.LabelField(ANDROIDAID);
								
								EditorGUI.BeginChangeCheck();
								
								bool doesntMatch = false;
								bool fileDoesntExists = false;
								
								string name = string.Empty;
								string[] names = new string[resources.Keys.Count + 1];
								names[0] = "[None]";
								resources.Keys.CopyTo(names, 1);
								List<string> listNames = new List<string>(names);
								if (achievement.AndroidId.Equals(string.Empty)) {
									name = names[EditorGUILayout.Popup(0, names)];
								} else {
									if (FileStaticAPI.IsFileExists ("Plugins/Android/res/values/ids.xml")) {
										if (resources.ContainsValue(achievement.AndroidId)) {
											name = names[EditorGUILayout.Popup(listNames.IndexOf(GetKeyForValue(resources, achievement.AndroidId)), names)];
										} else {
											doesntMatch = true;
											name = names[EditorGUILayout.Popup(0, names)];
										}
									} else {
										fileDoesntExists = true;
									}
								}
								
								if (EditorGUI.EndChangeCheck()){
									if (!name.Equals("[None]")) {
										achievement.AndroidId = resources[name];
									}
								}
								
								if(achievement.AndroidId.Length > 0) {
									achievement.AndroidId 		= achievement.AndroidId.Trim();
								}
								gpAch.Id = achievement.AndroidId;
								EditorGUILayout.EndHorizontal();
								
								EditorGUILayout.BeginHorizontal();
								if (fileDoesntExists) {
									EditorGUILayout.HelpBox("XML file with PlayService ID's DOESN'T exist", MessageType.Warning);
								}
								if (doesntMatch) {
									EditorGUILayout.HelpBox("Achievement ID doesn't match any PlayService ID of ids.xml file", MessageType.Warning);
								}
								EditorGUILayout.EndHorizontal();

								EditorGUILayout.Space();
								EditorGUILayout.Space();
								
								EditorGUILayout.BeginHorizontal();
								EditorGUILayout.LabelField(AchievementDescriptionLabel);
								EditorGUILayout.EndHorizontal();
								
								EditorGUILayout.BeginHorizontal();
								achievement.Description	 = EditorGUILayout.TextArea(achievement.Description,  new GUILayoutOption[]{GUILayout.Height(60), GUILayout.Width(200)} );
								gkAch.Description = achievement.Description;
								gpAch.Description = achievement.Description;
								achievement.Texture = (Texture2D) EditorGUILayout.ObjectField("", achievement.Texture, typeof (Texture2D), false);
								gkAch.Texture = achievement.Texture;
								gpAch.Texture = achievement.Texture;
								EditorGUILayout.EndHorizontal();
								EditorGUILayout.Space();
								
							} EditorGUI.indentLevel--;
						}
						EditorGUILayout.EndVertical();
					}
					
					
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.Space();
					if(GUILayout.Button("Add new",  GUILayout.Width(80))) {
						UM_Achievement ac = new UM_Achievement();

						int index = 0;
						do {
							index++;
						} while (IsAchievementExists(ac.id + index.ToString()));
						ac.id = ac.id + index.ToString();
						ac.AndroidId = ac.id;
						ac.IOSId = ac.id;

						settings.AddAchievement(ac);
						AndroidNativeSettings.Instance.Achievements.Add(new GPAchievement(ac.AndroidId, ac.id));
						IOSNativeSettings.Instance.Achievements.Add(new GK_AchievementTemplate(){Id = ac.IOSId, Title = ac.id});
					}
					EditorGUILayout.Space();
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.Space();					
				}
			} EditorGUI.indentLevel--;


			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(AutoLoadBigmagesLoadTitle);
			UltimateMobileSettings.Instance.AutoLoadUsersBigImages = EditorGUILayout.Toggle(UltimateMobileSettings.Instance.AutoLoadUsersBigImages);

			IOSNativeSettings.Instance.AutoLoadUsersBigImages = UltimateMobileSettings.Instance.AutoLoadUsersBigImages;
			AndroidNativeSettings.Instance.LoadProfileImages = UltimateMobileSettings.Instance.AutoLoadUsersBigImages;

			EditorGUILayout.EndHorizontal();
			
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(AutoLoadSmallImagesLoadTitle);
			UltimateMobileSettings.Instance.AutoLoadUsersSmallImages = EditorGUILayout.Toggle(UltimateMobileSettings.Instance.AutoLoadUsersSmallImages);

			IOSNativeSettings.Instance.AutoLoadUsersSmallImages = UltimateMobileSettings.Instance.AutoLoadUsersSmallImages;
			AndroidNativeSettings.Instance.LoadProfileIcons = UltimateMobileSettings.Instance.AutoLoadUsersSmallImages;

			EditorGUILayout.EndHorizontal();
		}
	}

	private bool IsLeaderboardExists(string id) {
		foreach (UM_Leaderboard lb in settings.Leaderboards) {
			if (lb.id.Equals(id)) {
				return true;
			}
		}
		return false;
	}

	private bool IsAchievementExists(string id) {
		foreach(UM_Achievement ac in settings.Achievements) {
			if (ac.id.Equals(id)) {
				return true;
			}
		}
		return false;
	}
	
	GUIContent IOS_UnitAdId  	 = new GUIContent("Banners Ad Unit Id [?]:",  "IOS Banners Ad Unit Id ");
	GUIContent IOS_InterstAdId   = new GUIContent("Interstitials Ad Unit Id [?]:", "IOS Interstitials Ad Unit Id");
	
	GUIContent Android_UnitAdId  	 = new GUIContent("Banners Ad Unit Id [?]:",  "Android Banners Ad Unit Id ");
	GUIContent Android_InterstAdId   = new GUIContent("Interstitials Ad Unit Id [?]:", "Android Interstitials Ad Unit Id");
	
	GUIContent WP8_UnitAdId  	 = new GUIContent("Banners Ad Unit Id [?]:",  "WP8 Banners Ad Unit Id ");
	GUIContent WP8_InterstAdId   = new GUIContent("Interstitials Ad Unit Id [?]:", "WP8 Interstitials Ad Unit Id");
	
	GUIContent Ad_Endgine   = new GUIContent("Ad Engine [?]:", "Ad Engine for seleceted platform");
	
	private void AdSettings() {
		EditorGUI.indentLevel = 0;
		settings.IsaAdvertisementSettingsOpen = EditorGUILayout.Foldout(settings.IsaAdvertisementSettingsOpen, "Advertisement");
		if(settings.IsaAdvertisementSettingsOpen) {
			
			EditorGUI.indentLevel++;
			settings.AdIOSSettings = EditorGUILayout.Foldout(settings.AdIOSSettings, "IOS");
			
			if(settings.AdIOSSettings) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(Ad_Endgine);
				
				EditorGUI.BeginChangeCheck();
				settings.IOSAdEdngine = (UM_IOSAdEngineOprions) EditorGUILayout.EnumPopup(settings.IOSAdEdngine);
				
				if(EditorGUI.EndChangeCheck()) {
					UpdateGoogleAdIOSAPI();
				}
				
				
				EditorGUILayout.EndHorizontal();
				
				
				if(settings.IOSAdEdngine == UM_IOSAdEngineOprions.GoogleMobileAd) {
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(IOS_UnitAdId);
					GoogleMobileAdSettings.Instance.IOS_BannersUnitId	 	= EditorGUILayout.TextField(GoogleMobileAdSettings.Instance.IOS_BannersUnitId);
					
					if(GoogleMobileAdSettings.Instance.IOS_BannersUnitId.Length > 0) {
						GoogleMobileAdSettings.Instance.IOS_BannersUnitId		= GoogleMobileAdSettings.Instance.IOS_BannersUnitId.Trim();
					}
					
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(IOS_InterstAdId);
					GoogleMobileAdSettings.Instance.IOS_InterstisialsUnitId	 	= EditorGUILayout.TextField(GoogleMobileAdSettings.Instance.IOS_InterstisialsUnitId);
					if(GoogleMobileAdSettings.Instance.IOS_InterstisialsUnitId.Length > 0) {
						GoogleMobileAdSettings.Instance.IOS_InterstisialsUnitId		= GoogleMobileAdSettings.Instance.IOS_InterstisialsUnitId.Trim();
					}
					
					EditorGUILayout.EndHorizontal();
				}
			}
			
			settings.AdAndroidSettings = EditorGUILayout.Foldout(settings.AdAndroidSettings, "Android");
			if(settings.AdAndroidSettings) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(Android_UnitAdId);
				GoogleMobileAdSettings.Instance.Android_BannersUnitId	 	= EditorGUILayout.TextField(GoogleMobileAdSettings.Instance.Android_BannersUnitId);
				if(GoogleMobileAdSettings.Instance.Android_BannersUnitId.Length > 0) {
					GoogleMobileAdSettings.Instance.Android_BannersUnitId		= GoogleMobileAdSettings.Instance.Android_BannersUnitId.Trim();
				}
				
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(Android_InterstAdId);
				GoogleMobileAdSettings.Instance.Android_InterstisialsUnitId	 	= EditorGUILayout.TextField(GoogleMobileAdSettings.Instance.Android_InterstisialsUnitId);
				if(GoogleMobileAdSettings.Instance.Android_InterstisialsUnitId.Length > 0) {
					GoogleMobileAdSettings.Instance.Android_InterstisialsUnitId		= GoogleMobileAdSettings.Instance.Android_InterstisialsUnitId.Trim();
				}
				
				EditorGUILayout.EndHorizontal();
			}
			
			
			settings.AdWp8Settings = EditorGUILayout.Foldout(settings.AdWp8Settings, "WP8");
			if(settings.AdWp8Settings) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(Ad_Endgine);
				settings.WP8AdEdngine = (UM_WP8AdEngineOprions) EditorGUILayout.EnumPopup(settings.WP8AdEdngine);
				
				
				EditorGUILayout.EndHorizontal();
				
				
				if(settings.WP8AdEdngine == UM_WP8AdEngineOprions.GoogleMobileAd) {
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(WP8_UnitAdId);
					GoogleMobileAdSettings.Instance.WP8_BannersUnitId	 	= EditorGUILayout.TextField(GoogleMobileAdSettings.Instance.WP8_BannersUnitId);
					if(GoogleMobileAdSettings.Instance.WP8_BannersUnitId.Length > 0) {
						GoogleMobileAdSettings.Instance.WP8_BannersUnitId		= GoogleMobileAdSettings.Instance.WP8_BannersUnitId.Trim();
					}
					
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(WP8_InterstAdId);
					GoogleMobileAdSettings.Instance.WP8_InterstisialsUnitId	 	= EditorGUILayout.TextField(GoogleMobileAdSettings.Instance.WP8_InterstisialsUnitId);
					if(GoogleMobileAdSettings.Instance.WP8_InterstisialsUnitId.Length > 0) {
						GoogleMobileAdSettings.Instance.WP8_InterstisialsUnitId		= GoogleMobileAdSettings.Instance.WP8_InterstisialsUnitId.Trim();
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			
			
			EditorGUI.indentLevel--;
		}
	}
	
	
	
	private void InAppSettings() {
		
		EditorGUI.indentLevel = 0;
		settings.IsInAppSettingsOpen = EditorGUILayout.Foldout(settings.IsInAppSettingsOpen, "In-App Purchases");
		if(settings.IsInAppSettingsOpen) {
			
			
			EditorGUI.indentLevel = 1;
			settings.IsInAppSettingsProductsOpen = EditorGUILayout.Foldout(settings.IsInAppSettingsProductsOpen, "Products");
			if(settings.IsInAppSettingsProductsOpen) {
				
				if(settings.InAppProducts.Count == 0) {
					EditorGUILayout.HelpBox("No In-App Products Added", MessageType.Warning);
				}
				
				
				foreach(UM_InAppProduct p in settings.InAppProducts) {
					EditorGUILayout.BeginVertical (GUI.skin.box);
					
					
					p.IsOpen = EditorGUILayout.Foldout(p.IsOpen, p.id);
					if(p.IsOpen) {
						EditorGUI.indentLevel++;
						
						
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField(InAppID);
						p.id	 	= EditorGUILayout.TextField(p.id);
						if(p.id.Length > 0) {
							p.id 		= p.id.Trim();
						}
						EditorGUILayout.EndHorizontal();
						
						
						
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField(IsCons);
						p.IsConsumable	 	= EditorGUILayout.Toggle(p.IsConsumable);
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.Space();
						
						
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField(IOSSKU);
						p.IOSId	 	= EditorGUILayout.TextField(p.IOSId);
						
						if(p.IOSId.Length > 0) {
							p.IOSId 		= p.IOSId.Trim();
						}
						EditorGUILayout.EndHorizontal();
						
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField(AndroidSKU);
						p.AndroidId	 	= EditorGUILayout.TextField(p.AndroidId);
						if(p.AndroidId.Length > 0) {
							p.AndroidId 		= p.AndroidId.Trim();
						}
						
						EditorGUILayout.EndHorizontal();
						
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField(WP8SKU);
						p.WP8Id	 	= EditorGUILayout.TextField(p.WP8Id);
						
						if(p.WP8Id.Length > 0) {
							p.WP8Id 		= p.WP8Id.Trim();
						}
						EditorGUILayout.EndHorizontal();
						
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField(DefaultPrice);
						p.ActualPrice	 	= EditorGUILayout.TextField(p.ActualPrice);
						
						if(p.ActualPrice.Length > 0) {
							p.ActualPrice 		= p.ActualPrice.Trim();
						}
						EditorGUILayout.EndHorizontal();
						
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.Space();
						if(GUILayout.Button("Remove",  GUILayout.Width(80))) {
							settings.RemoveProduct(p);
							break;
						}
						EditorGUILayout.EndHorizontal();
						
						EditorGUILayout.Space();
						EditorGUI.indentLevel--;
					}
					
					
					
					
					
					EditorGUILayout.EndVertical();
				}
				
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				if(GUILayout.Button("Add",  GUILayout.Width(80))) {
					settings.AddProduct(new UM_InAppProduct());
				}
				EditorGUILayout.EndHorizontal();
				
				
				EditorGUILayout.Space();
				
				
			}
			
			
			settings.IsInAppSettingsPlatfromsOpen = EditorGUILayout.Foldout(settings.IsInAppSettingsPlatfromsOpen, "Platfroms Settings");
			if(settings.IsInAppSettingsPlatfromsOpen) {
				
				
				EditorGUILayout.LabelField("Android:");
				EditorGUI.indentLevel = 2;
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(Base64KeyLabel);
				AndroidNativeSettings.Instance.base64EncodedPublicKey	 	= EditorGUILayout.TextField(AndroidNativeSettings.Instance.base64EncodedPublicKey);
				
				if(AndroidNativeSettings.Instance.base64EncodedPublicKey.ToString().Length > 0) {
					AndroidNativeSettings.Instance.base64EncodedPublicKey		= AndroidNativeSettings.Instance.base64EncodedPublicKey.ToString().Trim();
				}
				
				
				EditorGUILayout.EndHorizontal();
				
				EditorGUI.indentLevel = 1;
			}
		}
		
		
		
		
		EditorGUI.indentLevel = 0;
	}
	
	
	public static void UpdatePluginSettings() {
		AndroidNativeSettingsEditor.UpdatePluginSettings();
		
		string UM_InAppPurchaseManagerContent = FileStaticAPI.Read("Extensions/UltimateMobile/Scripts/InApps/Manage/UM_InAppPurchaseManager.cs");
		
		
		
		int endlineIndex;
		endlineIndex = UM_InAppPurchaseManagerContent.IndexOf(System.Environment.NewLine);
		if(endlineIndex == -1) {
			endlineIndex = UM_InAppPurchaseManagerContent.IndexOf("\n");
		}
		
		string UM_IN_Line = UM_InAppPurchaseManagerContent.Substring(0, endlineIndex);
		
		
		
		
		
		
		if(AndroidNativeSettings.Instance.EnableATCSupport) {
			UM_InAppPurchaseManagerContent 	= UM_InAppPurchaseManagerContent.Replace(UM_IN_Line, "#define ATC_SUPPORT_ENABLED");
		} else {
			UM_InAppPurchaseManagerContent 	= UM_InAppPurchaseManagerContent.Replace(UM_IN_Line, "//#define ATC_SUPPORT_ENABLED");
		}
		
		FileStaticAPI.Write("Extensions/UltimateMobile/Scripts/InApps/Manage/UM_InAppPurchaseManager.cs", UM_InAppPurchaseManagerContent);
	}
	
	private void OtherSettings() {
		
		
		UltimateMobileSettings.Instance.IsCameraAndGallerySettingsOpen = EditorGUILayout.Foldout(UltimateMobileSettings.Instance.IsCameraAndGallerySettingsOpen, "Camera And Gallery");
		if (UltimateMobileSettings.Instance.IsCameraAndGallerySettingsOpen) {
			
			EditorGUI.indentLevel++;
			UltimateMobileSettings.Instance.IsCameraAndGalleryIOSSettingsOpen = EditorGUILayout.Foldout(settings.IsCameraAndGalleryIOSSettingsOpen, "IOS");
			if(UltimateMobileSettings.Instance.IsCameraAndGalleryIOSSettingsOpen) {
				IOSNativeSettingsEditor.CameraAndGallery();
			}
			
			UltimateMobileSettings.Instance.IsCameraAndGalleryAndroidSettingsOpen = EditorGUILayout.Foldout(settings.IsCameraAndGalleryAndroidSettingsOpen, "Android");
			if(UltimateMobileSettings.Instance.IsCameraAndGalleryAndroidSettingsOpen) {
				AndroidNativeSettingsEditor.CameraAndGalleryParams();
			}
			EditorGUI.indentLevel--;
		}
		
		
		
		UltimateMobileSettings.Instance.IsLPSettingsOpen = EditorGUILayout.Foldout(UltimateMobileSettings.Instance.IsLPSettingsOpen, "Local And Push Notifications");
		if (UltimateMobileSettings.Instance.IsLPSettingsOpen) {
			
			EditorGUI.indentLevel++;
			UltimateMobileSettings.Instance.IsLP_IOS_SettingsOpen = EditorGUILayout.Foldout(settings.IsLP_IOS_SettingsOpen, "IOS");
			if(UltimateMobileSettings.Instance.IsLP_IOS_SettingsOpen) {
				EditorGUILayout.HelpBox("No Settings Required", MessageType.None);
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				if(GUILayout.Button("IOS Push Notifications Guide",  GUILayout.Width(200))) {
					Application.OpenURL("http://goo.gl/3CCJ9Q");
				}
				EditorGUILayout.EndHorizontal();
			}
			
			UltimateMobileSettings.Instance.IsLP_Android_SettingsOpen = EditorGUILayout.Foldout(settings.IsLP_Android_SettingsOpen, "Android");
			if(UltimateMobileSettings.Instance.IsLP_Android_SettingsOpen) {
				
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("GCM Sender Id");
				AndroidNativeSettings.Instance.GCM_SenderId	 	= EditorGUILayout.TextField(AndroidNativeSettings.Instance.GCM_SenderId);
				if(AndroidNativeSettings.Instance.GCM_SenderId.Length > 0) {
					AndroidNativeSettings.Instance.GCM_SenderId		= AndroidNativeSettings.Instance.GCM_SenderId.Trim();
				}
				
				EditorGUILayout.EndHorizontal();
				
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				if(GUILayout.Button("Android Push Notifications Guide",  GUILayout.Width(200))) {
					Application.OpenURL("http://goo.gl/F0Jkfv");
				}
				EditorGUILayout.EndHorizontal();
				
			}
			
			EditorGUI.indentLevel--;
		}
		
		EditorGUILayout.Space ();
		UltimateMobileSettings.Instance.ThirdPartyParams_SettingsOpen = EditorGUILayout.Foldout (UltimateMobileSettings.Instance.ThirdPartyParams_SettingsOpen, "Third-Party Plug-Ins Params");
		if (UltimateMobileSettings.Instance.ThirdPartyParams_SettingsOpen) {
			EditorGUI.BeginChangeCheck ();
			
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Anti-Cheat Toolkit Support");
			AndroidNativeSettings.Instance.EnableATCSupport = EditorGUILayout.Toggle ("", AndroidNativeSettings.Instance.EnableATCSupport);
			
			
			EditorGUILayout.Space ();
			EditorGUILayout.EndHorizontal ();
			
			if(EditorGUI.EndChangeCheck()) {
				#if !UNITY_WEBPLAYER
				UpdatePluginSettings();
				#endif
			}
			
			
			
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.Space ();
			if (GUILayout.Button("[?] Read More", GUILayout.Width(100.0f))) {
				Application.OpenURL("http://goo.gl/dokdpv");
			}
			
			EditorGUILayout.EndHorizontal ();
		}
		
		
	}
	
	
	
	private void AboutGUI() {
		
		EditorGUILayout.HelpBox("About Ultimate Mobile", MessageType.None);
		EditorGUILayout.Space();
		
		SelectableLabelField(SdkVersion, UltimateMobileSettings.VERSION_NUMBER);
		SelectableLabelField(SupportEmail, "stans.assets@gmail.com");
		
	}
	
	private void SelectableLabelField(GUIContent label, string value) {
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField(label, GUILayout.Width(180), GUILayout.Height(16));
		EditorGUILayout.SelectableLabel(value, GUILayout.Height(16));
		EditorGUILayout.EndHorizontal();
	}
	
	
	
	private static void DirtyEditor() {
		#if UNITY_EDITOR
		EditorUtility.SetDirty(UltimateMobileSettings.Instance);
		EditorUtility.SetDirty(AndroidNativeSettings.Instance);
		EditorUtility.SetDirty(GoogleMobileAdSettings.Instance);
		EditorUtility.SetDirty(IOSNativeSettings.Instance);
		#endif
	}
	
	
}
