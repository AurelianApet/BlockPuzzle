using UnityEngine;
using UnityEditor;
using System.Collections;

public class MNP_PlatformMenu : EditorWindow {

	#if UNITY_EDITOR
	
	//--------------------------------------
	//  GENERAL
	//--------------------------------------
	
	[MenuItem("Window/Stan's Assets/Mobile Native Pop Up/Edit Settings")]
	public static void Edit() {
		Selection.activeObject = MNP_PlatformSettings.Instance;
	}

	#endif
}
