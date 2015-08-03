using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraModule : MonoBehaviour {

	public enum OPTICAL_ZOOM {

		NONE = -1,

		OFF = 0,			// 명시적으로 리셋.

		_2BY2_0_0,			// ４배 좌상.
		_2BY2_1_0,			// ４배 우상.
		_2BY2_0_1,			// ４배 좌하.
		_2BY2_1_1,			// ４배 우하.
		AS_IS,				// 동일배율(테스트용).

		NUM,
	};
	public    OPTICAL_ZOOM		optical_zoom         = OPTICAL_ZOOM.OFF;
	protected OPTICAL_ZOOM		optical_zoom_current = OPTICAL_ZOOM.OFF;

	public struct Posture {

		public Vector3		position;
		public Vector3		intererst;
		public Vector3		up;
	};

	public float		focal_length = 30.0f;
	protected Posture	current;

	protected List<Posture>	stack = new List<Posture>();

	protected GameObject	locator_intererst = null;

	// ================================================================ //
	// MonoBehaviour로부터 상속.

	void	Awake()
	{
		this.attach();
	}

	void	Start()
	{
	}

	void	Update()
	{
	}

	// UnityEditor 상에서도(일시정지 중도!)실행됩니다.
	void	OnValidate()
	{
		this.update_optical_zoom();
	#if false
		do {

			if(this.locator_intererst != null) {

				break;
			}

			if(this.transform.FindChild("intererst") != null) {

				this.locator_intererst = this.transform.FindChild("intererst").gameObject;
			}

			if(this.locator_intererst != null) {

				break;
			}

			this.locator_intererst = new GameObject("intererst");

			this.locator_intererst.transform.parent = this.gameObject.transform;
			this.locator_intererst.transform.localPosition = Vector3.forward*this.focal_length;
			this.locator_intererst.transform.localRotation = Quaternion.identity;

		} while(false);

		this.locator_intererst.transform.localPosition = Vector3.forward*this.focal_length;
		this.locator_intererst.transform.localRotation = Quaternion.identity;
	#endif
	}

	// ================================================================ //
	
	protected void	update_optical_zoom()
	{
		if(this.optical_zoom != this.optical_zoom_current) {

			float	near  = this.GetComponent<Camera>().nearClipPlane;
			float	far   = this.GetComponent<Camera>().farClipPlane;
			float	top   = Mathf.Tan(this.GetComponent<Camera>().fieldOfView/2.0f*Mathf.Deg2Rad)*near;
			float	right = this.GetComponent<Camera>().aspect*top;

			switch(this.optical_zoom) {

				case OPTICAL_ZOOM._2BY2_0_0:
				{
					Matrix4x4	m = CameraModule.createFrustumProjectionMatrix(0.0f, -right, top, 0.0f, near, far);
			
					this.GetComponent<Camera>().projectionMatrix = m;
				}
				break;

				case OPTICAL_ZOOM._2BY2_1_0:
				{
					Matrix4x4	m = CameraModule.createFrustumProjectionMatrix(right, 0.0f, top, 0.0f, near, far);
			
					this.GetComponent<Camera>().projectionMatrix = m;
				}
				break;

				case OPTICAL_ZOOM._2BY2_0_1:
				{
					Matrix4x4	m = CameraModule.createFrustumProjectionMatrix(0.0f, -right, 0.0f, -top, near, far);
			
					this.GetComponent<Camera>().projectionMatrix = m;
				}
				break;

				case OPTICAL_ZOOM._2BY2_1_1:
				{
					Matrix4x4	m = CameraModule.createFrustumProjectionMatrix(right, 0.0f, 0.0f, -top, near, far);
			
					this.GetComponent<Camera>().projectionMatrix = m;
				}
				break;

				case OPTICAL_ZOOM.AS_IS:
				{
					Matrix4x4	m = CameraModule.createFrustumProjectionMatrix(right, -right, top, -top, near, far);
			
					this.GetComponent<Camera>().projectionMatrix = m;
				}
				break;

				default:
				case OPTICAL_ZOOM.OFF:
				{
					this.GetComponent<Camera>().ResetProjectionMatrix();
				}
				break;
			}

			this.optical_zoom_current = this.optical_zoom;
		}
	}

	// ================================================================ //

	public void		attach()
	{
		this.current.position  = this.transform.position;
		this.current.intererst = this.transform.TransformPoint(Vector3.forward*this.focal_length);
		this.current.up        = this.transform.TransformDirection(Vector3.up);
	}

	public void		dolly(float focal_length)
	{
		this.focal_length = focal_length;

		Vector3		eye_vector = this.current.intererst - this.current.position;

		eye_vector.Normalize();
		eye_vector *= this.focal_length;

		this.current.position = this.current.intererst - eye_vector;

		this.update();
	}

	public void		parallelMoveTo(Vector3 position)
	{
		Vector3		eye_vector = this.current.intererst - this.current.position;

		this.current.position  = position;
		this.current.intererst = this.current.position + eye_vector;

		this.update();
	}

	public void		setInterest(Vector3 interest)
	{
		this.current.intererst = interest;

		this.update();
	}

	public void		update()
	{
		this.transform.position = this.current.position;
		this.transform.rotation = Quaternion.LookRotation(this.current.intererst - this.current.position, this.current.up);

		this.focal_length = Vector3.Distance(this.current.intererst, this.current.position);
	}

	// 자세를 스택에 푸시합니다.
	public void		pushPosture()
	{
		this.stack.Add(this.current);
	}

	// 자세를 스택에서 팝합니다.
	public void		popPosture()
	{
		if(this.stack.Count > 0) {

			this.current = this.stack[this.stack.Count - 1];

			this.stack.RemoveRange(this.stack.Count - 1, 1);

			this.update();
		}
	}

	// ================================================================ //

	static Matrix4x4	createFrustumProjectionMatrix(float right, float left, float top, float bottom, float znear, float zfar)
	{
		Matrix4x4 m = new Matrix4x4();
	
		m[0, 0] = (2.0f*znear)/(right - left);
		m[1, 0] = 0.0f;
		m[2, 0] = 0.0f;
		m[3, 0] = 0.0f;
	
		m[0, 1] = 0.0f;
		m[1, 1] = (2.0f*znear)/(top - bottom);
		m[2, 1] = 0.0f;
		m[3, 1] = 0.0f;
	
		m[0, 2] = (right + left)/(right - left);
		m[1, 2] = (top + bottom)/(top - bottom);
		m[2, 2] = -zfar/(zfar - znear);
		m[3, 2] = -1.0f;
	
		m[0, 3] = 0.0f;
		m[1, 3] = 0.0f;
		m[2, 3] = -zfar*znear/(zfar - znear);
		m[3, 3] = 0.0f;

		return(m);
	}
}
