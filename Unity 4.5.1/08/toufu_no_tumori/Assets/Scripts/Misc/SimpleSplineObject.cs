using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class SimpleSplineObject : MonoBehaviour {

	public SimpleSpline.Curve	curve  = new SimpleSpline.Curve();
	public SimpleSpline.Tracer	tracer = new SimpleSpline.Tracer();

	protected LineRenderer			line_render = null;

	public bool	is_update_edit_mode = false;

	// ================================================================ //
	// MonoBehaviour로부터 상속.

	void	Start()
	{
		if(Application.isPlaying) {

			this.createControlVertices();

		} else {

		}
	}

	void	Update()
	{
		if(!Application.isPlaying) {

			if(this.is_update_edit_mode) {
	
				this.createControlVertices();
		
				//
		
				if(this.line_render == null) {
		
					this.line_render = this.gameObject.GetComponent<LineRenderer>();

					if(this.line_render == null) {

						this.line_render = this.gameObject.AddComponent<LineRenderer>();
					}
				}
		
				if(this.line_render != null) {
		
					this.updateLineRender();
				}
	
			} else {
	
				if(this.line_render != null) {
	
					this.line_render.SetVertexCount(0);
				}
			}
		}
	}

	// ================================================================ //

	// 제어점을 갱신합니다(자신의 자식에서부터).
	public void		createControlVertices()
	{
		this.curve.cvs.Clear();

		foreach(var i in System.Linq.Enumerable.Range(0, this.transform.childCount)) {

			GameObject		child = this.transform.GetChild(i).gameObject;

			var	cv = new SimpleSpline.ControlVertex();

			cv.index    = i;
			cv.position = child.transform.position;
			cv.tangent  = child.transform.TransformDirection(Vector3.forward)*child.transform.localScale.z;
			cv.tension  = 10.0f;

			this.curve.cvs.Add(cv);
		}
	}

	// ================================================================ //

	// LineRender를 갱신합니다.
	public void		updateLineRender()
	{
		this.tracer.attach(this.curve);
		this.tracer.restart();

		float	dt = 0.1f;
		int		vertex_num = Mathf.CeilToInt(this.curve.getEnd()/dt) + 1;

		this.line_render.SetVertexCount(vertex_num);
		this.line_render.SetColors(Color.red, Color.blue);

		for(int i = 0;i < vertex_num;i++) {

			this.tracer.proceed(dt);

			this.line_render.SetPosition(i, this.tracer.cv.position);
			if(this.tracer.isEnded()) {

				break;
			}
		}
	}
}


