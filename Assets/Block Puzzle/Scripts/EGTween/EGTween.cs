
#region Namespaces
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#endregion

public class EGTween : MonoBehaviour{
		
	#region Variables
	public static List<Hashtable> tweens = new List<Hashtable>();
	
	//camera fade object:
	private static GameObject cameraFade;
	
	//status members (made public for visual troubleshooting in the inspector):
	public string id, type, method;
	public EGTween.EaseType easeType;
	public float time, delay;
	public LoopType loopType;
	public bool isRunning,isPaused;
	/* GFX47 MOD START */
	public string _name;
	/* GFX47 MOD END */
		
	//private members:
 	private float runningTime, percentage;
	private float delayStarted; //probably not neccesary that this be protected but it shuts Unity's compiler up about this being "never used"
	private bool kinematic, isLocal, loop, reverse, wasPaused, physics;
	private Hashtable tweenArguments;
	private Space space;
	private delegate float EasingFunction(float start, float end, float Value);
	private delegate void ApplyTween();
	private EasingFunction ease;
	private ApplyTween apply;
	private Vector3[] vector3s;
	private Vector2[] vector2s;
	private Color[,] colors;
	private float[] floats;
	private Rect[] rects;
	private Vector3 preUpdate;
	private Vector3 postUpdate;
	//private NamedValueColor namedcolorvalue;

    private float lastRealTime; // Added by PressPlay
    private bool useRealTime; // Added by PressPlay
	
	private Transform thisTransform;

	/// <summary>
	/// The type of easing to use based on Robert Penner's open source easing equations (http://www.robertpenner.com/easing_terms_of_use.html).
	/// </summary>
	public enum EaseType{
		easeInBack,
		easeOutBack,
		easeInOutBack,
		easeInExpo,
		easeOutExpo,
		easeInOutExpo,
		linear,
	}
	
	/// <summary>
	/// The type of loop (if any) to use.  
	/// </summary>
	public enum LoopType{
		/// <summary>
		/// Do not loop.
		/// </summary>
		none,
		/// <summary>
		/// Rewind and replay.
		/// </summary>
		loop,
		/// <summary>
		/// Ping pong the animation back and forth.
		/// </summary>
		pingPong
	}
	
	/// <summary>
	/// Many shaders use more than one color. Use can have EGTween's Color methods operate on them by name.   
	/// </summary>
	public enum NamedValueColor{
		/// <summary>
		/// The main color of a material. Used by default and not required for Color methods to work in EGTween.
		/// </summary>
		_Color,
		/// <summary>
		/// The specular color of a material (used in specular/glossy/vertexlit shaders).
		/// </summary>
		_SpecColor,
		/// <summary>
		/// The emissive color of a material (used in vertexlit shaders).
		/// </summary>
		_Emission,
		/// <summary>
		/// The reflection color of the material (used in reflective shaders).
		/// </summary>
		_ReflectColor
	}
				
	#endregion
	
	#region Defaults
	
	/// <summary>
	/// A collection of baseline presets that EGTween needs and utilizes if certain parameters are not provided. 
	/// </summary>
	public static class Defaults{
		//general defaults:
		public static float time = 1f;
		public static float delay = 0f;	
		public static LoopType loopType = LoopType.none;
		public static EaseType easeType = EGTween.EaseType.easeOutExpo;
		public static bool isLocal = false;
		public static Space space = Space.Self;
		public static Color color = Color.white;
		//update defaults:
		public static float updateTimePercentage = .05f;
		public static float updateTime = 1f*updateTimePercentage;
		public static float lookAhead = .05f;
        public static bool useRealTime = false; // Added by PressPlay
		//look direction:
		public static Vector3 up = Vector3.up;
	}
	
	#endregion
	
	#region #1 Static Registers

	/// <summary>
	/// Sets up a GameObject to avoid hiccups when an initial EGTween is added. It's advisable to run this on every object you intend to run EGTween on in its Start or Awake.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target to be initialized for EGTween.
	/// </param>
	public static void Init(GameObject target){
		MoveBy(target,Vector3.zero,0);
	}
	
	/// <summary>
	/// Returns a value to an 'oncallback' method interpolated between the supplied 'from' and 'to' values for application as desired.  Requires an 'onupdate' callback that accepts the same type as the supplied 'from' and 'to' properties.
	/// </summary>
	/// <param name="from">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> or <see cref="Vector3"/> or <see cref="Vector2"/> or <see cref="Color"/> or <see cref="Rect"/> for the starting value.
	/// </param> 
	/// <param name="to">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> or <see cref="Vector3"/> or <see cref="Vector2"/> or <see cref="Color"/> or <see cref="Rect"/> for the ending value.
	/// </param> 
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="speed">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> can be used instead of time to allow animation based on speed (only works with Vector2, Vector3, and Floats)
	/// </param>	
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void ValueTo(GameObject target, Hashtable args){
		//clean args:
		args = EGTween.CleanArgs(args);
		
		if (!args.Contains("onupdate") || !args.Contains("from") || !args.Contains("to")) {
			Debug.LogError("EGTween Error: ValueTo() requires an 'onupdate' callback function and a 'from' and 'to' property.  The supplied 'onupdate' callback must accept a single argument that is the same type as the supplied 'from' and 'to' properties!");
			return;
		}else{
			//establish EGTween:
			args["type"]="value";
			
			if (args["from"].GetType() == typeof(Vector2)) {
				args["method"]="vector2";
			}else if (args["from"].GetType() == typeof(Vector3)) {
				args["method"]="vector3";
			}else if (args["from"].GetType() == typeof(Rect)) {
				args["method"]="rect";
			}else if (args["from"].GetType() == typeof(Single)) {
				args["method"]="float";
			}else{
				Debug.LogError("EGTween Error: ValueTo() only works with interpolating Vector3s, Vector2s, floats, ints, Rects and Colors!");
				return;	
			}
			
			//set a default easeType of linear if none is supplied since eased color interpolation is nearly unrecognizable:
			if (!args.Contains("easetype")) {
				args.Add("easetype",EaseType.linear);
			}
			
			Launch(target,args);
		}
	}
	
	/// <summary>
	/// Changes a GameObject's position over time to a supplied destination with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="position">
	/// A <see cref="Vector3"/> for the destination Vector3.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void MoveTo(GameObject target, Vector3 position, float time){
		MoveTo(target,Hash("position",position,"time",time));
	}	
		
	/// <summary>
	/// Changes a GameObject's position over time to a supplied destination with FULL customization options.
	/// </summary>
	/// <param name="position">
	/// A <see cref="Transform"/> or <see cref="Vector3"/> for a point in space the GameObject will animate to.
	/// </param>
	/// <param name="path">
	/// A <see cref="Transform[]"/> or <see cref="Vector3[]"/> for a list of points to draw a Catmull-Rom through for a curved animation path.
	/// </param>
	/// <param name="movetopath">
	/// A <see cref="System.Boolean"/> for whether to automatically generate a curve from the GameObject's current position to the beginning of the path. True by default.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="orienttopath">
	/// A <see cref="System.Boolean"/> for whether or not the GameObject will orient to its direction of travel.  False by default.
	/// </param>
	/// <param name="looktarget">
	/// A <see cref="Vector3"/> or A <see cref="Transform"/> for a target the GameObject will look at.
	/// </param>
	/// <param name="looktime">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the object will take to look at either the "looktarget" or "orienttopath".
	/// </param>
	/// <param name="lookahead">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for how much of a percentage to look ahead on a path to influence how strict "orientopath" is.
	/// </param>
	/// <param name="axis">
	/// A <see cref="System.String"/>. Restricts rotation to the supplied axis only.
	/// </param>
	/// <param name="islocal">
	/// A <see cref="System.Boolean"/> for whether to animate in world space or relative to the parent. False by default.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="speed">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> can be used instead of time to allow animation based on speed
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void MoveTo(GameObject target, Hashtable args){
		//clean args:
		args = EGTween.CleanArgs(args);
		
		//additional property to ensure ConflictCheck can work correctly since Transforms are refrences:		
		if(args.Contains("position")){
			if (args["position"].GetType() == typeof(Transform)) {
				Transform transform = (Transform)args["position"];
				args["position"]=new Vector3(transform.position.x,transform.position.y,transform.position.z);
				args["rotation"]=new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,transform.eulerAngles.z);
				args["scale"]=new Vector3(transform.localScale.x,transform.localScale.y,transform.localScale.z);
			}
		}		
		
		//establish EGTween:
		args["type"]="move";
		args["method"]="to";
		Launch(target,args);
	}
		
	/// <summary>
	/// Instantly changes a GameObject's position to a supplied destination then returns it to it's starting position over time with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="position">
	/// A <see cref="Vector3"/> for the destination Vector3.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void MoveFrom(GameObject target, Vector3 position, float time){
		MoveFrom(target,Hash("position",position,"time",time));
	}		
	
	/// <summary>
	/// Instantly changes a GameObject's position to a supplied destination then returns it to it's starting position over time with FULL customization options.
	/// </summary>
	/// <param name="position">
	/// A <see cref="Transform"/> or <see cref="Vector3"/> for a point in space the GameObject will animate to.
	/// </param>
	/// <param name="path">
	/// A <see cref="Transform[]"/> or <see cref="Vector3[]"/> for a list of points to draw a Catmull-Rom through for a curved animation path.
	/// </param>
	/// <param name="movetopath">
	/// A <see cref="System.Boolean"/> for whether to automatically generate a curve from the GameObject's current position to the beginning of the path. True by default.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="orienttopath">
	/// A <see cref="System.Boolean"/> for whether or not the GameObject will orient to its direction of travel.  False by default.
	/// </param>
	/// <param name="looktarget">
	/// A <see cref="Vector3"/> or A <see cref="Transform"/> for a target the GameObject will look at.
	/// </param>
	/// <param name="looktime">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the object will take to look at either the "looktarget" or "orienttopath".
	/// </param>
	/// <param name="lookahead">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for how much of a percentage to look ahead on a path to influence how strict "orientopath" is.
	/// </param>
	/// <param name="islocal">
	/// A <see cref="System.Boolean"/> for whether to animate in world space or relative to the parent. False by default.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="speed">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> can be used instead of time to allow animation based on speed
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void MoveFrom(GameObject target, Hashtable args){
		//clean args:
			args = EGTween.CleanArgs(args);
		
		bool tempIsLocal;
		
		//set tempIsLocal:
		if(args.Contains("islocal")){
			tempIsLocal = (bool)args["islocal"];
		}else{
			tempIsLocal = Defaults.isLocal;	
		}

		Vector3 tempPosition;
		Vector3 fromPosition;
		
		//set tempPosition and base fromPosition:
		if(tempIsLocal){
			tempPosition=fromPosition=target.transform.localPosition;
		}else{
			tempPosition=fromPosition=target.transform.position;	
		}
		
		//set augmented fromPosition:
		if(args.Contains("position")){
			if (args["position"].GetType() == typeof(Transform)){
				Transform trans = (Transform)args["position"];
				fromPosition=trans.position;
			}else if(args["position"].GetType() == typeof(Vector3)){
				fromPosition=(Vector3)args["position"];
			}			
		}else{
			if (args.Contains("x")) {
				fromPosition.x=(float)args["x"];
			}
			if (args.Contains("y")) {
				fromPosition.y=(float)args["y"];
			}
			if (args.Contains("z")) {
				fromPosition.z=(float)args["z"];
			}
		}
		
		//apply fromPosition:
		if(tempIsLocal){
			target.transform.localPosition = fromPosition;
		}else{
			target.transform.position = fromPosition;	
		}
		
		//set new position arg:
		args["position"]=tempPosition;
	
			
		//establish EGTween:
		args["type"]="move";
		args["method"]="to";
		Launch(target,args);
	}
		

	/// <summary>
	/// Adds the supplied coordinates to a GameObject's postion with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the amount of change in position to move the GameObject.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void MoveBy(GameObject target, Vector3 amount, float time){
		MoveBy(target,Hash("amount",amount,"time",time));
	}
	
	/// <summary>
	/// Adds the supplied coordinates to a GameObject's position with FULL customization options.
	/// </summary>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the amount of change in position to move the GameObject.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="orienttopath">
	/// A <see cref="System.Boolean"/> for whether or not the GameObject will orient to its direction of travel.  False by default.
	/// </param>
	/// <param name="looktarget">
	/// A <see cref="Vector3"/> or A <see cref="Transform"/> for a target the GameObject will look at.
	/// </param>
	/// <param name="looktime">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the object will take to look at either the "looktarget" or "orienttopath".
	/// </param>
	/// <param name="axis">
	/// A <see cref="System.String"/>. Restricts rotation to the supplied axis only.
	/// </param>
	/// <param name="space">
	/// A <see cref="Space"/> or <see cref="System.String"/> for applying the transformation in either the world coordinate or local cordinate system. Defaults to local space.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="speed">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> can be used instead of time to allow animation based on speed
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void MoveBy(GameObject target, Hashtable args){
		//clean args:
		args = EGTween.CleanArgs(args);
		
		//establish EGTween:
		args["type"]="move";
		args["method"]="by";
		Launch(target,args);
	}
	
	/// <summary>
	/// Changes a GameObject's scale over time with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="scale">
	/// A <see cref="Vector3"/> for the final scale.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void ScaleTo(GameObject target, Vector3 scale, float time){
		ScaleTo(target,Hash("scale",scale,"time",time));
	}
	
	/// <summary>
	/// Changes a GameObject's scale over time with FULL customization options.
	/// </summary>
	/// <param name="scale">
	/// A <see cref="Transform"/> or <see cref="Vector3"/> for the final scale.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="speed">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> can be used instead of time to allow animation based on speed
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void ScaleTo(GameObject target, Hashtable args){
		//clean args:
		args = EGTween.CleanArgs(args);
		
		//additional property to ensure ConflictCheck can work correctly since Transforms are refrences:		
		if(args.Contains("scale")){
			if (args["scale"].GetType() == typeof(Transform)) {
				Transform transform = (Transform)args["scale"];
				args["position"]=new Vector3(transform.position.x,transform.position.y,transform.position.z);
				args["rotation"]=new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,transform.eulerAngles.z);
				args["scale"]=new Vector3(transform.localScale.x,transform.localScale.y,transform.localScale.z);
			}
		}
		
		//establish EGTween:
		args["type"]="scale";
		args["method"]="to";
		Launch(target,args);
	}
	
	/// <summary>
	/// Instantly changes a GameObject's scale then returns it to it's starting scale over time with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="scale">
	/// A <see cref="Vector3"/> for the final scale.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void ScaleFrom(GameObject target, Vector3 scale, float time){
		ScaleFrom(target,Hash("scale",scale,"time",time));
	}
	
	/// <summary>
	/// Instantly changes a GameObject's scale then returns it to it's starting scale over time with FULL customization options.
	/// </summary>
	/// <param name="scale">
	/// A <see cref="Transform"/> or <see cref="Vector3"/> for the final scale.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="speed">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> can be used instead of time to allow animation based on speed
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void ScaleFrom(GameObject target, Hashtable args){
		Vector3 tempScale;
		Vector3 fromScale;
	
		//clean args:
		args = EGTween.CleanArgs(args);
		
		//set base fromScale:
		tempScale=fromScale=target.transform.localScale;
		
		//set augmented fromScale:
		if(args.Contains("scale")){
			if (args["scale"].GetType() == typeof(Transform)){
				Transform trans = (Transform)args["scale"];
				fromScale=trans.localScale;
			}else if(args["scale"].GetType() == typeof(Vector3)){
				fromScale=(Vector3)args["scale"];
			}	
		}else{
			if (args.Contains("x")) {
				fromScale.x=(float)args["x"];
			}
			if (args.Contains("y")) {
				fromScale.y=(float)args["y"];
			}
			if (args.Contains("z")) {
				fromScale.z=(float)args["z"];
			}
		}
		
		//apply fromScale:
		target.transform.localScale = fromScale;	
		
		//set new scale arg:
		args["scale"]=tempScale;
		
		//establish EGTween:
		args["type"]="scale";
		args["method"]="to";
		Launch(target,args);
	}
	
	/// <summary>
	/// Multiplies a GameObject's scale over time with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the amount of scale to be multiplied by the GameObject's current scale.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void ScaleBy(GameObject target, Vector3 amount, float time){
		ScaleBy(target,Hash("amount",amount,"time",time));
	}
	
	/// <summary>
	/// Multiplies a GameObject's scale over time with FULL customization options.
	/// </summary>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the amount to be multiplied to the GameObject's current scale.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="speed">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> can be used instead of time to allow animation based on speed
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void ScaleBy(GameObject target, Hashtable args){
		//clean args:
		args = EGTween.CleanArgs(args);
		
		//establish EGTween:
		args["type"]="scale";
		args["method"]="by";
		Launch(target,args);
	}
	
	/// <summary>
	/// Rotates a GameObject to the supplied Euler angles in degrees over time with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="rotation">
	/// A <see cref="Vector3"/> for the target Euler angles in degrees to rotate to.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void RotateTo(GameObject target, Vector3 rotation, float time){
		RotateTo(target,Hash("rotation",rotation,"time",time));
	}
	
	/// <summary>
	/// Rotates a GameObject to the supplied Euler angles in degrees over time with FULL customization options.
	/// </summary>
	/// <param name="rotation">
	/// A <see cref="Transform"/> or <see cref="Vector3"/> for the target Euler angles in degrees to rotate to.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="islocal">
	/// A <see cref="System.Boolean"/> for whether to animate in world space or relative to the parent. False by default.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="speed">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> can be used instead of time to allow animation based on speed
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void RotateTo(GameObject target, Hashtable args){
		//clean args:
		args = EGTween.CleanArgs(args);
		
		//additional property to ensure ConflictCheck can work correctly since Transforms are refrences:		
		if(args.Contains("rotation")){
			if (args["rotation"].GetType() == typeof(Transform)) {
				Transform transform = (Transform)args["rotation"];
				args["position"]=new Vector3(transform.position.x,transform.position.y,transform.position.z);
				args["rotation"]=new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,transform.eulerAngles.z);
				args["scale"]=new Vector3(transform.localScale.x,transform.localScale.y,transform.localScale.z);
			}
		}		
		
		//establish EGTween
		args["type"]="rotate";
		args["method"]="to";
		Launch(target,args);
	}	
	
	/// <summary>
	/// Instantly changes a GameObject's Euler angles in degrees then returns it to it's starting rotation over time (if allowed) with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="rotation">
	/// A <see cref="Vector3"/> for the target Euler angles in degrees to rotate from.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void RotateFrom(GameObject target, Vector3 rotation, float time){
		RotateFrom(target,Hash("rotation",rotation,"time",time));
	}
	
	/// <summary>
	/// Instantly changes a GameObject's Euler angles in degrees then returns it to it's starting rotation over time (if allowed) with FULL customization options.
	/// </summary>
	/// <param name="rotation">
	/// A <see cref="Transform"/> or <see cref="Vector3"/> for the target Euler angles in degrees to rotate to.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="islocal">
	/// A <see cref="System.Boolean"/> for whether to animate in world space or relative to the parent. False by default.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="speed">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> can be used instead of time to allow animation based on speed
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void RotateFrom(GameObject target, Hashtable args){
		Vector3 tempRotation;
		Vector3 fromRotation;
		bool tempIsLocal;
	
		//clean args:
		args = EGTween.CleanArgs(args);
		
		//set tempIsLocal:
		if(args.Contains("islocal")){
			tempIsLocal = (bool)args["islocal"];
		}else{
			tempIsLocal = Defaults.isLocal;	
		}

		//set tempRotation and base fromRotation:
		if(tempIsLocal){
			tempRotation=fromRotation=target.transform.localEulerAngles;
		}else{
			tempRotation=fromRotation=target.transform.eulerAngles;	
		}
		
		//set augmented fromRotation:
		if(args.Contains("rotation")){
			if (args["rotation"].GetType() == typeof(Transform)){
				Transform trans = (Transform)args["rotation"];
				fromRotation=trans.eulerAngles;
			}else if(args["rotation"].GetType() == typeof(Vector3)){
				fromRotation=(Vector3)args["rotation"];
			}	
		}else{
			if (args.Contains("x")) {
				fromRotation.x=(float)args["x"];
			}
			if (args.Contains("y")) {
				fromRotation.y=(float)args["y"];
			}
			if (args.Contains("z")) {
				fromRotation.z=(float)args["z"];
			}
		}
		
		//apply fromRotation:
		if(tempIsLocal){
			target.transform.localEulerAngles = fromRotation;
		}else{
			target.transform.eulerAngles = fromRotation;	
		}
		
		//set new rotation arg:
		args["rotation"]=tempRotation;
		
		//establish EGTween:
		args["type"]="rotate";
		args["method"]="to";
		Launch(target,args);
	}	
	
	/// <summary>
	/// Multiplies supplied values by 360 and rotates a GameObject by calculated amount over time with MINIMUM customization options.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the amount to be multiplied by 360 to rotate the GameObject.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void RotateBy(GameObject target, Vector3 amount, float time){
		RotateBy(target,Hash("amount",amount,"time",time));
	}
	
	/// <summary>
	/// Multiplies supplied values by 360 and rotates a GameObject by calculated amount over time with FULL customization options.
	/// </summary>
	/// <param name="amount">
	/// A <see cref="Vector3"/> for the amount to be multiplied by 360 to rotate the GameObject.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="space">
	/// A <see cref="Space"/> or <see cref="System.String"/> for applying the transformation in either the world coordinate or local cordinate system. Defaults to local space.
	/// </param>
	/// <param name="islocal">
	/// A <see cref="System.Boolean"/> for whether to animate in world space or relative to the parent. False by default.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	/// <param name="speed">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> can be used instead of time to allow animation based on speed
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will wait before beginning.
	/// </param>
	/// <param name="easetype">
	/// A <see cref="EaseType"/> or <see cref="System.String"/> for the shape of the easing curve applied to the animation.
	/// </param>   
	/// <param name="looptype">
	/// A <see cref="LoopType"/> or <see cref="System.String"/> for the type of loop to apply once the animation has completed.
	/// </param>
	/// <param name="onstart">
	/// A <see cref="System.String"/> for the name of a function to launch at the beginning of the animation.
	/// </param>
	/// <param name="onstarttarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onstart" method.
	/// </param>
	/// <param name="onstartparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onstart" method.
	/// </param>
	/// <param name="onupdate"> 
	/// A <see cref="System.String"/> for the name of a function to launch on every step of the animation.
	/// </param>
	/// <param name="onupdatetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "onupdate" method.
	/// </param>
	/// <param name="onupdateparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "onupdate" method.
	/// </param> 
	/// <param name="oncomplete">
	/// A <see cref="System.String"/> for the name of a function to launch at the end of the animation.
	/// </param>
	/// <param name="oncompletetarget">
	/// A <see cref="GameObject"/> for a reference to the GameObject that holds the "oncomplete" method.
	/// </param>
	/// <param name="oncompleteparams">
	/// A <see cref="System.Object"/> for arguments to be sent to the "oncomplete" method.
	/// </param>
	public static void RotateBy(GameObject target, Hashtable args){
		//clean args:
		args = EGTween.CleanArgs(args);
		
		//establish EGTween
		args["type"]="rotate";
		args["method"]="by";
		Launch(target,args);
	}		
	#endregion
	
	#region #2 Generate Method Targets
	
	//call correct set target method and set tween application delegate:
	void GenerateTargets(){
		switch (type) {
			case "value":
				switch (method) {
					case "float":
						GenerateFloatTargets();
						apply = new ApplyTween(ApplyFloatTargets);
					break;
				case "vector2":
						GenerateVector2Targets();
						apply = new ApplyTween(ApplyVector2Targets);
					break;
				case "vector3":
						GenerateVector3Targets();
						apply = new ApplyTween(ApplyVector3Targets);
					break;
				case "rect":
						GenerateRectTargets();
						apply = new ApplyTween(ApplyRectTargets);
					break;
				}
			break;

			case "move":
				switch (method) {
					case "to":
						GenerateMoveToTargets();
						apply = new ApplyTween(ApplyMoveToTargets);
					break;
					case "by":
						GenerateMoveByTargets();
						apply = new ApplyTween(ApplyMoveByTargets);
					break;
				}
			break;
			case "scale":
				switch (method){
					case "to":
						GenerateScaleToTargets();
						apply = new ApplyTween(ApplyScaleToTargets);
					break;
					case "by":
						GenerateScaleByTargets();
						apply = new ApplyTween(ApplyScaleToTargets);
					break;
				}
			break;
			case "rotate":
				switch (method) {
					case "to":
						GenerateRotateToTargets();
						apply = new ApplyTween(ApplyRotateToTargets);
					break;

					case "by":
						GenerateRotateByTargets();
						apply = new ApplyTween(ApplyRotateAddTargets);
					break;				
				}
			break;
		}
	}
	
	#endregion
	
	#region #3 Generate Specific Targets
	
	void GenerateRectTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		rects=new Rect[3];
		
		//from and to values:
		rects[0]=(Rect)tweenArguments["from"];
		rects[1]=(Rect)tweenArguments["to"];
	}		
	
	void GenerateColorTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		colors=new Color[1,3];
		
		//from and to values:
		colors[0,0]=(Color)tweenArguments["from"];
		colors[0,1]=(Color)tweenArguments["to"];
	}	
	
	void GenerateVector3Targets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		vector3s=new Vector3[3];
		
		//from and to values:
		vector3s[0]=(Vector3)tweenArguments["from"];
		vector3s[1]=(Vector3)tweenArguments["to"];
		
		//need for speed?
		if(tweenArguments.Contains("speed")){
			float distance = Math.Abs(Vector3.Distance(vector3s[0],vector3s[1]));
			time = distance/(float)tweenArguments["speed"];
		}
	}
	
	void GenerateVector2Targets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		vector2s=new Vector2[3];
		
		//from and to values:
		vector2s[0]=(Vector2)tweenArguments["from"];
		vector2s[1]=(Vector2)tweenArguments["to"];
		
		//need for speed?
		if(tweenArguments.Contains("speed")){
			Vector3 fromV3 = new Vector3(vector2s[0].x,vector2s[0].y,0);
			Vector3 toV3 = new Vector3(vector2s[1].x,vector2s[1].y,0);
			float distance = Math.Abs(Vector3.Distance(fromV3,toV3));
			time = distance/(float)tweenArguments["speed"];
		}
	}
	
	void GenerateFloatTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		floats=new float[3];
		
		//from and to values:
		floats[0]=(float)tweenArguments["from"];
		floats[1]=(float)tweenArguments["to"];
		
		//need for speed?
		if(tweenArguments.Contains("speed")){
			float distance = Math.Abs(floats[0] - floats[1]);
			time = distance/(float)tweenArguments["speed"];
		}
	}

	void GenerateMoveToTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		vector3s=new Vector3[3];
		
		//from values:
		if (isLocal) {
			vector3s[0]=vector3s[1]=thisTransform.localPosition;				
		}else{
			vector3s[0]=vector3s[1]=thisTransform.position;
		}
		
		//to values:
		if (tweenArguments.Contains("position")) {
			if (tweenArguments["position"].GetType() == typeof(Transform)){
				Transform trans = (Transform)tweenArguments["position"];
				vector3s[1]=trans.position;
			}else if(tweenArguments["position"].GetType() == typeof(Vector3)){
				vector3s[1]=(Vector3)tweenArguments["position"];
			}
		}else{
			if (tweenArguments.Contains("x")) {
				vector3s[1].x=(float)tweenArguments["x"];
			}
			if (tweenArguments.Contains("y")) {
				vector3s[1].y=(float)tweenArguments["y"];
			}
			if (tweenArguments.Contains("z")) {
				vector3s[1].z=(float)tweenArguments["z"];
			}
		}
		
		//handle orient to path request:
		if(tweenArguments.Contains("orienttopath") && (bool)tweenArguments["orienttopath"]){
			tweenArguments["looktarget"] = vector3s[1];
		}
		
		//need for speed?
		if(tweenArguments.Contains("speed")){
			float distance = Math.Abs(Vector3.Distance(vector3s[0],vector3s[1]));
			time = distance/(float)tweenArguments["speed"];
		}
	}
	
	void GenerateMoveByTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation, [3] previous value for Translate usage to allow Space utilization, [4] original rotation to make sure look requests don't interfere with the direction object should move in, [5] for dial in location:
		vector3s=new Vector3[6];
		
		//grab starting rotation:
		vector3s[4] = thisTransform.eulerAngles;
		
		//from values:
		vector3s[0]=vector3s[1]=vector3s[3]=thisTransform.position;
				
		//to values:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]=vector3s[0] + (Vector3)tweenArguments["amount"];
		}else{
			if (tweenArguments.Contains("x")) {
				vector3s[1].x=vector3s[0].x + (float)tweenArguments["x"];
			}
			if (tweenArguments.Contains("y")) {
				vector3s[1].y=vector3s[0].y +(float)tweenArguments["y"];
			}
			if (tweenArguments.Contains("z")) {
				vector3s[1].z=vector3s[0].z + (float)tweenArguments["z"];
			}
		}	
		
		//calculation for dial in:
		thisTransform.Translate(vector3s[1],space);
		vector3s[5] = thisTransform.position;
		thisTransform.position=vector3s[0];
		
		//handle orient to path request:
		if(tweenArguments.Contains("orienttopath") && (bool)tweenArguments["orienttopath"]){
			tweenArguments["looktarget"] = vector3s[1];
		}
		
		//need for speed?
		if(tweenArguments.Contains("speed")){
			float distance = Math.Abs(Vector3.Distance(vector3s[0],vector3s[1]));
			time = distance/(float)tweenArguments["speed"];
		}
	}
	
	void GenerateScaleToTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		vector3s=new Vector3[3];
		
		//from values:
		vector3s[0]=vector3s[1]=thisTransform.localScale;				

		//to values:
		if (tweenArguments.Contains("scale")) {
			if (tweenArguments["scale"].GetType() == typeof(Transform)){
				Transform trans = (Transform)tweenArguments["scale"];
				vector3s[1]=trans.localScale;					
			}else if(tweenArguments["scale"].GetType() == typeof(Vector3)){
				vector3s[1]=(Vector3)tweenArguments["scale"];
			}
		}else{
			if (tweenArguments.Contains("x")) {
				vector3s[1].x=(float)tweenArguments["x"];
			}
			if (tweenArguments.Contains("y")) {
				vector3s[1].y=(float)tweenArguments["y"];
			}
			if (tweenArguments.Contains("z")) {
				vector3s[1].z=(float)tweenArguments["z"];
			}
		} 
		
		//need for speed?
		if(tweenArguments.Contains("speed")){
			float distance = Math.Abs(Vector3.Distance(vector3s[0],vector3s[1]));
			time = distance/(float)tweenArguments["speed"];
		}
	}
	
	void GenerateScaleByTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		vector3s=new Vector3[3];
		
		//from values:
		vector3s[0]=vector3s[1]=thisTransform.localScale;				

		//to values:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]=Vector3.Scale(vector3s[1],(Vector3)tweenArguments["amount"]);
		}else{
			if (tweenArguments.Contains("x")) {
				vector3s[1].x*=(float)tweenArguments["x"];
			}
			if (tweenArguments.Contains("y")) {
				vector3s[1].y*=(float)tweenArguments["y"];
			}
			if (tweenArguments.Contains("z")) {
				vector3s[1].z*=(float)tweenArguments["z"];
			}
		} 
		
		//need for speed?
		if(tweenArguments.Contains("speed")){
			float distance = Math.Abs(Vector3.Distance(vector3s[0],vector3s[1]));
			time = distance/(float)tweenArguments["speed"];
		}
	}
	

	void GenerateRotateToTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		vector3s=new Vector3[3];
		
		//from values:
		if (isLocal) {
			vector3s[0]=vector3s[1]=thisTransform.localEulerAngles;				
		}else{
			vector3s[0]=vector3s[1]=thisTransform.eulerAngles;
		}
		
		//to values:
		if (tweenArguments.Contains("rotation")) {
			if (tweenArguments["rotation"].GetType() == typeof(Transform)){
				Transform trans = (Transform)tweenArguments["rotation"];
				vector3s[1]=trans.eulerAngles;			
			}else if(tweenArguments["rotation"].GetType() == typeof(Vector3)){
				vector3s[1]=(Vector3)tweenArguments["rotation"];
			}
		}else{
			if (tweenArguments.Contains("x")) {
				vector3s[1].x=(float)tweenArguments["x"];
			}
			if (tweenArguments.Contains("y")) {
				vector3s[1].y=(float)tweenArguments["y"];
			}
			if (tweenArguments.Contains("z")) {
				vector3s[1].z=(float)tweenArguments["z"];
			}
		}
		
		//shortest distance:
		vector3s[1]=new Vector3(clerp(vector3s[0].x,vector3s[1].x,1),clerp(vector3s[0].y,vector3s[1].y,1),clerp(vector3s[0].z,vector3s[1].z,1));
		
		//need for speed?
		if(tweenArguments.Contains("speed")){
			float distance = Math.Abs(Vector3.Distance(vector3s[0],vector3s[1]));
			time = distance/(float)tweenArguments["speed"];
		}
	}
	

	void GenerateRotateByTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation, [3] previous value for Rotate usage to allow Space utilization:
		vector3s=new Vector3[4];
		
		//from values:
		vector3s[0]=vector3s[1]=vector3s[3]=thisTransform.eulerAngles;
		
		//to values:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]+=Vector3.Scale((Vector3)tweenArguments["amount"],new Vector3(360,360,360));
		}else{
			if (tweenArguments.Contains("x")) {
				vector3s[1].x+=360 * (float)tweenArguments["x"];
			}
			if (tweenArguments.Contains("y")) {
				vector3s[1].y+=360 * (float)tweenArguments["y"];
			}
			if (tweenArguments.Contains("z")) {
				vector3s[1].z+=360 * (float)tweenArguments["z"];
			}
		}
		
		//need for speed?
		if(tweenArguments.Contains("speed")){
			float distance = Math.Abs(Vector3.Distance(vector3s[0],vector3s[1]));
			time = distance/(float)tweenArguments["speed"];
		}
	}
	#endregion
	
	#region #4 Apply Targets
	
	void ApplyRectTargets(){
		//calculate:
		rects[2].x = ease(rects[0].x,rects[1].x,percentage);
		rects[2].y = ease(rects[0].y,rects[1].y,percentage);
		rects[2].width = ease(rects[0].width,rects[1].width,percentage);
		rects[2].height = ease(rects[0].height,rects[1].height,percentage);
		
		//apply:
		tweenArguments["onupdateparams"]=rects[2];
		
		//dial in:
		if(percentage==1){
			tweenArguments["onupdateparams"]=rects[1];
		}
	}		
	
	void ApplyVector3Targets(){
		//calculate:
		vector3s[2].x = ease(vector3s[0].x,vector3s[1].x,percentage);
		vector3s[2].y = ease(vector3s[0].y,vector3s[1].y,percentage);
		vector3s[2].z = ease(vector3s[0].z,vector3s[1].z,percentage);
		
		//apply:
		tweenArguments["onupdateparams"]=vector3s[2];
		
		//dial in:
		if(percentage==1){
			tweenArguments["onupdateparams"]=vector3s[1];
		}
	}		
	
	void ApplyVector2Targets(){
		//calculate:
		vector2s[2].x = ease(vector2s[0].x,vector2s[1].x,percentage);
		vector2s[2].y = ease(vector2s[0].y,vector2s[1].y,percentage);
		
		//apply:
		tweenArguments["onupdateparams"]=vector2s[2];
		
		//dial in:
		if(percentage==1){
			tweenArguments["onupdateparams"]=vector2s[1];
		}
	}	
	
	void ApplyFloatTargets(){
		//calculate:
		floats[2] = ease(floats[0],floats[1],percentage);
		
		//apply:
		tweenArguments["onupdateparams"]=floats[2];
		
		//dial in:
		if(percentage==1){
			tweenArguments["onupdateparams"]=floats[1];
		}
	}	
	
	void ApplyMoveToTargets(){
		//record current:
		preUpdate=thisTransform.position;
			
		
		//calculate:
		vector3s[2].x = ease(vector3s[0].x,vector3s[1].x,percentage);
		vector3s[2].y = ease(vector3s[0].y,vector3s[1].y,percentage);
		vector3s[2].z = ease(vector3s[0].z,vector3s[1].z,percentage);
		
		//apply:	
		if (isLocal) {
			thisTransform.localPosition=vector3s[2];
		}else{
			thisTransform.position=vector3s[2];
		}
			
		//dial in:
		if(percentage==1){
			if (isLocal) {
				thisTransform.localPosition=vector3s[1];		
			}else{
				thisTransform.position=vector3s[1];
			}
		}
			
		//need physics?
		postUpdate=thisTransform.position;
		if(physics){
			thisTransform.position=preUpdate;
			GetComponent<Rigidbody>().MovePosition(postUpdate);
		}
	}	
	
	void ApplyMoveByTargets(){	
		preUpdate = thisTransform.position;
		
		//reset rotation to prevent look interferences as object rotates and attempts to move with translate and record current rotation
		Vector3 currentRotation = new Vector3();
		
		if(tweenArguments.Contains("looktarget")){
			currentRotation = thisTransform.eulerAngles;
			thisTransform.eulerAngles = vector3s[4];	
		}
		
		//calculate:
		vector3s[2].x = ease(vector3s[0].x,vector3s[1].x,percentage);
		vector3s[2].y = ease(vector3s[0].y,vector3s[1].y,percentage);
		vector3s[2].z = ease(vector3s[0].z,vector3s[1].z,percentage);
				
		//apply:
		thisTransform.Translate(vector3s[2]-vector3s[3],space);
		
		//record:
		vector3s[3]=vector3s[2];
		
		//reset rotation:
		if(tweenArguments.Contains("looktarget")){
			thisTransform.eulerAngles = currentRotation;	
		}
				
		/*
		//dial in:
		if(percentage==1){	
			transform.position=vector3s[5];
		}
		*/
		
		//need physics?
		postUpdate=thisTransform.position;
		if(physics){
			thisTransform.position=preUpdate;
			GetComponent<Rigidbody>().MovePosition(postUpdate);
		}
	}	
	
	void ApplyScaleToTargets(){
		//calculate:
		vector3s[2].x = ease(vector3s[0].x,vector3s[1].x,percentage);
		vector3s[2].y = ease(vector3s[0].y,vector3s[1].y,percentage);
		vector3s[2].z = ease(vector3s[0].z,vector3s[1].z,percentage);
		
		//apply:
		thisTransform.localScale=vector3s[2];	
		
		//dial in:
		if(percentage==1){
			thisTransform.localScale=vector3s[1];
		}
	}
	
	void ApplyLookToTargets(){
		//calculate:
		vector3s[2].x = ease(vector3s[0].x,vector3s[1].x,percentage);
		vector3s[2].y = ease(vector3s[0].y,vector3s[1].y,percentage);
		vector3s[2].z = ease(vector3s[0].z,vector3s[1].z,percentage);
		
		//apply:
		if (isLocal) {
			thisTransform.localRotation = Quaternion.Euler(vector3s[2]);
		}else{
			thisTransform.rotation = Quaternion.Euler(vector3s[2]);
		};	
	}	
	
	void ApplyRotateToTargets(){
		preUpdate=thisTransform.eulerAngles;
		
		//calculate:
		vector3s[2].x = ease(vector3s[0].x,vector3s[1].x,percentage);
		vector3s[2].y = ease(vector3s[0].y,vector3s[1].y,percentage);
		vector3s[2].z = ease(vector3s[0].z,vector3s[1].z,percentage);
		
		//apply:
		if (isLocal) {
			thisTransform.localRotation = Quaternion.Euler(vector3s[2]);
		}else{
			thisTransform.rotation = Quaternion.Euler(vector3s[2]);
		};	
		
		//dial in:
		if(percentage==1){
			if (isLocal) {
				thisTransform.localRotation = Quaternion.Euler(vector3s[1]);
			}else{
				thisTransform.rotation = Quaternion.Euler(vector3s[1]);
			};
		}
		
		//need physics?
		postUpdate=thisTransform.eulerAngles;
		if(physics){
			thisTransform.eulerAngles=preUpdate;
			GetComponent<Rigidbody>().MoveRotation(Quaternion.Euler(postUpdate));
		}
	}
	
	void ApplyRotateAddTargets(){
		preUpdate = thisTransform.eulerAngles;
		
		//calculate:
		vector3s[2].x = ease(vector3s[0].x,vector3s[1].x,percentage);
		vector3s[2].y = ease(vector3s[0].y,vector3s[1].y,percentage);
		vector3s[2].z = ease(vector3s[0].z,vector3s[1].z,percentage);
		
		//apply:
		thisTransform.Rotate(vector3s[2]-vector3s[3],space);

		//record:
		vector3s[3]=vector3s[2];	
		
		//need physics?
		postUpdate=thisTransform.eulerAngles;
		if(physics){
			thisTransform.eulerAngles=preUpdate;
			GetComponent<Rigidbody>().MoveRotation(Quaternion.Euler(postUpdate));
		}		
	}	
	#endregion	
	
	#region #5 Tween Steps
	
	IEnumerator TweenDelay(){
		delayStarted = Time.time;
		yield return new WaitForSeconds (delay);
		if(wasPaused){
			wasPaused=false;
			TweenStart();	
		}
	}	
	
	void TweenStart(){		
		CallBack("onstart");
		
		if(!loop){//only if this is not a loop
			ConflictCheck();
			GenerateTargets();
		}
		isRunning = true;
	}
	
	IEnumerator TweenRestart(){
		if(delay > 0){
			delayStarted = Time.time;
			yield return new WaitForSeconds (delay);
		}
		loop=true;
		TweenStart();
	}	
	
	void TweenUpdate(){
		apply();
		CallBack("onupdate");
		UpdatePercentage();		
	}
			
	void TweenComplete(){
		isRunning=false;
		
		//dial in percentage to 1 or 0 for final run:
		if(percentage>.5f){
			percentage=1f;
		}else{
			percentage=0;	
		}
		
		//apply dial in and final run:
		apply();
		if(type == "value"){
			CallBack("onupdate"); //CallBack run for ValueTo since it only calculates and applies in the update callback
		}
		
		//loop or dispose?
		if(loopType==LoopType.none){
			Dispose();
		}else{
			TweenLoop();
		}
		
		CallBack("oncomplete");
	}
	
	void TweenLoop(){
		switch(loopType){
			case LoopType.loop:
				//rewind:
				percentage=0;
				runningTime=0;
				apply();
				
				//replay:
				StartCoroutine("TweenRestart");
				break;
			case LoopType.pingPong:
				reverse = !reverse;
				runningTime=0;
			
				//replay:
				StartCoroutine("TweenRestart");
				break;
		}
	}	
	
	#endregion
	
	#region #6 Update Callable
	
	/// <summary>
	/// Returns a Rect that is eased between a current and target value by the supplied speed.
	/// </summary>
	/// <returns>
	/// A <see cref="Rect"/
	/// </returns>
	/// <param name='currentValue'>
	/// A <see cref="Rect"/> the starting or initial value
	/// </param>
	/// <param name='targetValue'>
	/// A <see cref="Rect"/> the target value that the current value will be eased to.
	/// </param>
	/// <param name='speed'>
	/// A <see cref="System.Single"/> to be used as rate of speed (larger number equals faster animation)
	/// </param>
	public static Rect RectUpdate(Rect currentValue, Rect targetValue, float speed){
		Rect diff = new Rect(FloatUpdate(currentValue.x, targetValue.x, speed), FloatUpdate(currentValue.y, targetValue.y, speed), FloatUpdate(currentValue.width, targetValue.width, speed), FloatUpdate(currentValue.height, targetValue.height, speed));
		return (diff);
	}
	
	/// <summary>
	/// Returns a Vector3 that is eased between a current and target value by the supplied speed.
	/// </summary>
	/// <returns>
	/// A <see cref="Vector3"/>
	/// </returns>
	/// <param name='currentValue'>
	/// A <see cref="Vector3"/> the starting or initial value
	/// </param>
	/// <param name='targetValue'>
	/// A <see cref="Vector3"/> the target value that the current value will be eased to.
	/// </param>
	/// <param name='speed'>
	/// A <see cref="System.Single"/> to be used as rate of speed (larger number equals faster animation)
	/// </param>
	public static Vector3 Vector3Update(Vector3 currentValue, Vector3 targetValue, float speed){
		Vector3 diff = targetValue - currentValue;
		currentValue += (diff * speed) * Time.deltaTime;
		return (currentValue);
	}
	
	/// <summary>
	/// Returns a Vector2 that is eased between a current and target value by the supplied speed.
	/// </summary>
	/// <returns>
	/// A <see cref="Vector2"/>
	/// </returns>
	/// <param name='currentValue'>
	/// A <see cref="Vector2"/> the starting or initial value
	/// </param>
	/// <param name='targetValue'>
	/// A <see cref="Vector2"/> the target value that the current value will be eased to.
	/// </param>
	/// <param name='speed'>
	/// A <see cref="System.Single"/> to be used as rate of speed (larger number equals faster animation)
	/// </param>
	public static Vector2 Vector2Update(Vector2 currentValue, Vector2 targetValue, float speed){
		Vector2 diff = targetValue - currentValue;
		currentValue += (diff * speed) * Time.deltaTime;
		return (currentValue);
	}
	
	/// <summary>
	/// Returns a float that is eased between a current and target value by the supplied speed.
	/// </summary>
	/// <returns>
	/// A <see cref="System.Single"/>
	/// </returns>
	/// <param name='currentValue'>
	/// A <see cref="System.Single"/> the starting or initial value
	/// </param>
	/// <param name='targetValue'>
	/// A <see cref="System.Single"/> the target value that the current value will be eased to.
	/// </param>
	/// <param name='speed'>
	/// A <see cref="System.Single"/> to be used as rate of speed (larger number equals faster animation)
	/// </param>
	public static float FloatUpdate(float currentValue, float targetValue, float speed){
		float diff = targetValue - currentValue;
		currentValue += (diff * speed) * Time.deltaTime;
		return (currentValue);
	}
	
	/// <summary>
	/// Similar to FadeTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with FULL customization options. Does not utilize an EaseType. 
	/// </summary>
	/// <param name="alpha">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the final alpha value of the animation.
	/// </param>
	/// <param name="includechildren">
	/// A <see cref="System.Boolean"/> for whether or not to include children of this GameObject. True by default.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void FadeUpdate(GameObject target, Hashtable args){
		args["a"]=args["alpha"];
		ColorUpdate(target,args);
	}
	
	/// <summary>
	/// Similar to FadeTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with MINIMUM customization options. Does not utilize an EaseType. 
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="alpha">
	/// A <see cref="System.Single"/> for the final alpha value of the animation.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void FadeUpdate(GameObject target, float alpha, float time){
		FadeUpdate(target,Hash("alpha",alpha,"time",time));
	}
	
	/// <summary>
	/// Similar to ColorTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with FULL customization options. Does not utilize an EaseType. 
	/// </summary>
	/// <param name="color">
	/// A <see cref="Color"/> to change the GameObject's color to.
	/// </param>
	/// <param name="r">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the color red.
	/// </param>
	/// <param name="g">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the color green.
	/// </param>
	/// <param name="b">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the color green.
	/// </param>
	/// <param name="a">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the alpha.
	/// </param> 
	/// <param name="namedcolorvalue">
	/// A <see cref="NamedColorValue"/> or <see cref="System.String"/> for the individual setting of the alpha.
	/// </param> 
	/// <param name="includechildren">
	/// A <see cref="System.Boolean"/> for whether or not to include children of this GameObject. True by default.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void ColorUpdate(GameObject target, Hashtable args){
		CleanArgs(args);
		
		float time;
		Color[] colors = new Color[4];
		
		//handle children:
		if(!args.Contains("includechildren") || (bool)args["includechildren"]){
			foreach(Transform child in target.transform){
				ColorUpdate(child.gameObject,args);
			}
		}		 
		
		//set smooth time:
		if(args.Contains("time")){
			time=(float)args["time"];
			time*=Defaults.updateTimePercentage;
		}else{
			time=Defaults.updateTime;
		}
		
		//init values:
		if(target.GetComponent<GUITexture>()){
			colors[0] = colors[1] = target.GetComponent<GUITexture>().color;
		}else if(target.GetComponent<GUIText>()){
			colors[0] = colors[1] = target.GetComponent<GUIText>().material.color;
		}else if(target.GetComponent<Renderer>()){
			colors[0] = colors[1] = target.GetComponent<Renderer>().material.color;
		}else if(target.GetComponent<Light>()){
			colors[0] = colors[1] = target.GetComponent<Light>().color;	
		}		
		
		//to values:
		if (args.Contains("color")) {
			colors[1]=(Color)args["color"];
		}else{
			if (args.Contains("r")) {
				colors[1].r=(float)args["r"];
			}
			if (args.Contains("g")) {
				colors[1].g=(float)args["g"];
			}
			if (args.Contains("b")) {
				colors[1].b=(float)args["b"];
			}
			if (args.Contains("a")) {
				colors[1].a=(float)args["a"];
			}
		}
		
		//calculate:
		colors[3].r=Mathf.SmoothDamp(colors[0].r,colors[1].r,ref colors[2].r,time);
		colors[3].g=Mathf.SmoothDamp(colors[0].g,colors[1].g,ref colors[2].g,time);
		colors[3].b=Mathf.SmoothDamp(colors[0].b,colors[1].b,ref colors[2].b,time);
		colors[3].a=Mathf.SmoothDamp(colors[0].a,colors[1].a,ref colors[2].a,time);
				
		//apply:
		if(target.GetComponent<GUITexture>()){
			target.GetComponent<GUITexture>().color=colors[3];
		}else if(target.GetComponent<GUIText>()){
			target.GetComponent<GUIText>().material.color=colors[3];
		}else if(target.GetComponent<Renderer>()){
			target.GetComponent<Renderer>().material.color=colors[3];
		}else if(target.GetComponent<Light>()){
			target.GetComponent<Light>().color=colors[3];	
		}
	}	
	
	/// <summary>
	/// Similar to ColorTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with MINIMUM customization options. Does not utilize an EaseType.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="color">
	/// A <see cref="Color"/> to change the GameObject's color to.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void ColorUpdate(GameObject target, Color color, float time){
		ColorUpdate(target,Hash("color",color,"time",time));
	}
	
	/// <summary>
	/// Similar to AudioTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with FULL customization options. Does not utilize an EaseType. 
	/// </summary>
	/// <param name="audiosource">
	/// A <see cref="AudioSource"/> for which AudioSource to use.
	/// </param> 
	/// <param name="volume">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the target level of volume.
	/// </param>
	/// <param name="pitch">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the target pitch.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void AudioUpdate(GameObject target, Hashtable args){
		CleanArgs(args);
		
		AudioSource audioSource;
		float time;
		Vector2[] vector2s = new Vector2[4];
			
		//set smooth time:
		if(args.Contains("time")){
			time=(float)args["time"];
			time*=Defaults.updateTimePercentage;
		}else{
			time=Defaults.updateTime;
		}

		//set audioSource:
		if(args.Contains("audiosource")){
			audioSource=(AudioSource)args["audiosource"];
		}else{
			if(target.GetComponent<AudioSource>()){
				audioSource=target.GetComponent<AudioSource>();
			}else{
				//throw error if no AudioSource is available:
				Debug.LogError("EGTween Error: AudioUpdate requires an AudioSource.");
				return;
			}
		}		
		
		//from values:
		vector2s[0] = vector2s[1] = new Vector2(audioSource.volume,audioSource.pitch);
		
		//set to:
		if(args.Contains("volume")){
			vector2s[1].x=(float)args["volume"];
		}
		if(args.Contains("pitch")){
			vector2s[1].y=(float)args["pitch"];
		}
		
		//calculate:
		vector2s[3].x=Mathf.SmoothDampAngle(vector2s[0].x,vector2s[1].x,ref vector2s[2].x,time);
		vector2s[3].y=Mathf.SmoothDampAngle(vector2s[0].y,vector2s[1].y,ref vector2s[2].y,time);
	
		//apply:
		audioSource.volume=vector2s[3].x;
		audioSource.pitch=vector2s[3].y;
	}
	
	/// <summary>
	/// Similar to AudioTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with MINIMUM customization options. Does not utilize an EaseType. 
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="volume">
	/// A <see cref="System.Single"/> for the target level of volume.
	/// </param>
	/// <param name="pitch">
	/// A <see cref="System.Single"/> for the target pitch.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void AudioUpdate(GameObject target, float volume, float pitch, float time){
		AudioUpdate(target,Hash("volume",volume,"pitch",pitch,"time",time));
	}
	
	/// <summary>
	/// Similar to RotateTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with FULL customization options. Does not utilize an EaseType. 
	/// </summary>
	/// <param name="rotation">
	/// A <see cref="Transform"/> or <see cref="Vector3"/> for the target Euler angles in degrees to rotate to.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="islocal">
	/// A <see cref="System.Boolean"/> for whether to animate in world space or relative to the parent. False by default.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param> 
	public static void RotateUpdate(GameObject target, Hashtable args){
		CleanArgs(args);
		
		bool isLocal;
		float time;
		Vector3[] vector3s = new Vector3[4];
		Vector3 preUpdate = target.transform.eulerAngles;
		
		//set smooth time:
		if(args.Contains("time")){
			time=(float)args["time"];
			time*=Defaults.updateTimePercentage;
		}else{
			time=Defaults.updateTime;
		}
		
		//set isLocal:
		if(args.Contains("islocal")){
			isLocal = (bool)args["islocal"];
		}else{
			isLocal = Defaults.isLocal;	
		}
		
		//from values:
		if(isLocal){
			vector3s[0] = target.transform.localEulerAngles;
		}else{
			vector3s[0] = target.transform.eulerAngles;	
		}
		
		//set to:
		if(args.Contains("rotation")){
			if (args["rotation"].GetType() == typeof(Transform)){
				Transform trans = (Transform)args["rotation"];
				vector3s[1]=trans.eulerAngles;
			}else if(args["rotation"].GetType() == typeof(Vector3)){
				vector3s[1]=(Vector3)args["rotation"];
			}	
		}
				
		//calculate:
		vector3s[3].x=Mathf.SmoothDampAngle(vector3s[0].x,vector3s[1].x,ref vector3s[2].x,time);
		vector3s[3].y=Mathf.SmoothDampAngle(vector3s[0].y,vector3s[1].y,ref vector3s[2].y,time);
		vector3s[3].z=Mathf.SmoothDampAngle(vector3s[0].z,vector3s[1].z,ref vector3s[2].z,time);
	
		//apply:
		if(isLocal){
			target.transform.localEulerAngles=vector3s[3];
		}else{
			target.transform.eulerAngles=vector3s[3];
		}
		
		//need physics?
		if(target.GetComponent<Rigidbody>() != null){
			Vector3 postUpdate=target.transform.eulerAngles;
			target.transform.eulerAngles=preUpdate;
			target.GetComponent<Rigidbody>().MoveRotation(Quaternion.Euler(postUpdate));
		}
	}
		
	/// <summary>
	/// Similar to RotateTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with MINIMUM customization options. Does not utilize an EaseType. 
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="rotation">
	/// A <see cref="Vector3"/> for the target Euler angles in degrees to rotate to.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void RotateUpdate(GameObject target, Vector3 rotation, float time){
		RotateUpdate(target,Hash("rotation",rotation,"time",time));
	}
	
	/// <summary>
	/// Similar to ScaleTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with FULL customization options.  Does not utilize an EaseType. 
	/// </summary>
	/// <param name="scale">
	/// A <see cref="Transform"/> or <see cref="Vector3"/> for the final scale.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param> 
	public static void ScaleUpdate(GameObject target, Hashtable args){
		CleanArgs(args);
		
		float time;
		Vector3[] vector3s = new Vector3[4];
			
		//set smooth time:
		if(args.Contains("time")){
			time=(float)args["time"];
			time*=Defaults.updateTimePercentage;
		}else{
			time=Defaults.updateTime;
		}
		
		//init values:
		vector3s[0] = vector3s[1] = target.transform.localScale;
		
		//to values:
		if (args.Contains("scale")) {
			if (args["scale"].GetType() == typeof(Transform)){
				Transform trans = (Transform)args["scale"];
				vector3s[1]=trans.localScale;
			}else if(args["scale"].GetType() == typeof(Vector3)){
				vector3s[1]=(Vector3)args["scale"];
			}				
		}else{
			if (args.Contains("x")) {
				vector3s[1].x=(float)args["x"];
			}
			if (args.Contains("y")) {
				vector3s[1].y=(float)args["y"];
			}
			if (args.Contains("z")) {
				vector3s[1].z=(float)args["z"];
			}
		}
		
		//calculate:
		vector3s[3].x=Mathf.SmoothDamp(vector3s[0].x,vector3s[1].x,ref vector3s[2].x,time);
		vector3s[3].y=Mathf.SmoothDamp(vector3s[0].y,vector3s[1].y,ref vector3s[2].y,time);
		vector3s[3].z=Mathf.SmoothDamp(vector3s[0].z,vector3s[1].z,ref vector3s[2].z,time);
				
		//apply:
		target.transform.localScale=vector3s[3];		
	}	
	
	/// <summary>
	/// Similar to ScaleTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with MINIMUM customization options.  Does not utilize an EaseType.
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="scale">
	/// A <see cref="Vector3"/> for the final scale.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void ScaleUpdate(GameObject target, Vector3 scale, float time){
		ScaleUpdate(target,Hash("scale",scale,"time",time));
	}
	
	/// <summary>
	/// Similar to MoveTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with FULL customization options. Does not utilize an EaseType. 
	/// </summary>
	/// <param name="position">
	/// A <see cref="Transform"/> or <see cref="Vector3"/> for a point in space the GameObject will animate to.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the x axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the y axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the individual setting of the z axis.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param> 
	/// <param name="islocal">
	/// A <see cref="System.Boolean"/> for whether to animate in world space or relative to the parent. False by default.
	/// </param>
	/// <param name="orienttopath">
	/// A <see cref="System.Boolean"/> for whether or not the GameObject will orient to its direction of travel.  False by default.
	/// </param>
	/// <param name="looktarget">
	/// A <see cref="Vector3"/> or A <see cref="Transform"/> for a target the GameObject will look at.
	/// </param>
	/// <param name="looktime">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the object will take to look at either the "looktarget" or "orienttopath".
	/// </param>
	/// <param name="axis">
	/// A <see cref="System.String"/>. Restricts rotation to the supplied axis only.
	/// </param>
	public static void MoveUpdate(GameObject target, Hashtable args){
		CleanArgs(args);
		
		float time;
		Vector3[] vector3s = new Vector3[4];
		bool isLocal;
		Vector3 preUpdate = target.transform.position;
			
		//set smooth time:
		if(args.Contains("time")){
			time=(float)args["time"];
			time*=Defaults.updateTimePercentage;
		}else{
			time=Defaults.updateTime;
		}
			
		//set isLocal:
		if(args.Contains("islocal")){
			isLocal = (bool)args["islocal"];
		}else{
			isLocal = Defaults.isLocal;	
		}
		 
		//init values:
		if(isLocal){
			vector3s[0] = vector3s[1] = target.transform.localPosition;
		}else{
			vector3s[0] = vector3s[1] = target.transform.position;	
		}
		
		//to values:
		if (args.Contains("position")) {
			if (args["position"].GetType() == typeof(Transform)){
				Transform trans = (Transform)args["position"];
				vector3s[1]=trans.position;
			}else if(args["position"].GetType() == typeof(Vector3)){
				vector3s[1]=(Vector3)args["position"];
			}			
		}else{
			if (args.Contains("x")) {
				vector3s[1].x=(float)args["x"];
			}
			if (args.Contains("y")) {
				vector3s[1].y=(float)args["y"];
			}
			if (args.Contains("z")) {
				vector3s[1].z=(float)args["z"];
			}
		}
		
		//calculate:
		vector3s[3].x=Mathf.SmoothDamp(vector3s[0].x,vector3s[1].x,ref vector3s[2].x,time);
		vector3s[3].y=Mathf.SmoothDamp(vector3s[0].y,vector3s[1].y,ref vector3s[2].y,time);
		vector3s[3].z=Mathf.SmoothDamp(vector3s[0].z,vector3s[1].z,ref vector3s[2].z,time);
			
		//handle orient to path:
		if(args.Contains("orienttopath") && (bool)args["orienttopath"]){
			args["looktarget"] = vector3s[3];
		}
		
		//look applications:
		if(args.Contains("looktarget")){
			EGTween.LookUpdate(target,args);
		}
		
		//apply:
		if(isLocal){
			target.transform.localPosition = vector3s[3];			
		}else{
			target.transform.position=vector3s[3];	
		}	
		
		//need physics?
		if(target.GetComponent<Rigidbody>() != null){
			Vector3 postUpdate=target.transform.position;
			target.transform.position=preUpdate;
			target.GetComponent<Rigidbody>().MovePosition(postUpdate);
		}
	}

	/// <summary>
	/// Similar to MoveTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with MINIMUM customization options. Does not utilize an EaseType. 
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="position">
	/// A <see cref="Vector3"/> for a point in space the GameObject will animate to.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void MoveUpdate(GameObject target, Vector3 position, float time){
		MoveUpdate(target,Hash("position",position,"time",time));
	}
	
	/// <summary>
	/// Similar to LookTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with FULL customization options. Does not utilize an EaseType. 
	/// </summary>
	/// <param name="looktarget">
	/// A <see cref="Transform"/> or <see cref="Vector3"/> for a target the GameObject will look at.
	/// </param>
	/// <param name="axis">
	/// A <see cref="System.String"/>. Restricts rotation to the supplied axis only.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/> for the time in seconds the animation will take to complete.
	/// </param> 
	public static void LookUpdate(GameObject target, Hashtable args){
		CleanArgs(args);
		
		float time;
		Vector3[] vector3s = new Vector3[5];
		
		//set smooth time:
		if(args.Contains("looktime")){
			time=(float)args["looktime"];
			time*=Defaults.updateTimePercentage;
		}else if(args.Contains("time")){
			time=(float)args["time"]*.15f;
			time*=Defaults.updateTimePercentage;
		}else{
			time=Defaults.updateTime;
		}
		
		//from values:
		vector3s[0] = target.transform.eulerAngles;
		
		//set look:
		if(args.Contains("looktarget")){
			if (args["looktarget"].GetType() == typeof(Transform)) {
				//target.transform.LookAt((Transform)args["looktarget"]);
				target.transform.LookAt((Transform)args["looktarget"], (Vector3?)args["up"] ?? Defaults.up);
			}else if(args["looktarget"].GetType() == typeof(Vector3)){
				//target.transform.LookAt((Vector3)args["looktarget"]);
				target.transform.LookAt((Vector3)args["looktarget"], (Vector3?)args["up"] ?? Defaults.up);
			}
		}else{
			Debug.LogError("EGTween Error: LookUpdate needs a 'looktarget' property!");
			return;
		}
		
		//to values and reset look:
		vector3s[1]=target.transform.eulerAngles;
		target.transform.eulerAngles=vector3s[0];
		
		//calculate:
		vector3s[3].x=Mathf.SmoothDampAngle(vector3s[0].x,vector3s[1].x,ref vector3s[2].x,time);
		vector3s[3].y=Mathf.SmoothDampAngle(vector3s[0].y,vector3s[1].y,ref vector3s[2].y,time);
		vector3s[3].z=Mathf.SmoothDampAngle(vector3s[0].z,vector3s[1].z,ref vector3s[2].z,time);
	
		//apply:
		target.transform.eulerAngles=vector3s[3];
		
		//axis restriction:
		if(args.Contains("axis")){
			vector3s[4]=target.transform.eulerAngles;
			switch((string)args["axis"]){
				case "x":
					vector3s[4].y=vector3s[0].y;
					vector3s[4].z=vector3s[0].z;
				break;
				case "y":
					vector3s[4].x=vector3s[0].x;
					vector3s[4].z=vector3s[0].z;
				break;
				case "z":
					vector3s[4].x=vector3s[0].x;
					vector3s[4].y=vector3s[0].y;
				break;
			}
			
			//apply axis restriction:
			target.transform.eulerAngles=vector3s[4];
		}	
	}
	
	/// <summary>
	/// Similar to LookTo but incredibly less expensive for usage inside the Update function or similar looping situations involving a "live" set of changing values with FULL customization options. Does not utilize an EaseType. 
	/// </summary>
	/// <param name="target">
	/// A <see cref="GameObject"/> to be the target of the animation.
	/// </param>
	/// <param name="looktarget">
	/// A <see cref="Vector3"/> for a target the GameObject will look at.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> for the time in seconds the animation will take to complete.
	/// </param>
	public static void LookUpdate(GameObject target, Vector3 looktarget, float time){
		LookUpdate(target,Hash("looktarget",looktarget,"time",time));
	}

	#endregion
	
	#region #7 External Utilities
	
	/// <summary>
	/// Returns the length of a curved path drawn through the provided array of Transforms.
	/// </summary>
	/// <returns>
	/// A <see cref="System.Single"/>
	/// </returns>
	/// <param name='path'>
	/// A <see cref="Transform[]"/>
	/// </param>
	public static float PathLength(Transform[] path){
		Vector3[] suppliedPath = new Vector3[path.Length];
		float pathLength = 0;
		
		//create and store path points:
		for (int i = 0; i < path.Length; i++) {
			suppliedPath[i]=path[i].position;
		}
		
		Vector3[] vector3s = PathControlPointGenerator(suppliedPath);
		
		//Line Draw:
		Vector3 prevPt = Interp(vector3s,0);
		int SmoothAmount = path.Length*20;
		for (int i = 1; i <= SmoothAmount; i++) {
			float pm = (float) i / SmoothAmount;
			Vector3 currPt = Interp(vector3s,pm);
			pathLength += Vector3.Distance(prevPt,currPt);
			prevPt = currPt;
		}
		
		return pathLength;
	}
	
	/// <summary>
	/// Returns the length of a curved path drawn through the provided array of Vector3s.
	/// </summary>
	/// <returns>
	/// The length.
	/// </returns>
	/// <param name='path'>
	/// A <see cref="Vector3[]"/>
	/// </param>
	public static float PathLength(Vector3[] path){
		float pathLength = 0;
		
		Vector3[] vector3s = PathControlPointGenerator(path);
		
		//Line Draw:
		Vector3 prevPt = Interp(vector3s,0);
		int SmoothAmount = path.Length*20;
		for (int i = 1; i <= SmoothAmount; i++) {
			float pm = (float) i / SmoothAmount;
			Vector3 currPt = Interp(vector3s,pm);
			pathLength += Vector3.Distance(prevPt,currPt);
			prevPt = currPt;
		}
		
		return pathLength;
	}	
	
	/// <summary>
	/// Creates and returns a full-screen Texture2D for use with CameraFade.
	/// </summary>
	/// <returns>
	/// Texture2D
	/// </returns>
	/// <param name='color'>
	/// Color
	/// </param>
	public static Texture2D CameraTexture(Color color){
		Texture2D texture = new Texture2D(Screen.width,Screen.height,TextureFormat.ARGB32, false);
		Color[] colors = new Color[Screen.width*Screen.height];
		for (int i = 0; i < colors.Length; i++) {
			colors[i]=color;
		}
		texture.SetPixels(colors);
		texture.Apply();
		return(texture);		
	}
	
	/// <summary>
	/// When called from an OnDrawGizmos() function it will draw a line through the provided array of Vector3s.
	/// </summary>
	/// <param name="line">
	/// A <see cref="Vector3s[]"/>
	/// </param>
	public static void DrawLine(Vector3[] line) {
		if(line.Length>0){
			DrawLineHelper(line,Defaults.color,"gizmos");
		}
	}	
	
	/// <summary>
	/// When called from an OnDrawGizmos() function it will draw a line through the provided array of Vector3s.
	/// </summary>
	/// <param name="line">
	/// A <see cref="Vector3s[]"/>
	/// </param>
	/// <param name="color">
	/// A <see cref="Color"/>
	/// </param> 
	public static void DrawLine(Vector3[] line, Color color) {
		if(line.Length>0){
			DrawLineHelper(line,color,"gizmos");
		}
	}		
	
	/// <summary>
	/// When called from an OnDrawGizmos() function it will draw a line through the provided array of Transforms.
	/// </summary>
	/// <param name="line">
	/// A <see cref="Transform[]"/>
	/// </param>
	public static void DrawLine(Transform[] line) {
		if(line.Length>0){
			//create and store line points:
			Vector3[] suppliedLine = new Vector3[line.Length];
			for (int i = 0; i < line.Length; i++) {
				suppliedLine[i]=line[i].position;
			}
			DrawLineHelper(suppliedLine,Defaults.color,"gizmos");
		}
	}		
	
	/// <summary>
	/// When called from an OnDrawGizmos() function it will draw a line through the provided array of Transforms.
	/// </summary>
	/// <param name="line">
	/// A <see cref="Transform[]"/>
	/// </param>
	/// <param name="color">
	/// A <see cref="Color"/>
	/// </param> 
	public static void DrawLine(Transform[] line,Color color) {
		if(line.Length>0){
			//create and store line points:
			Vector3[] suppliedLine = new Vector3[line.Length];
			for (int i = 0; i < line.Length; i++) {
				suppliedLine[i]=line[i].position;
			}
			
			DrawLineHelper(suppliedLine, color,"gizmos");
		}
	}	
	
	/// <summary>
	/// Draws a line through the provided array of Vector3s with Gizmos.DrawLine().
	/// </summary>
	/// <param name="line">
	/// A <see cref="Vector3s[]"/>
	/// </param>
	public static void DrawLineGizmos(Vector3[] line) {
		if(line.Length>0){
			DrawLineHelper(line,Defaults.color,"gizmos");
		}
	}	
	
	/// <summary>
	/// Draws a line through the provided array of Vector3s with Gizmos.DrawLine().
	/// </summary>
	/// <param name="line">
	/// A <see cref="Vector3s[]"/>
	/// </param>
	/// <param name="color">
	/// A <see cref="Color"/>
	/// </param> 
	public static void DrawLineGizmos(Vector3[] line, Color color) {
		if(line.Length>0){
			DrawLineHelper(line,color,"gizmos");
		}
	}		
	
	/// <summary>
	/// Draws a line through the provided array of Transforms with Gizmos.DrawLine().
	/// </summary>
	/// <param name="line">
	/// A <see cref="Transform[]"/>
	/// </param>
	public static void DrawLineGizmos(Transform[] line) {
		if(line.Length>0){
			//create and store line points:
			Vector3[] suppliedLine = new Vector3[line.Length];
			for (int i = 0; i < line.Length; i++) {
				suppliedLine[i]=line[i].position;
			}
			DrawLineHelper(suppliedLine,Defaults.color,"gizmos");
		}
	}		
	
	/// <summary>
	/// Draws a line through the provided array of Transforms with Gizmos.DrawLine().
	/// </summary>
	/// <param name="line">
	/// A <see cref="Transform[]"/>
	/// </param>
	/// <param name="color">
	/// A <see cref="Color"/>
	/// </param> 
	public static void DrawLineGizmos(Transform[] line,Color color) {
		if(line.Length>0){
			//create and store line points:
			Vector3[] suppliedLine = new Vector3[line.Length];
			for (int i = 0; i < line.Length; i++) {
				suppliedLine[i]=line[i].position;
			}
			
			DrawLineHelper(suppliedLine, color,"gizmos");
		}
	}

	/// <summary>
	/// Draws a line through the provided array of Vector3s with Handles.DrawLine().
	/// </summary>
	/// <param name="line">
	/// A <see cref="Vector3s[]"/>
	/// </param>
	public static void DrawLineHandles(Vector3[] line) {
		if(line.Length>0){
			DrawLineHelper(line,Defaults.color,"handles");
		}
	}	
	
	/// <summary>
	/// Draws a line through the provided array of Vector3s with Handles.DrawLine().
	/// </summary>
	/// <param name="line">
	/// A <see cref="Vector3s[]"/>
	/// </param>
	/// <param name="color">
	/// A <see cref="Color"/>
	/// </param> 
	public static void DrawLineHandles(Vector3[] line, Color color) {
		if(line.Length>0){
			DrawLineHelper(line,color,"handles");
		}
	}		
	
	/// <summary>
	/// Draws a line through the provided array of Transforms with Handles.DrawLine().
	/// </summary>
	/// <param name="line">
	/// A <see cref="Transform[]"/>
	/// </param>
	public static void DrawLineHandles(Transform[] line) {
		if(line.Length>0){
			//create and store line points:
			Vector3[] suppliedLine = new Vector3[line.Length];
			for (int i = 0; i < line.Length; i++) {
				suppliedLine[i]=line[i].position;
			}
			DrawLineHelper(suppliedLine,Defaults.color,"handles");
		}
	}		
	
	/// <summary>
	/// Draws a line through the provided array of Transforms with Handles.DrawLine().
	/// </summary>
	/// <param name="line">
	/// A <see cref="Transform[]"/>
	/// </param>
	/// <param name="color">
	/// A <see cref="Color"/>
	/// </param> 
	public static void DrawLineHandles(Transform[] line,Color color) {
		if(line.Length>0){
			//create and store line points:
			Vector3[] suppliedLine = new Vector3[line.Length];
			for (int i = 0; i < line.Length; i++) {
				suppliedLine[i]=line[i].position;
			}
			
			DrawLineHelper(suppliedLine, color,"handles");
		}
	}	
	
	/// <summary>
	/// Returns a Vector3 position on a path at the provided percentage  
	/// </summary>
	/// <param name="path">
	/// A <see cref="Vector3[]"/>
	/// </param>
	/// <param name="percent">
	/// A <see cref="System.Single"/>
	/// </param>
	/// <returns>
	/// A <see cref="Vector3"/>
	/// </returns>
	public static Vector3 PointOnPath(Vector3[] path, float percent){
		return(Interp(PathControlPointGenerator(path),percent));
	}		
	
	/// <summary>
	/// When called from an OnDrawGizmos() function it will draw a curved path through the provided array of Vector3s.
	/// </summary>
	/// <param name="path">
	/// A <see cref="Vector3s[]"/>
	/// </param>
	public static void DrawPath(Vector3[] path) {
		if(path.Length>0){
			DrawPathHelper(path,Defaults.color,"gizmos");
		}
	}		
	
	/// <summary>
	/// When called from an OnDrawGizmos() function it will draw a curved path through the provided array of Vector3s.
	/// </summary>
	/// <param name="path">
	/// A <see cref="Vector3s[]"/>
	/// </param>
	/// <param name="color">
	/// A <see cref="Color"/>
	/// </param> 
	public static void DrawPath(Vector3[] path, Color color) {
		if(path.Length>0){
			DrawPathHelper(path, color,"gizmos");
		}
	}
	
	/// <summary>
	/// When called from an OnDrawGizmos() function it will draw a curved path through the provided array of Transforms.
	/// </summary>
	/// <param name="path">
	/// A <see cref="Transform[]"/>
	/// </param>
	public static void DrawPath(Transform[] path) {
		if(path.Length>0){
			//create and store path points:
			Vector3[] suppliedPath = new Vector3[path.Length];
			for (int i = 0; i < path.Length; i++) {
				suppliedPath[i]=path[i].position;
			}
			
			DrawPathHelper(suppliedPath,Defaults.color,"gizmos");	
		}
	}		
	
	/// <summary>
	/// When called from an OnDrawGizmos() function it will draw a curved path through the provided array of Transforms.
	/// </summary>
	/// <param name="path">
	/// A <see cref="Transform[]"/>
	/// </param>
	/// <param name="color">
	/// A <see cref="Color"/>
	/// </param> 
	public static void DrawPath(Transform[] path,Color color) {
		if(path.Length>0){
			//create and store path points:
			Vector3[] suppliedPath = new Vector3[path.Length];
			for (int i = 0; i < path.Length; i++) {
				suppliedPath[i]=path[i].position;
			}
			
			DrawPathHelper(suppliedPath, color,"gizmos");
		}
	}	
	
	/// <summary>
	/// Draws a curved path through the provided array of Vector3s with Gizmos.DrawLine().
	/// </summary>
	/// <param name="path">
	/// A <see cref="Vector3s[]"/>
	/// </param>
	public static void DrawPathGizmos(Vector3[] path) {
		if(path.Length>0){
			DrawPathHelper(path,Defaults.color,"gizmos");
		}
	}		
	
	/// <summary>
	/// Draws a curved path through the provided array of Vector3s with Gizmos.DrawLine().
	/// </summary>
	/// <param name="path">
	/// A <see cref="Vector3s[]"/>
	/// </param>
	/// <param name="color">
	/// A <see cref="Color"/>
	/// </param> 
	public static void DrawPathGizmos(Vector3[] path, Color color) {
		if(path.Length>0){
			DrawPathHelper(path, color,"gizmos");
		}
	}
	
	/// <summary>
	/// Draws a curved path through the provided array of Transforms with Gizmos.DrawLine().
	/// </summary>
	/// <param name="path">
	/// A <see cref="Transform[]"/>
	/// </param>
	public static void DrawPathGizmos(Transform[] path) {
		if(path.Length>0){
			//create and store path points:
			Vector3[] suppliedPath = new Vector3[path.Length];
			for (int i = 0; i < path.Length; i++) {
				suppliedPath[i]=path[i].position;
			}
			
			DrawPathHelper(suppliedPath,Defaults.color,"gizmos");	
		}
	}		
	
	/// <summary>
	/// Draws a curved path through the provided array of Transforms with Gizmos.DrawLine().
	/// </summary>
	/// <param name="path">
	/// A <see cref="Transform[]"/>
	/// </param>
	/// <param name="color">
	/// A <see cref="Color"/>
	/// </param> 
	public static void DrawPathGizmos(Transform[] path,Color color) {
		if(path.Length>0){
			//create and store path points:
			Vector3[] suppliedPath = new Vector3[path.Length];
			for (int i = 0; i < path.Length; i++) {
				suppliedPath[i]=path[i].position;
			}
			
			DrawPathHelper(suppliedPath, color,"gizmos");
		}
	}	

	/// <summary>
	/// Draws a curved path through the provided array of Vector3s with Handles.DrawLine().
	/// </summary>
	/// <param name="path">
	/// A <see cref="Vector3s[]"/>
	/// </param>
	public static void DrawPathHandles(Vector3[] path) {
		if(path.Length>0){
			DrawPathHelper(path,Defaults.color,"handles");
		}
	}		
	
	/// <summary>
	/// Draws a curved path through the provided array of Vector3s with Handles.DrawLine().
	/// </summary>
	/// <param name="path">
	/// A <see cref="Vector3s[]"/>
	/// </param>
	/// <param name="color">
	/// A <see cref="Color"/>
	/// </param> 
	public static void DrawPathHandles(Vector3[] path, Color color) {
		if(path.Length>0){
			DrawPathHelper(path, color,"handles");
		}
	}
	
	/// <summary>
	/// Draws a curved path through the provided array of Transforms with Handles.DrawLine().
	/// </summary>
	/// <param name="path">
	/// A <see cref="Transform[]"/>
	/// </param>
	public static void DrawPathHandles(Transform[] path) {
		if(path.Length>0){
			//create and store path points:
			Vector3[] suppliedPath = new Vector3[path.Length];
			for (int i = 0; i < path.Length; i++) {
				suppliedPath[i]=path[i].position;
			}
			
			DrawPathHelper(suppliedPath,Defaults.color,"handles");	
		}
	}		
	
	/// <summary>
	/// Draws a curved path through the provided array of Transforms with Handles.DrawLine().
	/// </summary>
	/// <param name="path">
	/// A <see cref="Transform[]"/>
	/// </param>
	/// <param name="color">
	/// A <see cref="Color"/>
	/// </param> 
	public static void DrawPathHandles(Transform[] path,Color color) {
		if(path.Length>0){
			//create and store path points:
			Vector3[] suppliedPath = new Vector3[path.Length];
			for (int i = 0; i < path.Length; i++) {
				suppliedPath[i]=path[i].position;
			}
			
			DrawPathHelper(suppliedPath, color,"handles");
		}
	}
	
	/// <summary>
	/// Resume all EGTweens on a GameObject.
	/// </summary>
	public static void Resume(GameObject target){
		Component[] tweens = target.GetComponents<EGTween>();
		foreach (EGTween item in tweens){
			item.enabled=true;
		}
	}
	
	/// <summary>
	/// Resume all EGTweens on a GameObject including its children.
	/// </summary>
	public static void Resume(GameObject target, bool includechildren){
		Resume(target);
		if(includechildren){
			foreach(Transform child in target.transform){
				Resume(child.gameObject,true);
			}			
		}
	}	
	
	/// <summary>
	/// Resume all EGTweens on a GameObject of a particular type.
	/// </summar
	/// <param name="type">
	/// A <see cref="System.String"/> name of the type of EGTween you would like to resume.  Can be written as part of a name such as "mov" for all "MoveTo" EGTweens.
	/// </param>	
	public static void Resume(GameObject target, string type){
		Component[] tweens = target.GetComponents<EGTween>();
		foreach (EGTween item in tweens){
			string targetType = item.type+item.method;
			targetType=targetType.Substring(0,type.Length);
			if(targetType.ToLower() == type.ToLower()){
				item.enabled=true;
			}
		}
	}
	
	/// <summary>
	/// Resume all EGTweens on a GameObject of a particular type including its children.
	/// </summar
	/// <param name="type">
	/// A <see cref="System.String"/> name of the type of EGTween you would like to resume.  Can be written as part of a name such as "mov" for all "MoveTo" EGTweens.
	/// </param>	
	public static void Resume(GameObject target, string type, bool includechildren){
		Component[] tweens = target.GetComponents<EGTween>();
		foreach (EGTween item in tweens){
			string targetType = item.type+item.method;
			targetType=targetType.Substring(0,type.Length);
			if(targetType.ToLower() == type.ToLower()){
				item.enabled=true;
			}
		}
		if(includechildren){
			foreach(Transform child in target.transform){
				Resume(child.gameObject,type,true);
			}			
		}		
	}	
	
	/// <summary>
	/// Resume all EGTweens in scene.
	/// </summary>
	public static void Resume(){
		for (int i = 0; i < tweens.Count; i++) {
			Hashtable currentTween = tweens[i];
			GameObject target = (GameObject)currentTween["target"];
			Resume(target);
		}
	}	
	
	/// <summary>
	/// Resume all EGTweens in scene of a particular type.
	/// </summary>
	/// <param name="type">
	/// A <see cref="System.String"/> name of the type of EGTween you would like to resume.  Can be written as part of a name such as "mov" for all "MoveTo" EGTweens.
	/// </param> 
	public static void Resume(string type){
		ArrayList resumeArray = new ArrayList();
		
		for (int i = 0; i < tweens.Count; i++) {
			Hashtable currentTween = tweens[i];
			GameObject target = (GameObject)currentTween["target"];
			resumeArray.Insert(resumeArray.Count,target);
		}
		
		for (int i = 0; i < resumeArray.Count; i++) {
			Resume((GameObject)resumeArray[i],type);
		}
	}			
	
	//#################################
	//# PAUSE UTILITIES AND OVERLOADS # 
	//#################################

	/// <summary>
	/// Pause all EGTweens on a GameObject.
	/// </summary>
	public static void Pause(GameObject target){
		Component[] tweens = target.GetComponents<EGTween>();
		foreach (EGTween item in tweens){
			if(item.delay>0){
				item.delay-=Time.time-item.delayStarted;
				item.StopCoroutine("TweenDelay");
			}
			item.isPaused=true;
			item.enabled=false;
		}
	}
	
	/// <summary>
	/// Pause all EGTweens on a GameObject including its children.
	/// </summary>
	public static void Pause(GameObject target, bool includechildren){
		Pause(target);
		if(includechildren){
			foreach(Transform child in target.transform){
				Pause(child.gameObject,true);
			}			
		}
	}	
	
	/// <summary>
	/// Pause all EGTweens on a GameObject of a particular type.
	/// </summar
	/// <param name="type">
	/// A <see cref="System.String"/> name of the type of EGTween you would like to pause.  Can be written as part of a name such as "mov" for all "MoveTo" EGTweens.
	/// </param>	
	public static void Pause(GameObject target, string type){
		Component[] tweens = target.GetComponents<EGTween>();
		foreach (EGTween item in tweens){
			string targetType = item.type+item.method;
			targetType=targetType.Substring(0,type.Length);
			if(targetType.ToLower() == type.ToLower()){
				if(item.delay>0){
					item.delay-=Time.time-item.delayStarted;
					item.StopCoroutine("TweenDelay");
				}
				item.isPaused=true;
				item.enabled=false;
			}
		}
	}
	
	/// <summary>
	/// Pause all EGTweens on a GameObject of a particular type including its children.
	/// </summar
	/// <param name="type">
	/// A <see cref="System.String"/> name of the type of EGTween you would like to pause.  Can be written as part of a name such as "mov" for all "MoveTo" EGTweens.
	/// </param>	
	public static void Pause(GameObject target, string type, bool includechildren){
		Component[] tweens = target.GetComponents<EGTween>();
		foreach (EGTween item in tweens){
			string targetType = item.type+item.method;
			targetType=targetType.Substring(0,type.Length);
			if(targetType.ToLower() == type.ToLower()){
				if(item.delay>0){
					item.delay-=Time.time-item.delayStarted;
					item.StopCoroutine("TweenDelay");
				}
				item.isPaused=true;
				item.enabled=false;
			}
		}
		if(includechildren){
			foreach(Transform child in target.transform){
				Pause(child.gameObject,type,true);
			}			
		}		
	}	
	
	/// <summary>
	/// Pause all EGTweens in scene.
	/// </summary>
	public static void Pause(){
		for (int i = 0; i < tweens.Count; i++) {
			Hashtable currentTween = tweens[i];
			GameObject target = (GameObject)currentTween["target"];
			Pause(target);
		}
	}	
	
	/// <summary>
	/// Pause all EGTweens in scene of a particular type.
	/// </summary>
	/// <param name="type">
	/// A <see cref="System.String"/> name of the type of EGTween you would like to pause.  Can be written as part of a name such as "mov" for all "MoveTo" EGTweens.
	/// </param> 
	public static void Pause(string type){
		ArrayList pauseArray = new ArrayList();
		
		for (int i = 0; i < tweens.Count; i++) {
			Hashtable currentTween = tweens[i];
			GameObject target = (GameObject)currentTween["target"];
			pauseArray.Insert(pauseArray.Count,target);
		}
		
		for (int i = 0; i < pauseArray.Count; i++) {
			Pause((GameObject)pauseArray[i],type);
		}
	}		
	
	//#################################
	//# COUNT UTILITIES AND OVERLOADS # 
	//#################################	
	
	/// <summary>
	/// Count all EGTweens in current scene.
	/// </summary>
	public static int Count(){
		return(tweens.Count);
	}
	
	/// <summary>
	/// Count all EGTweens in current scene of a particular type.
	/// </summary>
	/// <param name="type">
	/// A <see cref="System.String"/> name of the type of EGTween you would like to stop.  Can be written as part of a name such as "mov" for all "MoveTo" EGTweens.
	/// </param> 
	public static int Count(string type){
		int tweenCount = 0;

		for (int i = 0; i < tweens.Count; i++) {
			Hashtable currentTween = tweens[i];
			string targetType = (string)currentTween["type"]+(string)currentTween["method"];
			targetType=targetType.Substring(0,type.Length);
			if(targetType.ToLower() == type.ToLower()){
				tweenCount++;
			}
		}	
		
		return(tweenCount);
	}			

	/// <summary>
	/// Count all EGTweens on a GameObject.
	/// </summary>
	public static int Count(GameObject target){
		Component[] tweens = target.GetComponents<EGTween>();
		return(tweens.Length);
	}
	
	/// <summary>
	/// Count all EGTweens on a GameObject of a particular type.
	/// </summary>
	/// <param name="type">
	/// A <see cref="System.String"/> name of the type of EGTween you would like to count.  Can be written as part of a name such as "mov" for all "MoveTo" EGTweens.
	/// </param>  
	public static int Count(GameObject target, string type){
		int tweenCount = 0;
		Component[] tweens = target.GetComponents<EGTween>();
		foreach (EGTween item in tweens){
			string targetType = item.type+item.method;
			targetType=targetType.Substring(0,type.Length);
			if(targetType.ToLower() == type.ToLower()){
				tweenCount++;
			}
		}
		return(tweenCount);
	}	
	
	//################################
	//# STOP UTILITIES AND OVERLOADS # 
	//################################	
	
	/// <summary>
	/// Stop and destroy all Tweens in current scene.
	/// </summary>
	public static void Stop(){
		for (int i = 0; i < tweens.Count; i++) {
			Hashtable currentTween = tweens[i];
			GameObject target = (GameObject)currentTween["target"];
			Stop(target);
		}
		tweens.Clear();
	}	
	
	/// <summary>
	/// Stop and destroy all EGTweens in current scene of a particular type.
	/// </summary>
	/// <param name="type">
	/// A <see cref="System.String"/> name of the type of EGTween you would like to stop.  Can be written as part of a name such as "mov" for all "MoveTo" EGTweens.
	/// </param> 
	public static void Stop(string type){
		ArrayList stopArray = new ArrayList();
		
		for (int i = 0; i < tweens.Count; i++) {
			Hashtable currentTween = tweens[i];
			GameObject target = (GameObject)currentTween["target"];
			stopArray.Insert(stopArray.Count,target);
		}
		
		for (int i = 0; i < stopArray.Count; i++) {
			Stop((GameObject)stopArray[i],type);
		}
	}		
	
	/* GFX47 MOD START */
	/// <summary>
	/// Stop and destroy all EGTweens in current scene of a particular name.
	/// </summary>
	/// <param name="name">
	/// The <see cref="System.String"/> name of EGTween you would like to stop.
	/// </param> 
	public static void StopByName(string name){
		ArrayList stopArray = new ArrayList();
		
		for (int i = 0; i < tweens.Count; i++) {
			Hashtable currentTween = tweens[i];
			GameObject target = (GameObject)currentTween["target"];
			stopArray.Insert(stopArray.Count,target);
		}
		
		for (int i = 0; i < stopArray.Count; i++) {
			StopByName((GameObject)stopArray[i],name);
		}
	}
	/* GFX47 MOD END */
	
	/// <summary>
	/// Stop and destroy all EGTweens on a GameObject.
	/// </summary>
	public static void Stop(GameObject target){
		Component[] tweens = target.GetComponents<EGTween>();
		foreach (EGTween item in tweens){
			item.Dispose();
		}
	}
	
	/// <summary>
	/// Stop and destroy all EGTweens on a GameObject including its children.
	/// </summary>
	public static void Stop(GameObject target, bool includechildren){
		Stop(target);
		if(includechildren){
			foreach(Transform child in target.transform){
				Stop(child.gameObject,true);
			}			
		}
	}	
	
	/// <summary>
	/// Stop and destroy all EGTweens on a GameObject of a particular type.
	/// </summar
	/// <param name="type">
	/// A <see cref="System.String"/> name of the type of EGTween you would like to stop.  Can be written as part of a name such as "mov" for all "MoveTo" EGTweens.
	/// </param>	
	public static void Stop(GameObject target, string type){
		Component[] tweens = target.GetComponents<EGTween>();
		foreach (EGTween item in tweens){
			string targetType = item.type+item.method;
			targetType=targetType.Substring(0,type.Length);
			if(targetType.ToLower() == type.ToLower()){
				item.Dispose();
			}
		}
	}
	
	/* GFX47 MOD START */
	/// <summary>
	/// Stop and destroy all EGTweens on a GameObject of a particular name.
	/// </summar
	/// <param name="name">
	/// The <see cref="System.String"/> name of EGTween you would like to stop.
	/// </param>	
	public static void StopByName(GameObject target, string name){
		Component[] tweens = target.GetComponents<EGTween>();
		foreach (EGTween item in tweens){
			/*string targetType = item.type+item.method;
			targetType=targetType.Substring(0,type.Length);
			if(targetType.ToLower() == type.ToLower()){
				item.Dispose();
			}*/
			if(item._name == name){
				item.Dispose();
			}
		}
	}
	/* GFX47 MOD END */
	
	/// <summary>
	/// Stop and destroy all EGTweens on a GameObject of a particular type including its children.
	/// </summar
	/// <param name="type">
	/// A <see cref="System.String"/> name of the type of EGTween you would like to stop.  Can be written as part of a name such as "mov" for all "MoveTo" EGTweens.
	/// </param>	
	public static void Stop(GameObject target, string type, bool includechildren){
		Component[] tweens = target.GetComponents<EGTween>();
		foreach (EGTween item in tweens){
			string targetType = item.type+item.method;
			targetType=targetType.Substring(0,type.Length);
			if(targetType.ToLower() == type.ToLower()){
				item.Dispose();
			}
		}
		if(includechildren){
			foreach(Transform child in target.transform){
				Stop(child.gameObject,type,true);
			}			
		}		
	}
	
	/* GFX47 MOD START */
	/// <summary>
	/// Stop and destroy all EGTweens on a GameObject of a particular name including its children.
	/// </summar
	/// <param name="name">
	/// The <see cref="System.String"/> name of EGTween you would like to stop.
	/// </param>	
	public static void StopByName(GameObject target, string name, bool includechildren){
		Component[] tweens = target.GetComponents<EGTween>();
		foreach (EGTween item in tweens){
			/*string targetType = item.type+item.method;
			targetType=targetType.Substring(0,type.Length);
			if(targetType.ToLower() == type.ToLower()){
				item.Dispose();
			}*/
			if(item._name == name){
				item.Dispose();
			}
		}
		if(includechildren){
			foreach(Transform child in target.transform){
				//Stop(child.gameObject,type,true);
				StopByName(child.gameObject,name,true);
			}			
		}		
	}
	/* GFX47 MOD END */

	/// <summary>
	/// Universal interface to help in the creation of Hashtables.  Especially useful for C# users.
	/// </summary>
	/// <param name="args">
	/// A <see cref="System.Object[]"/> of alternating name value pairs.  For example "time",1,"delay",2...
	/// </param>
	/// <returns>
	/// A <see cref="Hashtable"/>
	/// </returns>
	public static Hashtable Hash(params object[] args){
		Hashtable hashTable = new Hashtable(args.Length/2);
		if (args.Length %2 != 0) {
			Debug.LogError("Tween Error: Hash requires an even number of arguments!"); 
			return null;
		}else{
			int i = 0;
			while(i < args.Length - 1) {
				hashTable.Add(args[i], args[i+1]);
				i += 2;
			}
			return hashTable;
		}
	}	
	
	#endregion		

	#region Component Segments
	
	private EGTween(Hashtable h) {
		tweenArguments = h;	
	}
	
	void Awake(){
		thisTransform = transform;
			
		RetrieveArgs();
        lastRealTime = Time.realtimeSinceStartup; // Added by PressPlay
	}
	
	IEnumerator Start(){
		if(delay > 0){
			yield return StartCoroutine("TweenDelay");
		}
		TweenStart();
	}	
	
	//non-physics
	void Update(){
		if(isRunning && !physics){
			if(!reverse){
				if(percentage<1f){
					TweenUpdate();
				}else{
					TweenComplete();	
				}
			}else{
				if(percentage>0){
					TweenUpdate();
				}else{
					TweenComplete();	
				}
			}
		}
	}
	
	//physics
	void FixedUpdate(){
		if(isRunning && physics){
			if(!reverse){
				if(percentage<1f){
					TweenUpdate();
				}else{
					TweenComplete();	
				}
			}else{
				if(percentage>0){
					TweenUpdate();
				}else{
					TweenComplete();	
				}
			}
		}	
	}

	void LateUpdate(){
		//look applications:
		if(tweenArguments.Contains("looktarget") && isRunning){
			if(type =="move" || type =="shake" || type=="punch"){
				LookUpdate(gameObject,tweenArguments);
			}			
		}
	}
	
	void OnEnable(){
		//resume delay:
		if(isPaused){
			isPaused=false;
			if(delay > 0){
				wasPaused=true;
				ResumeDelay();
			}
		}
	}

	void OnDisable(){
	}
	
	#endregion
	
	#region Internal Helpers
	private static void DrawLineHelper(Vector3[] line, Color color, string method){
		Gizmos.color=color;
		for (int i = 0; i < line.Length-1; i++) {
			if(method == "gizmos"){
				Gizmos.DrawLine(line[i], line[i+1]);;
			}else if(method == "handles"){
				Debug.LogError("EGTween Error: Drawing a line with Handles is temporarily disabled because of compatability issues with Unity 2.6!");
				//UnityEditor.Handles.DrawLine(line[i], line[i+1]);
			}
		}
	}		
	
	private static void DrawPathHelper(Vector3[] path, Color color, string method){
		Vector3[] vector3s = PathControlPointGenerator(path);
		
		//Line Draw:
		Vector3 prevPt = Interp(vector3s,0);
		Gizmos.color=color;
		int SmoothAmount = path.Length*20;
		for (int i = 1; i <= SmoothAmount; i++) {
			float pm = (float) i / SmoothAmount;
			Vector3 currPt = Interp(vector3s,pm);
			if(method == "gizmos"){
				Gizmos.DrawLine(currPt, prevPt);
			}else if(method == "handles"){
				Debug.LogError("EGTween Error: Drawing a path with Handles is temporarily disabled because of compatability issues with Unity 2.6!");
				//UnityEditor.Handles.DrawLine(currPt, prevPt);
			}
			prevPt = currPt;
		}
	}	
	
	private static Vector3[] PathControlPointGenerator(Vector3[] path){
		Vector3[] suppliedPath;
		Vector3[] vector3s;
		
		//create and store path points:
		suppliedPath = path;

		//populate calculate path;
		int offset = 2;
		vector3s = new Vector3[suppliedPath.Length+offset];
		Array.Copy(suppliedPath,0,vector3s,1,suppliedPath.Length);
		
		//populate start and end control points:
		//vector3s[0] = vector3s[1] - vector3s[2];
		vector3s[0] = vector3s[1] + (vector3s[1] - vector3s[2]);
		vector3s[vector3s.Length-1] = vector3s[vector3s.Length-2] + (vector3s[vector3s.Length-2] - vector3s[vector3s.Length-3]);
		
		//is this a closed, continuous loop? yes? well then so let's make a continuous Catmull-Rom spline!
		if(vector3s[1] == vector3s[vector3s.Length-2]){
			Vector3[] tmpLoopSpline = new Vector3[vector3s.Length];
			Array.Copy(vector3s,tmpLoopSpline,vector3s.Length);
			tmpLoopSpline[0]=tmpLoopSpline[tmpLoopSpline.Length-3];
			tmpLoopSpline[tmpLoopSpline.Length-1]=tmpLoopSpline[2];
			vector3s=new Vector3[tmpLoopSpline.Length];
			Array.Copy(tmpLoopSpline,vector3s,tmpLoopSpline.Length);
		}	
		
		return(vector3s);
	}
	
	//andeeee from the Unity forum's steller Catmull-Rom class ( http://forum.unity3d.com/viewtopic.php?p=218400#218400 ):
	private static Vector3 Interp(Vector3[] pts, float t){
		int numSections = pts.Length - 3;
		int currPt = Mathf.Min(Mathf.FloorToInt(t * (float) numSections), numSections - 1);
		float u = t * (float) numSections - (float) currPt;
				
		Vector3 a = pts[currPt];
		Vector3 b = pts[currPt + 1];
		Vector3 c = pts[currPt + 2];
		Vector3 d = pts[currPt + 3];
		
		return .5f * (
			(-a + 3f * b - 3f * c + d) * (u * u * u)
			+ (2f * a - 5f * b + 4f * c - d) * (u * u)
			+ (-a + c) * u
			+ 2f * b
		);
	}	
		
	//catalog new tween and add component phase of EGTween:
	static void Launch(GameObject target, Hashtable args){
		if(!args.Contains("id")){
			args["id"] = GenerateID();
		}
		if(!args.Contains("target")){
			args["target"] = target;
		
		}		

		tweens.Insert (0, args);
		target.AddComponent<EGTween>();
	}		
	
	//cast any accidentally supplied doubles and ints as floats as EGTween only uses floats internally and unify parameter case:
	static Hashtable CleanArgs(Hashtable args){
		Hashtable argsCopy = new Hashtable(args.Count);
		Hashtable argsCaseUnified = new Hashtable(args.Count);
		
		foreach (DictionaryEntry item in args) {
			argsCopy.Add(item.Key, item.Value);
		}
		
		foreach (DictionaryEntry item in argsCopy) {
			if(item.Value.GetType() == typeof(System.Int32)){
				int original = (int)item.Value;
				float casted = (float)original;
				args[item.Key] = casted;
			}
			if(item.Value.GetType() == typeof(System.Double)){
				double original = (double)item.Value;
				float casted = (float)original;
				args[item.Key] = casted;
			}
		}	
		
		//unify parameter case:
		foreach (DictionaryEntry item in args) {
			argsCaseUnified.Add(item.Key.ToString().ToLower(), item.Value);
		}	
		
		//swap back case unification:
		args = argsCaseUnified;
				
		return args;
	}	
	
	//random ID generator:
	static string GenerateID(){
//		int strlen = 15;
//		char[] chars = {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z','A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z','0','1','2','3','4','5','6','7','8'};
//		int num_chars = chars.Length - 1;
//		string randomChar = "";
//		for (int i = 0; i < strlen; i++) {
//			randomChar += chars[(int)Mathf.Floor(UnityEngine.Random.Range(0,num_chars))];
//		}
		return System.Guid.NewGuid().ToString();
	}	
	
	//grab and set generic, neccesary EGTween arguments:
	void RetrieveArgs(){
		foreach (Hashtable item in tweens) {
			if((GameObject)item["target"] == gameObject){
				tweenArguments=item;
				break;
			}
		}
		
		id=(string)tweenArguments["id"];
		type=(string)tweenArguments["type"];
		/* GFX47 MOD START */
		_name=(string)tweenArguments["name"];
		/* GFX47 MOD END */
		method=(string)tweenArguments["method"];
               
		if(tweenArguments.Contains("time")){
			time=(float)tweenArguments["time"];
		}else{
			time=Defaults.time;
		}
			
		//do we need to use physics, is there a rigidbody?
		if(GetComponent<Rigidbody>() != null){
			physics=true;
		}
               
		if(tweenArguments.Contains("delay")){
			delay=(float)tweenArguments["delay"];
		}else{
			delay=Defaults.delay;
		}
				
		if(tweenArguments.Contains("looptype")){
			//allows loopType to be set as either an enum(C# friendly) or a string(JS friendly), string case usage doesn't matter to further increase usability:
			if(tweenArguments["looptype"].GetType() == typeof(LoopType)){
				loopType=(LoopType)tweenArguments["looptype"];
			}else{
				try {
					loopType=(LoopType)Enum.Parse(typeof(LoopType),(string)tweenArguments["looptype"],true); 
				} catch {
					Debug.LogWarning("EGTween: Unsupported loopType supplied! Default will be used.");
					loopType = EGTween.LoopType.none;	
				}
			}			
		}else{
			loopType = EGTween.LoopType.none;	
		}			
         
		if(tweenArguments.Contains("easetype")){
			//allows easeType to be set as either an enum(C# friendly) or a string(JS friendly), string case usage doesn't matter to further increase usability:
			if(tweenArguments["easetype"].GetType() == typeof(EaseType)){
				easeType=(EaseType)tweenArguments["easetype"];
			}else{
				try {
					easeType=(EaseType)Enum.Parse(typeof(EaseType),(string)tweenArguments["easetype"],true); 
				} catch {
					Debug.LogWarning("EGTween: Unsupported easeType supplied! Default will be used.");
					easeType=Defaults.easeType;
				}
			}
		}else{
			easeType=Defaults.easeType;
		}
				
		if(tweenArguments.Contains("space")){
			//allows space to be set as either an enum(C# friendly) or a string(JS friendly), string case usage doesn't matter to further increase usability:
			if(tweenArguments["space"].GetType() == typeof(Space)){
				space=(Space)tweenArguments["space"];
			}else{
				try {
					space=(Space)Enum.Parse(typeof(Space),(string)tweenArguments["space"],true); 	
				} catch {
					Debug.LogWarning("EGTween: Unsupported space supplied! Default will be used.");
					space = Defaults.space;
				}
			}			
		}else{
			space = Defaults.space;
		}
		
		if(tweenArguments.Contains("islocal")){
			isLocal = (bool)tweenArguments["islocal"];
		}else{
			isLocal = Defaults.isLocal;
		}

        // Added by PressPlay
        if (tweenArguments.Contains("ignoretimescale"))
        {
            useRealTime = (bool)tweenArguments["ignoretimescale"];
        }
        else
        {
            useRealTime = Defaults.useRealTime;
        }

		//instantiates a cached ease equation reference:
		GetEasingFunction();
	}	
	
	//instantiates a cached ease equation refrence:
	void GetEasingFunction(){
		switch (easeType){
		case EaseType.easeInExpo:
			ease = new EasingFunction(easeInExpo);
			break;
		case EaseType.easeOutExpo:
			ease = new EasingFunction(easeOutExpo);
			break;
		case EaseType.easeInOutExpo:
			ease = new EasingFunction(easeInOutExpo);
			break;
		case EaseType.linear:
			ease = new EasingFunction(linear);
			break;
		case EaseType.easeInBack:
			ease = new EasingFunction(easeInBack);
			break;
		case EaseType.easeOutBack:
			ease = new EasingFunction(easeOutBack);
			break;
		case EaseType.easeInOutBack:
			ease = new EasingFunction(easeInOutBack);
			break;
		}
	}
	
	//calculate percentage of tween based on time:
	void UpdatePercentage(){

	        // Added by PressPlay   
	        if (useRealTime)
	        {
	            runningTime += (Time.realtimeSinceStartup - lastRealTime);      
	        }
	        else
	        {
	            runningTime += Time.deltaTime;
	        }
	
			if(reverse){
				percentage = 1 - runningTime/time;	
			}else{
				percentage = runningTime/time;	
			}
	
	        lastRealTime = Time.realtimeSinceStartup; // Added by PressPlay
	}
	
	void CallBack(string callbackType){
		if (tweenArguments.Contains(callbackType) && !tweenArguments.Contains("ischild")) {
			//establish target:
			GameObject target;
			if (tweenArguments.Contains(callbackType+"target")) {
				target=(GameObject)tweenArguments[callbackType+"target"];
			}else{
				target=gameObject;	
			}
			
			//throw an error if a string wasn't passed for callback:
			if (tweenArguments[callbackType].GetType() == typeof(System.String)) {
				target.SendMessage((string)tweenArguments[callbackType],(object)tweenArguments[callbackType+"params"],SendMessageOptions.DontRequireReceiver);
			}else{
				Debug.LogError("EGTween Error: Callback method references must be passed as a String!");
				Destroy (this);
			}
		}
	}
	
	void Dispose(){
		for (int i = 0; i < tweens.Count; i++) {
			Hashtable tweenEntry = tweens[i];
			if ((string)tweenEntry["id"] == id){
				tweens.RemoveAt(i);
				break;
			}
		}
		Destroy(this);
	}	
	
	void ConflictCheck(){//if a new EGTween is about to run and is of the same type as an in progress EGTween this will destroy the previous if the new one is NOT identical in every way or it will destroy the new EGTween if they are:	
		Component[] tweens = GetComponents<EGTween>();
		foreach (EGTween item in tweens) {
			if(item.type == "value"){
				return;
			}else if(item.isRunning && item.type==type){
				//cancel out if this is a shake or punch variant:
				if (item.method != method) {
					return;
				}				
				
				//step 1: check for length first since it's the fastest:
				if(item.tweenArguments.Count != tweenArguments.Count){
					item.Dispose();
					return;
				}
				
				//step 2: side-by-side check to figure out if this is an identical tween scenario to handle Update usages of EGTween:
				foreach (DictionaryEntry currentProp in tweenArguments) {
					if(!item.tweenArguments.Contains(currentProp.Key)){
						item.Dispose();
						return;
					}else{
						if(!item.tweenArguments[currentProp.Key].Equals(tweenArguments[currentProp.Key]) && (string)currentProp.Key != "id"){//if we aren't comparing ids and something isn't exactly the same replace the running EGTween: 
							item.Dispose();
							return;
						}
					}
				}
				
				//step 3: prevent a new EGTween addition if it is identical to the currently running EGTween
				Dispose();
				//Destroy(this);	
			}
		}
	}
	
	void ResumeDelay(){
		StartCoroutine("TweenDelay");
	}	
	
	#endregion	
	
	#region Easing Curves
	
	private float linear(float start, float end, float value){
		return Mathf.Lerp(start, end, value);
	}
	
	private float clerp(float start, float end, float value){
		float min = 0.0f;
		float max = 360.0f;
		float half = Mathf.Abs((max - min) * 0.5f);
		float retval = 0.0f;
		float diff = 0.0f;
		if ((end - start) < -half){
			diff = ((max - start) + end) * value;
			retval = start + diff;
		}else if ((end - start) > half){
			diff = -((max - end) + start) * value;
			retval = start + diff;
		}else retval = start + (end - start) * value;
		return retval;
    }

	private float easeInBack(float start, float end, float value){
		end -= start;
		value /= 1;
		float s = 1.70158f;
		return end * (value) * value * ((s + 1) * value - s) + start;
	}

	private float easeOutBack(float start, float end, float value){
		float s = 1.70158f;
		end -= start;
		value = (value) - 1;
		return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
	}

	private float easeInOutBack(float start, float end, float value){
		float s = 1.70158f;
		end -= start;
		value /= .5f;
		if ((value) < 1){
			s *= (1.525f);
			return end * 0.5f * (value * value * (((s) + 1) * value - s)) + start;
		}
		value -= 2;
		s *= (1.525f);
		return end * 0.5f * ((value) * value * (((s) + 1) * value + s) + 2) + start;
	}

	private float easeInExpo(float start, float end, float value){
		end -= start;
		return end * Mathf.Pow(2, 10 * (value - 1)) + start;
	}
	
	private float easeOutExpo(float start, float end, float value){
		end -= start;
		return end * (-Mathf.Pow(2, -10 * value ) + 1) + start;
	}
	
	private float easeInOutExpo(float start, float end, float value){
		value /= .5f;
		end -= start;
		if (value < 1) return end * 0.5f * Mathf.Pow(2, 10 * (value - 1)) + start;
		value--;
		return end * 0.5f * (-Mathf.Pow(2, -10 * value) + 2) + start;
	}
	#endregion	
} 
