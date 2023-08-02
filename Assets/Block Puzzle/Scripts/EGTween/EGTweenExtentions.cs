using UnityEngine;
using System.Collections;

public static class EGTweenExtentions 
{
	public static void Init(this GameObject target)
	{
		EGTween.Init(target);
	}

	public static void ValueTo(this GameObject target, Hashtable args)
	{
		EGTween.ValueTo (target, args);
	}

	public static void MoveTo(this GameObject target, Vector3 position, float time){
		EGTween.MoveTo(target,EGTween.Hash("position",position,"time",time));
	}

	public static void MoveTo(this GameObject target, Hashtable args){
		EGTween.MoveTo (target, args);
	}

	public static void MoveFrom(this GameObject target, Vector3 position, float time){
		EGTween.MoveFrom(target,EGTween.Hash("position",position,"time",time));
	}

	public static void MoveFrom(this GameObject target, Hashtable args){
		EGTween.MoveFrom (target, args);
	}

	public static void MoveBy(this GameObject target, Vector3 amount, float time){
		EGTween.MoveBy(target,EGTween.Hash("amount",amount,"time",time));
	}

	public static void MoveBy(this GameObject target, Hashtable args){
		EGTween.MoveBy (target, args);
	}

	public static void ScaleTo(this GameObject target, Vector3 scale, float time){
		EGTween.ScaleTo(target,EGTween.Hash("scale",scale,"time",time));
	}

	public static void ScaleTo(this GameObject target, Hashtable args){
		EGTween.ScaleTo (target, args);
	}

	public static void ScaleFrom(this GameObject target, Vector3 scale, float time){
		EGTween.ScaleFrom(target,EGTween.Hash("scale",scale,"time",time));
	}

	public static void ScaleFrom(this GameObject target, Hashtable args){
		EGTween.ScaleFrom (target, args);
	}

	public static void ScaleBy(this GameObject target, Vector3 amount, float time){
		EGTween.ScaleBy(target,EGTween.Hash("amount",amount,"time",time));
	}

	public static void ScaleBy(this GameObject target, Hashtable args){
		EGTween.ScaleBy (target, args);
	}

	public static void RotateTo(this GameObject target, Vector3 rotation, float time){
		EGTween.RotateTo(target,EGTween.Hash("rotation",rotation,"time",time));
	}

	public static void RotateTo(this GameObject target, Hashtable args){
		EGTween.RotateTo (target, args);
	}

	public static void RotateFrom(this GameObject target, Vector3 rotation, float time){
		EGTween.RotateFrom(target,EGTween.Hash("rotation",rotation,"time",time));
	}

	public static void RotateFrom(this GameObject target, Hashtable args){
		EGTween.RotateFrom (target, args);
	}

	public static void RotateBy(this GameObject target, Vector3 amount, float time){
		EGTween.RotateBy(target,EGTween.Hash("amount",amount,"time",time));
	}

	public static void RotateBy(this GameObject target, Hashtable args){
		EGTween.RotateBy (target, args);
	}
}
