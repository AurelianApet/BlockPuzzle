using UnityEngine;
using System.Collections;

public class UM_Result : MonoBehaviour {

	protected UM_Error _Error = null;
	protected bool _IsSucceeded = true;
	
	
	//--------------------------------------
	// Initialize
	//--------------------------------------

	public UM_Result(ISN_Result result) {
		_IsSucceeded = result.IsSucceeded;

		if(!_IsSucceeded) {
			_Error =  new UM_Error(result.Error.Code, result.Error.Description);
		}
	}

	public UM_Result(GooglePlayResult result) {
		_IsSucceeded = result.isSuccess;
		
		if(!_IsSucceeded) {
			_Error =  new UM_Error( (int) result.response, result.message);
		}
	}



	//--------------------------------------
	// Public Methods (internal use only)
	//--------------------------------------
	

	
	//--------------------------------------
	// Get / Set
	//--------------------------------------
	
	
	public bool IsSucceeded {
		get {
			return _IsSucceeded;
		}
	}
	
	public bool IsFailed {
		get {
			return !_IsSucceeded;
		}
	}
	
	public UM_Error Error {
		get {
			return _Error;
		}
	}
	
	
	//--------------------------------------
	// Private Methods
	//--------------------------------------
	
	public void SetError(UM_Error e) {
		_Error = e;
		_IsSucceeded = false;
	}
}
