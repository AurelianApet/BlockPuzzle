using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToggleButton : MonoBehaviour, IBeginDragHandler,IDragHandler,IEndDragHandler,IPointerClickHandler
{
	public Image btnToggleGraphics;
	[HideInInspector] public Vector2 btnToggleGraphicsPosition = Vector2.zero;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
		btnToggleGraphicsPosition = btnToggleGraphics.rectTransform.anchoredPosition;
	}


	#region IPointerClickHandler implementation

	/// <summary>
	/// Raises the pointer click event.
	/// </summary>
	/// <param name="eventData">Event data.</param>
	public virtual void OnPointerClick (PointerEventData eventData)
	{
		Vector2 points = btnToggleGraphics.rectTransform.anchoredPosition;
		points [0] = Mathf.Clamp (points.x, -(Mathf.Abs (btnToggleGraphicsPosition.x)), (Mathf.Abs (btnToggleGraphicsPosition.x)));
		btnToggleGraphics.rectTransform.anchoredPosition = points;
		bool toggleStatus = (points.x < 0F) ? true : false;
		EGTween.MoveTo (btnToggleGraphics.gameObject, EGTween.Hash ("x", (toggleStatus ? (Mathf.Abs (btnToggleGraphicsPosition.x)) : -(Mathf.Abs (btnToggleGraphicsPosition.x))), "isLocal", true, "time", 0.5F, "easeType", EGTween.EaseType.easeOutExpo));		
		OnToggleStatusChanged (toggleStatus);
	}

	#endregion

	#region IBeginDragHandler implementation

	/// <summary>
	/// Raises the begin drag event.
	/// </summary>
	/// <param name="eventData">Event data.</param>
	public void OnBeginDrag (PointerEventData eventData)
	{
	}

	#endregion

	#region IDragHandler implementation

	/// <summary>
	/// Raises the drag event.
	/// </summary>
	/// <param name="eventData">Event data.</param>
	public virtual void OnDrag (PointerEventData eventData)
	{
		Vector2 points = Vector2.zero;
		RectTransformUtility.ScreenPointToLocalPointInRectangle (transform.GetComponent<RectTransform> (), eventData.position, Camera.main, out points);
		points [0] = Mathf.Clamp (points.x, -(Mathf.Abs (btnToggleGraphicsPosition.x)), (Mathf.Abs (btnToggleGraphicsPosition.x)));
		points [1] = btnToggleGraphics.rectTransform.anchoredPosition.y;
		btnToggleGraphics.rectTransform.anchoredPosition = points;
	}

	#endregion

	#region IDropHandler implementation

	/// <summary>
	/// Raises the end drag event.
	/// </summary>
	/// <param name="eventData">Event data.</param>
	public virtual void OnEndDrag (PointerEventData eventData)
	{
		Vector2 points = btnToggleGraphics.rectTransform.anchoredPosition;
		points [0] = Mathf.Clamp (points.x, -(Mathf.Abs (btnToggleGraphicsPosition.x)), (Mathf.Abs (btnToggleGraphicsPosition.x)));
		btnToggleGraphics.rectTransform.anchoredPosition = points;
		bool toggleStatus = (points.x < 0F) ? false : true;
		EGTween.MoveTo (btnToggleGraphics.gameObject, EGTween.Hash ("x", (toggleStatus ? (Mathf.Abs (btnToggleGraphicsPosition.x)) : -(Mathf.Abs (btnToggleGraphicsPosition.x))), "isLocal", true, "time", 0.5F, "easeType", EGTween.EaseType.easeOutExpo));
		OnToggleStatusChanged (toggleStatus);
	}

	#endregion

	/// <summary>
	/// Raises the toggle status changed event.
	/// </summary>
	/// <param name="status">If set to <c>true</c> status.</param>
	public virtual void OnToggleStatusChanged (bool status)
	{
		//Do Nothing here
	}
}
