using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	public CameraModule	module = null;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Awake()
	{
		this.module = this.gameObject.GetComponent<CameraModule>();

		if(this.module == null) {

			this.module = this.gameObject.AddComponent<CameraModule>();
		}
	}

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	private	static CameraControl	instance = null;

	public static CameraControl	get()
	{
		if(CameraControl.instance == null) {

			CameraControl.instance = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraControl>();
		}

		return(CameraControl.instance);
	}
}
